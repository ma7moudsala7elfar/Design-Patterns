using System;
using System.Collections.Generic;

namespace ChainOfResponsibility
{
    // ── Order ────────────────────────────────────────────
    class Order
    {
        public string CustomerType { get; }   // "Regular", "Member", "VIP"
        public double FinalPrice { get; private set; }
        public int Quantity { get; }
        public bool IsSeasonSale { get; }

        public List<string> AppliedDiscounts { get; } = new();

        public Order(string customerType, double price, int quantity, bool isSeasonSale)
        {
            CustomerType = customerType;
            FinalPrice = price;
            Quantity = quantity;
            IsSeasonSale = isSeasonSale;
        }

        public void ApplyDiscount(string label, double percent)
        {
            double saved = FinalPrice * (percent / 100.0);
            FinalPrice -= saved;
            AppliedDiscounts.Add($"{label} -{percent}%  (saved ${saved:F2})");
        }
    }

    // ── Base Handler ──────────────────────────────────────
    abstract class DiscountHandler
    {
        private DiscountHandler _next;

        public DiscountHandler SetNext(DiscountHandler next)
        {
            _next = next;
            return next;        // fluent chaining
        }

        public void Handle(Order order)
        {
            Process(order);       // كل handler يشوف هيطبق الخصم ولا لأ
            _next?.Handle(order); // دايمًا بيعدي للتالي
        }

        protected abstract void Process(Order order);
    }

    // ── Concrete Handlers ─────────────────────────────────
    class SeasonSaleHandler : DiscountHandler
    {
        protected override void Process(Order order)
        {
            if (order.IsSeasonSale) order.ApplyDiscount("Season Sale", 10);
        }
    }

    class BulkHandler : DiscountHandler
    {
        protected override void Process(Order order)
        {
            if (order.Quantity >= 5) order.ApplyDiscount("Bulk (qty>=5)", 5);
        }
    }

    class MemberHandler : DiscountHandler
    {
        protected override void Process(Order order)
        {
            if (order.CustomerType is "Member" or "VIP")
                order.ApplyDiscount("Member", 8);
        }
    }

    class VipHandler : DiscountHandler
    {
        protected override void Process(Order order)
        {
            if (order.CustomerType == "VIP") order.ApplyDiscount("VIP", 15);
        }
    }

    // ── Program ───────────────────────────────────────────
    class Program
    {
        static void Main()
        {
            // بناء الـ chain مرة واحدة
            var chain = new SeasonSaleHandler();
            chain.SetNext(new BulkHandler())
                 .SetNext(new MemberHandler())
                 .SetNext(new VipHandler());

            var orders = new[]
            {
                new Order("Regular", 200,  2, false),
                new Order("Regular", 800,  6, true),
                new Order("Member",  600,  3, true),
                new Order("VIP",     900, 10, true),
            };

            foreach (var order in orders)
            {
                chain.Handle(order);
                PrintResult(order);
            }
        }

        static void PrintResult(Order order)
        {
            Console.WriteLine($"\n[{order.CustomerType}]");
            if (order.AppliedDiscounts.Count == 0)
                Console.WriteLine("  No discounts.");
            else
                foreach (var d in order.AppliedDiscounts)
                    Console.WriteLine($"  + {d}");
            Console.WriteLine($"  Final: ${order.FinalPrice:F2}");
        }
    }
}