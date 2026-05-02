using System;
using System.Collections.Generic;
using System.Threading;

namespace VirtualProxyDemo
{
    // ==========================================
    //  INTERFACE - shared contract for all file types
    // ==========================================
    interface IFile
    {
        string Name { get; }
        string Type { get; }
        long SizeInMB { get; }
        void Open();
        void Preview();
        bool IsLoaded { get; }
    }

    // ==========================================
    //  REAL OBJECT - the actual file
    // ==========================================
    class RealFile : IFile
    {
        public string Name { get; }
        public string Type { get; }
        public long SizeInMB { get; }
        public bool IsLoaded { get; private set; }

        private string _content;

        public RealFile(string name, string type, long sizeInMB)
        {
            Name = name;
            Type = type;
            SizeInMB = sizeInMB;
            LoadFromDisk();
        }

        private void LoadFromDisk()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"  Loading [{Name}] from disk ({SizeInMB} MB)");

            // simulate load time based on file size
            int steps = (int)(SizeInMB / 100);
            for (int i = 0; i < steps; i++)
            {
                Thread.Sleep(300);
                Console.Write(".");
            }

            _content = $"[File content: {Name}]";
            IsLoaded = true;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($" Done!");
            Console.ResetColor();
        }

        public void Open()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  Opening file: {Name} ({Type})");
            Console.WriteLine($"  Content: {_content}");
            Console.ResetColor();
        }

        public void Preview()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  Preview: {Name} - first 100 bytes of content...");
            Console.ResetColor();
        }
    }

    // ==========================================
    //  PROXY - defers loading until first access
    // ==========================================
    class LazyFileProxy : IFile
    {
        public string Name { get; }
        public string Type { get; }
        public long SizeInMB { get; }
        public bool IsLoaded => _realFile?.IsLoaded ?? false;

        private RealFile _realFile; // null initially

        public LazyFileProxy(string name, string type, long sizeInMB)
        {
            Name = name;
            Type = type;
            SizeInMB = sizeInMB;
            // nothing is loaded here
        }

        private void EnsureLoaded()
        {
            if (_realFile == null)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"\n  [Proxy] First request for '{Name}' - loading now...");
                Console.ResetColor();
                _realFile = new RealFile(Name, Type, SizeInMB);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"\n  [Proxy] '{Name}' is already in memory");
                Console.ResetColor();
            }
        }

        public void Open()
        {
            EnsureLoaded();
            _realFile.Open();
        }

        public void Preview()
        {
            EnsureLoaded();
            _realFile.Preview();
        }
    }

    // ==========================================
    //  FILE MANAGER
    // ==========================================
    class FileManager
    {
        private List<IFile> _files = new List<IFile>();

        public void AddFile(IFile file) => _files.Add(file);

        public void ListFiles()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n  ╔══════════════════════════════════════════════╗");
            Console.WriteLine("  ║                  File List                  ║");
            Console.WriteLine("  ╠══╦══════════════════════╦════════╦══════════╣");
            Console.WriteLine("  ║# ║ Name                 ║ Size   ║ Status   ║");
            Console.WriteLine("  ╠══╬══════════════════════╬════════╬══════════╣");

            for (int i = 0; i < _files.Count; i++)
            {
                var f = _files[i];
                string status = f.IsLoaded ? "Loaded" : "Not loaded";
                Console.ForegroundColor = f.IsLoaded ? ConsoleColor.Green : ConsoleColor.Gray;
                Console.WriteLine($"  ║{i + 1} ║ {f.Name,-20} ║ {f.SizeInMB,4} MB ║ {status,-10} ║");
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ╚══╩══════════════════════╩════════╩══════════╝");
            Console.ResetColor();
        }

        public IFile GetFile(int index)
        {
            if (index < 1 || index > _files.Count) return null;
            return _files[index - 1];
        }

        public int Count => _files.Count;
    }

    // ==========================================
    //  PROGRAM - entry point
    // ==========================================
    class Program
    {
        static FileManager manager = new FileManager();

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            PrintHeader();
            SetupFiles();

            bool running = true;
            while (running)
            {
                PrintMenu();
                string input = Console.ReadLine()?.Trim();

                switch (input)
                {
                    case "1": manager.ListFiles(); break;
                    case "2": OpenFile(); break;
                    case "3": PreviewFile(); break;
                    case "4":
                        running = false;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("\n  Goodbye!");
                        Console.ResetColor();
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("  Invalid choice.");
                        Console.ResetColor();
                        break;
                }
            }
        }

        static void SetupFiles()
        {
            // all files are Proxies - nothing is loaded yet
            manager.AddFile(new LazyFileProxy("report_q4.pdf", "PDF", 500));
            manager.AddFile(new LazyFileProxy("promo_video.mp4", "Video", 1500));
            manager.AddFile(new LazyFileProxy("banner.png", "Image", 200));
            manager.AddFile(new LazyFileProxy("dataset.csv", "CSV", 800));

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  4 files registered - none loaded yet (Lazy Loading)\n");
            Console.ResetColor();
        }

        static void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(@"
  ╔══════════════════════════════════════════════════╗
  ║       File Manager  -  Virtual Proxy Demo        ║
  ║       Files are loaded only when requested!      ║
  ╚══════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        static void PrintMenu()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(@"
  ┌────────────────────────────┐
  │          Main Menu         │
  ├────────────────────────────┤
  │  1. List files             │
  │  2. Open a file            │
  │  3. Preview a file         │
  │  4. Exit                   │
  └────────────────────────────┘");
            Console.ResetColor();
            Console.Write("  Choose: ");
        }

        static void OpenFile()
        {
            manager.ListFiles();
            Console.Write($"\n  Enter file number (1-{manager.Count}): ");
            if (int.TryParse(Console.ReadLine(), out int idx))
            {
                var file = manager.GetFile(idx);
                if (file != null) file.Open();
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("  Invalid number.");
                    Console.ResetColor();
                }
            }
        }

        static void PreviewFile()
        {
            manager.ListFiles();
            Console.Write($"\n  Enter file number (1-{manager.Count}): ");
            if (int.TryParse(Console.ReadLine(), out int idx))
            {
                var file = manager.GetFile(idx);
                if (file != null) file.Preview();
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("  Invalid number.");
                    Console.ResetColor();
                }
            }
        }
    }
}