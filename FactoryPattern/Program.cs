using System;
using System.Collections.Generic;

namespace FactoryPattern
{
    // ── Message ──────────────────────────────────────────
    class Message
    {
        public string Recipient { get; }
        public string Subject { get; }
        public string Body { get; }

        public Message(string recipient, string subject, string body)
        {
            Recipient = recipient;
            Subject = subject;
            Body = body;
        }
    }

    // ── Interface ─────────────────────────────────────────
    interface INotification
    {
        void Send(Message msg);
    }

    // ── Concrete Products ─────────────────────────────────
    class EmailNotification : INotification
    {
        public void Send(Message msg)
            => Console.WriteLine($"[Email] To: {msg.Recipient} | {msg.Subject}: {msg.Body}");
    }

    class SmsNotification : INotification
    {
        public void Send(Message msg)
            => Console.WriteLine($"[SMS] To: {msg.Recipient} | {msg.Subject}: {msg.Body}");
    }

    class PushNotification : INotification
    {
        public void Send(Message msg)
            => Console.WriteLine($"[Push] To: {msg.Recipient} | {msg.Subject}: {msg.Body}");
    }

    // ── Factory ───────────────────────────────────────────
    static class NotificationFactory
    {
        public static INotification Create(string channel) => channel.ToLower() switch
        {
            "email" => new EmailNotification(),
            "sms" => new SmsNotification(),
            "push" => new PushNotification(),
            _ => throw new ArgumentException($"Unknown channel: {channel}")
        };
    }

    // ── Program ───────────────────────────────────────────
    class Program
    {
        static void Main()
        {
            var msg = new Message("john@example.com", "Alert", "CPU usage exceeded 90%.");

            // Client مش عارف إيه الـ class الفعلي — بس بيطلب من الـ Factory
            foreach (var channel in new[] { "email", "sms", "push" })
            {
                INotification sender = NotificationFactory.Create(channel);
                sender.Send(msg);
            }
        }
    }
}