using System;
using System.Collections.Generic;

namespace ContractPrototype
{
    // ==========================================
    //  CLAUSE - a single section inside a contract
    //  needs its own Clone (Deep Copy)
    // ==========================================
    class Clause
    {
        public string Title { get; set; }
        public string Content { get; set; }

        public Clause(string title, string content)
        {
            Title = title;
            Content = content;
        }

        // deep copy of the clause itself
        public Clause Clone() => new Clause(Title, Content);

        public override string ToString() => $"    [{Title}]\n    {Content}";
    }

    // ==========================================
    //  PROTOTYPE INTERFACE
    // ==========================================
    interface IContractPrototype
    {
        IContractPrototype Clone();
        void Print();
    }

    // ==========================================
    //  BASE CONTRACT
    // ==========================================
    abstract class Contract : IContractPrototype
    {
        public string ContractType { get; protected set; }
        public string PartyA { get; set; }   // company
        public string PartyB { get; set; }   // client / employee
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Value { get; set; }
        public string Currency { get; set; }
        public string Jurisdiction { get; set; }
        public List<Clause> Clauses { get; private set; }
        public string Status { get; set; }   // "Template", "Draft", "Final"

        protected Contract()
        {
            Clauses = new List<Clause>();
            Currency = "USD";
            Status = "Template";
        }

        // deep copy: copies all clauses as new objects
        protected void CopyTo(Contract target)
        {
            target.ContractType = ContractType;
            target.PartyA = PartyA;
            target.PartyB = PartyB;
            target.StartDate = StartDate;
            target.EndDate = EndDate;
            target.Value = Value;
            target.Currency = Currency;
            target.Jurisdiction = Jurisdiction;
            target.Status = "Draft";

            target.Clauses = new List<Clause>();
            foreach (var clause in Clauses)
                target.Clauses.Add(clause.Clone()); // deep copy each clause
        }

        public abstract IContractPrototype Clone();

        public void AddClause(string title, string content)
            => Clauses.Add(new Clause(title, content));

        public void Print()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\n  ╔══════════════════════════════════════════════════╗");
            Console.WriteLine($"  ║  {ContractType,-48}║");
            Console.WriteLine($"  ╠══════════════════════════════════════════════════╣");
            Console.ResetColor();

