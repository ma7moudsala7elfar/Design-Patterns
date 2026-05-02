using System;
using System.Collections.Generic;

namespace PrototypePattern
{
    // ── Clause ──────────────────────────────────────────
    class Clause
    {
        public string Title { get; set; }
        public string Content { get; set; }

        public Clause(string title, string content)
        {
            Title = title;
            Content = content;
        }

        public Clause Clone() => new Clause(Title, Content);

        public override string ToString() => $"  [{Title}] {Content}";
    }

    // ── Prototype Interface ──────────────────────────────
    interface IPrototype<T>
    {
        T Clone();
    }

    // ── Contract (Base) ──────────────────────────────────
    class Contract : IPrototype<Contract>
    {
        public string Type { get; set; }
        public string PartyA { get; set; }
        public string PartyB { get; set; }
        public decimal Value { get; set; }
        public List<Clause> Clauses { get; private set; } = new List<Clause>();

        public void AddClause(string title, string content)
            => Clauses.Add(new Clause(title, content));

        // Deep Copy
        public Contract Clone()
        {
            var copy = new Contract
            {
                Type = Type,
                PartyA = PartyA,
                PartyB = PartyB,
                Value = Value
            };

            foreach (var c in Clauses)
                copy.Clauses.Add(c.Clone());   // deep copy كل clause

            return copy;
        }

        public void Print()
        {
            Console.WriteLine($"\n=== {Type} ===");
            Console.WriteLine($"Party A : {PartyA}");
            Console.WriteLine($"Party B : {(string.IsNullOrEmpty(PartyB) ? "(not set)" : PartyB)}");
            Console.WriteLine($"Value   : {Value:N0} USD");
            Console.WriteLine($"Clauses :");
            foreach (var c in Clauses)
                Console.WriteLine(c);
        }
    }

    // ── Registry ─────────────────────────────────────────
    class Registry
    {
        private Dictionary<string, Contract> _templates = new();

        public void Register(string key, Contract template)
            => _templates[key] = template;

        public Contract GetClone(string key)
            => _templates[key].Clone();
    }

    // ── Program ───────────────────────────────────────────
    class Program
    {
        static void Main()
        {
            var registry = new Registry();

            // ── بناء الـ Template مرة واحدة ──
            var employment = new Contract { Type = "Employment", PartyA = "TechCorp", Value = 80_000 };
            employment.AddClause("Probation", "90-day probation period.");
            employment.AddClause("Confidentiality", "Must not disclose company info.");
            registry.Register("employment", employment);

            // ── Clone وتخصيص بدون ما نبني من الصفر ──
            var emp1 = registry.GetClone("employment");
            emp1.PartyB = "John Smith";
            emp1.Value = 95_000;
            emp1.Print();

            var emp2 = registry.GetClone("employment");
            emp2.PartyB = "Sara Ali";
            emp2.Value = 70_000;
            emp2.Print();

            // ── إثبات الـ Deep Copy ──
            Console.WriteLine("\n── Deep Copy Test ──");
            emp1.Clauses[0].Content = "*** MODIFIED ***";
            Console.WriteLine("emp1 clause[0] : " + emp1.Clauses[0].Content);
            Console.WriteLine("emp2 clause[0] : " + emp2.Clauses[0].Content); // لازم يفضل زي ما هو
        }
    }
}