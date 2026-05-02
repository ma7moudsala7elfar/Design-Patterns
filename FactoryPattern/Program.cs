using System;
using System.Collections.Generic;

namespace NotificationFactory
{
    // ==========================================
    //  NOTIFICATION MESSAGE - travels to sender
    // ==========================================
    class NotificationMessage
    {
        public string Recipient { get; }
        public string Subject { get; }
        public string Body { get; }
        public string Priority { get; }   // "Low", "Normal", "High"

        public NotificationMessage(string recipient, string subject, string body, string priority = "Normal")
        {
            Recipient = recipient;
            Subject = subject;
            Body = body;
            Priority = priority;
        }
    }

    // ==========================================
    //  INTERFACE - shared contract for all senders
    // ==========================================
    interface INotification
    {
        string Channel { get; }   // "Email", "SMS", "Push", "Slack"
        string Icon { get; }
        void Send(NotificationMessage message);
        void ShowConfig();
    }

    // ==========================================
    //  CONCRETE PRODUCTS
    // ==========================================

    class EmailNotification : INotification
    {
        public string Channel => "Email";
        public string Icon => "📧";

        private string _smtpServer;
        private int _port;

        public EmailNotification(string smtpServer, int port)
        {
            _smtpServer = smtpServer;
            _port = port;
        }

        public void Send(NotificationMessage msg)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  {Icon} [EMAIL] To: {msg.Recipient}");
            Console.WriteLine($"       Subject : {msg.Subject}");
            Console.WriteLine($"       Body    : {msg.Body}");
            Console.WriteLine($"       Priority: {msg.Priority}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"       Status  : Sent via {_smtpServer}:{_port} ✔");
            Console.ResetColor();
        }

        public void ShowConfig()
        {
            Console.WriteLine($"  {Icon} Email  →  SMTP: {_smtpServer}  Port: {_port}");
        }
    }

    class SmsNotification : INotification
    {
        public string Channel => "SMS";
        public string Icon => "📱";

        private string _apiKey;
        private string _senderNumber;

        public SmsNotification(string apiKey, string senderNumber)
        {
            _apiKey = apiKey;
            _senderNumber = senderNumber;
        }

        public void Send(NotificationMessage msg)
        {
            // SMS: subject + first 160 chars of body
            string smsText = $"{msg.Subject}: {msg.Body}";
            if (smsText.Length > 160) smsText = smsText.Substring(0, 157) + "...";

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  {Icon} [SMS] To: {msg.Recipient}");
            Console.WriteLine($"       Text    : {smsText}");
            Console.WriteLine($"       Priority: {msg.Priority}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"       Status  : Sent from {_senderNumber} ✔");
            Console.ResetColor();
        }

        public void ShowConfig()
        {
            Console.WriteLine($"  {Icon} SMS    →  Sender: {_senderNumber}  Key: {_apiKey[..6]}***");
        }
    }

    class PushNotification : INotification
    {
        public string Channel => "Push";
        public string Icon => "🔔";

        private string _appId;
        private string _platform;   // "iOS", "Android", "Web"

        public PushNotification(string appId, string platform)
        {
            _appId = appId;
            _platform = platform;
        }

        public void Send(NotificationMessage msg)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"  {Icon} [PUSH] To device: {msg.Recipient}");
            Console.WriteLine($"       Title   : {msg.Subject}");
            Console.WriteLine($"       Body    : {msg.Body}");
            Console.WriteLine($"       Priority: {msg.Priority}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"       Status  : Pushed via {_platform} ({_appId}) ✔");
            Console.ResetColor();
        }

        public void ShowConfig()
        {
            Console.WriteLine($"  {Icon} Push   →  Platform: {_platform}  AppId: {_appId}");
        }
    }

    class SlackNotification : INotification
    {
        public string Channel => "Slack";
        public string Icon => "💬";

        private string _webhookUrl;
        private string _slackChannel;

        public SlackNotification(string webhookUrl, string slackChannel)
        {
            _webhookUrl = webhookUrl;
            _slackChannel = slackChannel;
        }

        public void Send(NotificationMessage msg)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"  {Icon} [SLACK] Channel: #{_slackChannel}");
            Console.WriteLine($"       From    : {msg.Recipient}");
            Console.WriteLine($"       *{msg.Subject}*");
            Console.WriteLine($"       {msg.Body}");
            Console.WriteLine($"       Priority: {msg.Priority}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"       Status  : Posted to #{_slackChannel} ✔");
            Console.ResetColor();
        }

