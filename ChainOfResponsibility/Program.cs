using System;
using System.Collections.Generic;

namespace DiscountChain
{
    // ==========================================
    //  ORDER - the object that travels the chain
    // ==========================================
    class Order
    {
        public int Id { get; }
        public string CustomerType { get; }   // "Regular", "Member", "VIP"
        public double OriginalPrice { get; }
        public double FinalPrice { get; private set; }
        public int Quantity { get; }
        public bool IsSeasonSale { get; }

        public List<string> AppliedDiscounts { get; } = new List<string>();

        public Order(int id, string customerType, double price, int quantity, bool isSeasonSale)
        {
            Id = id;
            CustomerType = customerType;
            OriginalPrice = price;
            FinalPrice = price;
            Quantity = quantity;
            IsSeasonSale = isSeasonSale;
        }

        public void ApplyDiscount(string label, double percent)
        {
            double amount = FinalPrice * (percent / 100.0);
            FinalPrice -= amount;
            AppliedDiscounts.Add($"{label} -{percent}%  (saved ${amount:F2})");
        }
    }

    // ==========================================
    //  BASE HANDLER - abstract chain link
    // ==========================================
    abstract class DiscountHandler
    {
        private DiscountHandler _next;

        // fluent linking: handler1.SetNext(handler2).SetNext(handler3)
        public DiscountHandler SetNext(DiscountHandler next)
        {
            _next = next;
            return next;
        }

        public void Handle(Order order)
        {
            Process(order);       // try to apply this handler's discount
            _next?.Handle(order); // always pass to the next handler
        }

        protected abstract void Process(Order order);
    }

    // ==========================================
    //  CONCRETE HANDLERS
    // ==========================================

    // 1. Season Sale Discount — 10% if it's a seasonal sale
    class SeasonSaleHandler : DiscountHandler
    {
        protected override void Process(Order order)
        {
            if (order.IsSeasonSale)
                order.ApplyDiscount("Season Sale   ", 10);
        }
    }

    // 2. Bulk Discount — 5% if quantity >= 5
    class BulkDiscountHandler : DiscountHandler
    {
        protected override void Process(Order order)
        {
            if (order.Quantity >= 5)
                order.ApplyDiscount("Bulk Purchase ", 5);
        }
    }

    // 3. Member Discount — 8% for Members and VIPs
    class MemberDiscountHandler : DiscountHandler
    {
        protected override void Process(Order order)
        {
            if (order.CustomerType == "Member" || order.CustomerType == "VIP")
                order.ApplyDiscount("Member        ", 8);
        }
    }

    // 4. VIP Discount — extra 15% for VIPs only
    class VipDiscountHandler : DiscountHandler
    {
        protected override void Process(Order order)
        {
            if (order.CustomerType == "VIP")
                order.ApplyDiscount("VIP Exclusive ", 15);
        }
    }

    // 5. Loyalty Discount — 5% if final price is still above $500
    class LoyaltyDiscountHandler : DiscountHandler
    {
        protected override void Process(Order order)
        {
            if (order.FinalPrice > 500)
                order.ApplyDiscount("Loyalty (>500)", 5);
        }
    }

    // ==========================================
    //  PROGRAM
    // ==========================================
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // --- Build the chain once ---
            var chain = new SeasonSaleHandler();
            chain.SetNext(new BulkDiscountHandler())
                 .SetNext(new MemberDiscountHandler())
                 .SetNext(new VipDiscountHandler())
                 .SetNext(new LoyaltyDiscountHandler());

