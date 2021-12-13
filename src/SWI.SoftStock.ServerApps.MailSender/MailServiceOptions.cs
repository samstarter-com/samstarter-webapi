using System;

namespace SWI.SoftStock.ServerApps.MailSender
{
    public class MailServiceOptions
    {
        public TimeSpan Interval { get; set; }
        public string BaseAddress { get; set; }
        public SmtpClientOptions SmtpClientOptions { get; set; }
    }

    public class SmtpClientOptions
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}