        public void ShowConfig()
        {
            Console.WriteLine($"  {Icon} Slack  →  Channel: #{_slackChannel}  Webhook: {_webhookUrl[..20]}...");
        }
    }

    // ==========================================
    //  FACTORY - the core of the pattern
    // ==========================================
    static class NotificationFactory
    {
        public static INotification Create(string channel)
        {
            switch (channel.ToLower())
            {
                case "email":
                    return new EmailNotification("smtp.mailserver.com", 587);

                case "sms":
                    return new SmsNotification("SK_LIVE_ABC123XYZ", "+1-800-555-0100");

                case "push":
                    return new PushNotification("app_notify_prod", "Android");

                case "slack":
                    return new SlackNotification("https://hooks.slack.com/services/T00/B00/xyz", "alerts");

                default:
                    throw new ArgumentException($"Unknown channel: '{channel}'. Valid: email, sms, push, slack");
            }
        }

        // create multiple senders at once
        public static List<INotification> CreateMultiple(params string[] channels)
        {
            var list = new List<INotification>();
            foreach (var ch in channels)
                list.Add(Create(ch));
            return list;
        }
    }

    // ==========================================
    //  NOTIFICATION SERVICE - uses the factory
    // ==========================================
    class NotificationService
    {
        // single channel
        public void Notify(string channel, NotificationMessage message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"\n  [Factory] Creating '{channel}' sender...");
            Console.ResetColor();

            INotification sender = NotificationFactory.Create(channel);
            sender.Send(message);
        }

        // broadcast to multiple channels
        public void Broadcast(string[] channels, NotificationMessage message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"\n  [Factory] Creating {channels.Length} senders for broadcast...");
            Console.ResetColor();

            var senders = NotificationFactory.CreateMultiple(channels);
            foreach (var sender in senders)
            {
                Console.WriteLine();
                sender.Send(message);
            }
        }
    }

    // ==========================================
    //  PROGRAM
    // ==========================================
    class Program
    {
        static NotificationService service = new NotificationService();

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            bool running = true;
            while (running)
            {
                PrintHeader();
                PrintMenu();
                string choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1": RunDemo(); break;
                    case "2": RunCustomSend(); break;
                    case "3": RunBroadcast(); break;
                    case "4": ShowFactoryInfo(); break;
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

        // --- demo: one message sent via each channel ---
        static void RunDemo()
        {
            Console.WriteLine();
            string[] channels = { "email", "sms", "push", "slack" };

            foreach (var ch in channels)
            {
                var msg = new NotificationMessage(
                    recipient: ch == "slack" ? "dev-team" : "user@example.com",
                    subject: "System Alert",
                    body: "CPU usage exceeded 90% on server-01.",
                    priority: "High"
                );
                service.Notify(ch, msg);
                PrintDivider();
            }

            Pause();
        }

        // --- user picks channel and writes message ---
        static void RunCustomSend()
        {
            Console.WriteLine();
            string channel = PromptChoice("  Channel:", new[] { "email", "sms", "push", "slack" });
            string recipient = PromptString("  Recipient: ");
            string subject = PromptString("  Subject  : ");
            string body = PromptString("  Body     : ");
            string priority = PromptChoice("  Priority:", new[] { "Low", "Normal", "High" });

            var msg = new NotificationMessage(recipient, subject, body, priority);

            Console.WriteLine();
            try
            {
                service.Notify(channel, msg);
            }
            catch (ArgumentException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  Error: {ex.Message}");
                Console.ResetColor();
            }

            Pause();
        }

        // --- broadcast same message to all channels ---
        static void RunBroadcast()
        {
            Console.WriteLine();
            Console.WriteLine("  Broadcast: same message → all channels\n");

            var msg = new NotificationMessage(
                recipient: "all-users",
                subject: "Scheduled Maintenance",
                body: "The system will be down on Sunday 2-4 AM for maintenance.",
                priority: "Normal"
            );

            service.Broadcast(new[] { "email", "sms", "push", "slack" }, msg);
            Pause();
        }

        // --- show what the factory can create ---
        static void ShowFactoryInfo()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(@"
  ╔══════════════════════════════════════════════════════╗
  ║               Factory — Available Channels           ║
  ╚══════════════════════════════════════════════════════╝
");
            Console.ResetColor();

            var channels = new[] { "email", "sms", "push", "slack" };
            foreach (var ch in channels)
            {
                var sender = NotificationFactory.Create(ch);
                Console.ForegroundColor = ConsoleColor.White;
                sender.ShowConfig();
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(@"
  How it works:
  ┌──────────────┐     Create(""email"")     ┌─────────────────────┐
  │   Client     │ ─────────────────────► │  NotificationFactory │
  │  (Service)   │                        │                      │
  │              │ ◄───────────────────── │  returns INotification│
  └──────────────┘    INotification       └─────────────────────┘
         │
         │  .Send(message)
         ▼
  ┌──────────────────────────────────────┐
  │  EmailNotification / SmsNotification │
  │  PushNotification  / SlackNotif...   │
  └──────────────────────────────────────┘
");
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
  ║    Notification System  -  Factory Pattern       ║
  ║    The factory decides which sender to create!   ║
  ╚══════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        static void PrintMenu()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(@"
  ┌──────────────────────────────────────┐
  │               Main Menu              │
  ├──────────────────────────────────────┤
  │  1. Demo (all 4 channels)            │
  │  2. Send custom notification         │
  │  3. Broadcast to all channels        │
  │  4. Show factory info                │
  │  5. Exit                             │
  └──────────────────────────────────────┘");
            Console.ResetColor();
            Console.Write("  Choose: ");
        }

        static void PrintDivider()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ──────────────────────────────────────────────");
            Console.ResetColor();
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
    }
}