            bool running = true;
            while (running)
            {
                PrintHeader();
                PrintMenu();
                string choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1": RunDemo(chain); break;
                    case "2": RunCustomOrder(chain); break;
                    case "3": ShowChainDiagram(); break;
                    case "4":
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("\n  Goodbye!\n");
                        Console.ResetColor();
                        running = false;
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("  Invalid choice.");
                        Console.ResetColor();
                        break;
                }
            }
        }

        // --- Pre-built demo with 4 different order types ---
        static void RunDemo(DiscountHandler chain)
        {
            var orders = new List<Order>
            {
                new Order(1, "Regular", 200,  2, false),
                new Order(2, "Regular", 800,  6, true),
                new Order(3, "Member",  600,  3, true),
                new Order(4, "VIP",     900, 10, true),
            };

            Console.WriteLine();
            foreach (var order in orders)
            {
                chain.Handle(order);
                PrintOrderResult(order);
            }

            Console.Write("\n  Press Enter to continue...");
            Console.ReadLine();
        }

        // --- Let the user build their own order ---
        static void RunCustomOrder(DiscountHandler chain)
        {
            Console.WriteLine();

            string customerType = PromptChoice(
                "  Customer type:",
                new[] { "Regular", "Member", "VIP" });

            double price = PromptDouble("  Original price ($): ");
            int quantity = PromptInt("  Quantity: ");
            bool season = PromptBool("  Is it a season sale? (y/n): ");

            var order = new Order(99, customerType, price, quantity, season);
            chain.Handle(order);

            Console.WriteLine();
            PrintOrderResult(order);

            Console.Write("\n  Press Enter to continue...");
            Console.ReadLine();
        }

        // --- Visual diagram of the chain ---
        static void ShowChainDiagram()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
  Chain of Responsibility — Discount Pipeline
  ============================================

  Order
    │
    ▼
  ┌─────────────────────────────┐
  │  1. SeasonSaleHandler       │  IsSeasonSale == true  → -10%
  └──────────────┬──────────────┘
                 │ (always passes to next)
                 ▼
  ┌─────────────────────────────┐
  │  2. BulkDiscountHandler     │  Quantity >= 5         → -5%
  └──────────────┬──────────────┘
                 │
                 ▼
  ┌─────────────────────────────┐
  │  3. MemberDiscountHandler   │  Member or VIP         → -8%
  └──────────────┬──────────────┘
                 │
                 ▼
  ┌─────────────────────────────┐
  │  4. VipDiscountHandler      │  VIP only              → -15%
  └──────────────┬──────────────┘
                 │
                 ▼
  ┌─────────────────────────────┐
  │  5. LoyaltyDiscountHandler  │  FinalPrice > $500     → -5%
  └─────────────────────────────┘
                 │
                 ▼
             Final Price
");
            Console.ResetColor();
            Console.Write("  Press Enter to continue...");
            Console.ReadLine();
        }

        // ==========================================
        //  UI HELPERS
        // ==========================================
        static void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(@"
  ╔══════════════════════════════════════════════════╗
  ║     Discount System  -  Chain of Responsibility  ║
  ║     Every handler decides: apply or skip!        ║
  ╚══════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        static void PrintMenu()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(@"
  ┌─────────────────────────────────┐
  │            Main Menu            │
  ├─────────────────────────────────┤
  │  1. Run demo (4 sample orders)  │
  │  2. Create custom order         │
  │  3. Show chain diagram          │
  │  4. Exit                        │
  └─────────────────────────────────┘");
            Console.ResetColor();
            Console.Write("  Choose: ");
        }

        static void PrintOrderResult(Order order)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  ┌─ Order #{order.Id}  [{order.CustomerType}]  Qty: {order.Quantity}  Season: {(order.IsSeasonSale ? "Yes" : "No")}");
            Console.WriteLine($"  │  Original Price : ${order.OriginalPrice:F2}");

            if (order.AppliedDiscounts.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("  │  No discounts applied.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                foreach (var d in order.AppliedDiscounts)
                    Console.WriteLine($"  │  ✔ {d}");
            }

            double saved = order.OriginalPrice - order.FinalPrice;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  │  Final Price    : ${order.FinalPrice:F2}   (total saved: ${saved:F2})");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  └─────────────────────────────────────────────");
            Console.ResetColor();
        }

        static string PromptChoice(string label, string[] options)
        {
            Console.WriteLine(label);
            for (int i = 0; i < options.Length; i++)
                Console.WriteLine($"    {i + 1}. {options[i]}");
            Console.Write("  Choice: ");
            if (int.TryParse(Console.ReadLine(), out int idx) && idx >= 1 && idx <= options.Length)
                return options[idx - 1];
            return options[0];
        }

        static double PromptDouble(string label)
        {
            Console.Write(label);
            return double.TryParse(Console.ReadLine(), out double v) ? v : 0;
        }

        static int PromptInt(string label)
        {
            Console.Write(label);
            return int.TryParse(Console.ReadLine(), out int v) ? v : 1;
        }

        static bool PromptBool(string label)
        {
            Console.Write(label);
            return Console.ReadLine()?.Trim().ToLower() == "y";
        }
    }
}