using System;
using System.IO;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Core.Services
{
    public class EmailService
    {
        private readonly string Server = "smtp.gmail.com";
        private readonly int Port = 465;
        private readonly string Email = "urfu.learn.api@gmail.com";
        private readonly string Password = "8ff7dcce9824491ab985017d5605a241";
        private readonly string Sender = "Почтовая подсистема ВКР";

        public EmailService(IConfiguration configuration)
        {
        }

        public async Task SendEmail(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(Sender, Email));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;

            var body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            var attachment = new MimePart("image", "jpeg")
            {
                Content = new MimeContent(File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "media", "VKRFace.jpg")), ContentEncoding.Default),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = "Новый логотип!"
            };

            var multipart = new Multipart("mixed") {body, attachment};

            emailMessage.Body = multipart;

            using var client = new SmtpClient
            {
                //SslProtocols = SslProtocols.Tls13,
                CheckCertificateRevocation = false
            };
            await client.ConnectAsync(Server, Port, true);
            await client.AuthenticateAsync(Email, Password);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }
}