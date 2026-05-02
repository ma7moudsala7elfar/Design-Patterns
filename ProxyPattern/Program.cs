using System;
using System.Collections.Generic;
using System.Threading;

namespace VirtualProxyDemo
{
    // ── Interface ───────────────────────────────────────
    interface IFile
    {
        string Name { get; }
        long SizeInMB { get; }
        void Open();
        void Preview();
    }

    // ── Real Object ─────────────────────────────────────
    class RealFile : IFile
    {
        public string Name { get; }
        public long SizeInMB { get; }
        private string _content;

        public RealFile(string name, long sizeInMB)
        {
            Name = name;
            SizeInMB = sizeInMB;

            // simulate loading
            Console.Write($"Loading '{Name}' ({SizeInMB} MB)");
            Thread.Sleep((int)(SizeInMB / 100) * 300);
            _content = $"[Content of {Name}]";
            Console.WriteLine(" Done.");
        }

        public void Open() => Console.WriteLine($"Opening: {_content}");
        public void Preview() => Console.WriteLine($"Preview: first 100 bytes of {Name}...");
    }

    // ── Proxy ────────────────────────────────────────────
    class LazyFileProxy : IFile
    {
        public string Name { get; }
        public long SizeInMB { get; }

        private RealFile _real; // null حتى أول طلب

        public LazyFileProxy(string name, long sizeInMB)
        {
            Name = name;
            SizeInMB = sizeInMB;
        }

        private void EnsureLoaded()
        {
            if (_real == null)
            {
                Console.WriteLine($"[Proxy] First access → loading '{Name}'...");
                _real = new RealFile(Name, SizeInMB);
            }
            else
            {
                Console.WriteLine($"[Proxy] '{Name}' already in memory.");
            }
        }

        public void Open() { EnsureLoaded(); _real.Open(); }
        public void Preview() { EnsureLoaded(); _real.Preview(); }
    }

    // ── Program ───────────────────────────────────────────
    class Program
    {
        static void Main()
        {
            var files = new List<IFile>
            {
                new LazyFileProxy("report.pdf",  500),
                new LazyFileProxy("video.mp4",  1500),
                new LazyFileProxy("photo.png",   200),
            };

            Console.WriteLine("Files registered — nothing loaded yet.\n");

            // أول وصول → بيحمّل
            files[0].Open();

            // تاني وصول لنفس الملف → من الميموري
            files[0].Preview();

            // ملف تاني
            files[2].Preview();
        }
    }
}