            PrintField("Status", Status);
            PrintField("Party A", string.IsNullOrEmpty(PartyA) ? "(not set)" : PartyA);
            PrintField("Party B", string.IsNullOrEmpty(PartyB) ? "(not set)" : PartyB);
            PrintField("Start Date", StartDate == default ? "(not set)" : StartDate.ToString("yyyy-MM-dd"));
            PrintField("End Date", EndDate == default ? "(not set)" : EndDate.ToString("yyyy-MM-dd"));
            PrintField("Value", Value == 0 ? "(not set)" : $"{Value:N0} {Currency}");
            PrintField("Jurisdiction", string.IsNullOrEmpty(Jurisdiction) ? "(not set)" : Jurisdiction);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n  Clauses ({Clauses.Count}):");
            Console.ForegroundColor = ConsoleColor.Gray;
            foreach (var c in Clauses)
                Console.WriteLine(c);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ╚══════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        private void PrintField(string label, string value)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"  ║  {label,-13}: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(value);
            Console.ResetColor();
        }
    }

    // ==========================================
    //  CONCRETE CONTRACTS
    // ==========================================

    class EmploymentContract : Contract
    {
        public string JobTitle { get; set; }
        public int WorkingHours { get; set; }

        public EmploymentContract()
        {
            ContractType = "EMPLOYMENT CONTRACT";
            Jurisdiction = "New York, USA";
            WorkingHours = 40;
        }

        public override IContractPrototype Clone()
        {
            var copy = new EmploymentContract();
            CopyTo(copy);
            copy.JobTitle = JobTitle;
            copy.WorkingHours = WorkingHours;
            return copy;
        }

        public new void Print()
        {
            base.Print();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"  Job Title: {JobTitle}   Working Hours/Week: {WorkingHours}h");
            Console.ResetColor();
        }
    }

    class FreelanceContract : Contract
    {
        public string ProjectName { get; set; }
        public string PaymentSchedule { get; set; }  // "Milestone", "Monthly", "OnCompletion"

        public FreelanceContract()
        {
            ContractType = "FREELANCE CONTRACT";
            Jurisdiction = "Delaware, USA";
            PaymentSchedule = "Milestone";
        }

        public override IContractPrototype Clone()
        {
            var copy = new FreelanceContract();
            CopyTo(copy);
            copy.ProjectName = ProjectName;
            copy.PaymentSchedule = PaymentSchedule;
            return copy;
        }

        public new void Print()
        {
            base.Print();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"  Project: {ProjectName}   Payment: {PaymentSchedule}");
            Console.ResetColor();
        }
    }

    class NdaContract : Contract
    {
        public string ConfidentialityScope { get; set; }
        public int PenaltyAmount { get; set; }

        public NdaContract()
        {
            ContractType = "NON-DISCLOSURE AGREEMENT (NDA)";
            Jurisdiction = "California, USA";
            ConfidentialityScope = "All proprietary business information";
        }

        public override IContractPrototype Clone()
        {
            var copy = new NdaContract();
            CopyTo(copy);
            copy.ConfidentialityScope = ConfidentialityScope;
            copy.PenaltyAmount = PenaltyAmount;
            return copy;
        }

        public new void Print()
        {
            base.Print();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"  Scope: {ConfidentialityScope}   Penalty: ${PenaltyAmount:N0}");
            Console.ResetColor();
        }
    }

    // ==========================================
    //  TEMPLATE REGISTRY - stores master templates
    // ==========================================
    class ContractRegistry
    {
        private Dictionary<string, Contract> _templates = new Dictionary<string, Contract>();

        public void Register(string key, Contract template)
        {
            _templates[key] = template;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  [Registry] Template registered: '{key}'");
            Console.ResetColor();
        }

        public Contract GetClone(string key)
        {
            if (!_templates.ContainsKey(key))
                throw new ArgumentException($"Template '{key}' not found.");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  [Prototype] Cloning template '{key}'...");
            Console.ResetColor();

            return (Contract)_templates[key].Clone();
        }

        public void ListTemplates()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n  Registered Templates:");
            Console.ForegroundColor = ConsoleColor.Cyan;
            foreach (var key in _templates.Keys)
                Console.WriteLine($"    • {key}  →  {_templates[key].ContractType}");
            Console.ResetColor();
        }
    }

    // ==========================================
    //  PROGRAM
    // ==========================================
    class Program
    {
        static ContractRegistry registry = new ContractRegistry();

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            SetupTemplates();

            bool running = true;
            while (running)
            {
                PrintHeader();
                PrintMenu();
                string choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1": RunDemo(); break;
                    case "2": CreateCustomContract(); break;
                    case "3": ShowTemplates(); break;
                    case "4": ProveDeepCopy(); break;
                    case "5":
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

        // --- build master templates once ---
        static void SetupTemplates()
        {
            Console.WriteLine();

            // Employment template
            var employment = new EmploymentContract
            {
                PartyA = "TechCorp Inc.",
                StartDate = new DateTime(2025, 1, 1),
                EndDate = new DateTime(2026, 1, 1),
                Value = 80000,
                JobTitle = "Software Engineer",
                WorkingHours = 40
            };
            employment.AddClause("Probation Period", "Employee is subject to a 90-day probation period.");
            employment.AddClause("Confidentiality", "Employee must not disclose company information.");
            employment.AddClause("Termination", "Either party may terminate with 30 days written notice.");
            registry.Register("employment", employment);

            // Freelance template
            var freelance = new FreelanceContract
            {
                PartyA = "TechCorp Inc.",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(6),
                Value = 15000,
                ProjectName = "Web Platform Redesign",
                PaymentSchedule = "Milestone"
            };
            freelance.AddClause("Deliverables", "Contractor must submit deliverables per agreed milestones.");
            freelance.AddClause("IP Ownership", "All work produced is property of the client upon full payment.");
            freelance.AddClause("Revisions", "Client is entitled to 2 rounds of revisions per milestone.");
            registry.Register("freelance", freelance);

            // NDA template
            var nda = new NdaContract
            {
                PartyA = "TechCorp Inc.",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(2),
                PenaltyAmount = 50000
            };
            nda.AddClause("Scope", "Covers all trade secrets, source code, and business strategies.");
            nda.AddClause("Exceptions", "Does not apply to publicly available information.");
            nda.AddClause("Enforcement", "Breach results in immediate legal action and financial penalties.");
            registry.Register("nda", nda);

            Console.WriteLine();
        }

        // --- demo: clone all 3 templates and customize ---
        static void RunDemo()
        {
            Console.WriteLine();

            // Clone 1: Employment for a new hire
            var emp = (EmploymentContract)registry.GetClone("employment");
            emp.PartyB = "John Smith";
            emp.Value = 95000;
            emp.JobTitle = "Senior Developer";
            emp.StartDate = new DateTime(2025, 3, 1);
            emp.Print();

            // Clone 2: Freelance for a new project
            var free = (FreelanceContract)registry.GetClone("freelance");
            free.PartyB = "Sara Design Studio";
            free.Value = 22000;
            free.ProjectName = "Mobile App UI";
            free.PaymentSchedule = "Monthly";
            free.Print();

            // Clone 3: NDA for a partner
            var ndaClone = (NdaContract)registry.GetClone("nda");
            ndaClone.PartyB = "PartnerCorp LLC";
            ndaClone.PenaltyAmount = 100000;
            ndaClone.EndDate = DateTime.Today.AddYears(3);
            ndaClone.Print();

            Pause();
        }

        // --- user builds a custom contract from a template ---
        static void CreateCustomContract()
        {
            Console.WriteLine();
            registry.ListTemplates();

            string key = PromptString("\n  Enter template key (employment / freelance / nda): ").ToLower();

            Contract contract;
            try { contract = registry.GetClone(key); }
            catch (ArgumentException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  Error: {ex.Message}");
                Console.ResetColor();
                Pause();
                return;
            }

            Console.WriteLine();
            contract.PartyB = PromptString("  Client / Employee name : ");
            string valueStr = PromptString("  Contract value ($)     : ");
            if (decimal.TryParse(valueStr, out decimal val)) contract.Value = val;

            string startStr = PromptString("  Start date (yyyy-MM-dd): ");
            if (DateTime.TryParse(startStr, out DateTime start)) contract.StartDate = start;

            contract.Status = "Final";
            contract.Print();
            Pause();
        }

        // --- show all templates as-is ---
        static void ShowTemplates()
        {
            Console.WriteLine();
            foreach (var key in new[] { "employment", "freelance", "nda" })
            {
                var t = registry.GetClone(key);
                t.Print();
            }
            Pause();
        }

        // --- prove that clone is independent from template ---
        static void ProveDeepCopy()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  Proving Deep Copy — modifying Clone should NOT affect Template\n");
            Console.ResetColor();

            var original = registry.GetClone("employment");
            var clone = (Contract)original.Clone();

            // mutate the clone's clause
            clone.Clauses[0].Content = "*** MODIFIED BY CLONE ***";
            clone.PartyB = "Clone Employee";
            clone.Value = 999999;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  --- CLONE (after modification) ---");
            Console.ResetColor();
            clone.Print();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  --- ORIGINAL (should be unchanged) ---");
            Console.ResetColor();
            original.Print();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n  Original clause is intact — Deep Copy confirmed! ✔");
            Console.ResetColor();
            Pause();
        }

        // ==========================================
        //  UI HELPERS
        // ==========================================
        static void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(@"
  ╔══════════════════════════════════════════════════╗
  ║   Contract System  -  Prototype Pattern          ║
  ║   Clone a template, customize, never rebuild!    ║
  ╚══════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        static void PrintMenu()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(@"
  ┌──────────────────────────────────────────┐
  │                 Main Menu                │
  ├──────────────────────────────────────────┤
  │  1. Demo (clone all 3 templates)         │
  │  2. Create custom contract from template │
  │  3. Show master templates                │
  │  4. Prove deep copy independence         │
  │  5. Exit                                 │
  └──────────────────────────────────────────┘");
            Console.ResetColor();
            Console.Write("  Choose: ");
        }

        static void Pause()
        {
            Console.Write("\n  Press Enter to continue...");
            Console.ReadLine();
        }

        static string PromptString(string label)
        {
            Console.Write(label);
            return Console.ReadLine()?.Trim() ?? "";
        }
    }
}