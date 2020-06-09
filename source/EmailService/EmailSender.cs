using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

namespace EmailService
{
    public class EmailSender
    {
        private readonly Settings settings;

        public EmailSender(Settings settings)
        {
            this.settings = settings;
        }

        public async Task SendInvite(string email, string inviteUrl)
        {
            await Send(email, "Приглашение в группу", inviteUrl).ConfigureAwait(false);
        }

        private async Task Send(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(settings.Sender, settings.Email));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;

            var body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            // var attachment = new MimePart("image", "jpeg")
            // {
            //     Content = new MimeContent(File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "media", "VKRFace.jpg")), ContentEncoding.Default),
            //     ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
            //     ContentTransferEncoding = ContentEncoding.Base64,
            //     FileName = "Новый логотип!"
            // };

            var multipart = new Multipart("mixed") {body}; //attachment

            emailMessage.Body = multipart;

            using var client = new SmtpClient
            {
                //SslProtocols = SslProtocols.Tls13,
                CheckCertificateRevocation = false
            };
            await client.ConnectAsync(settings.Server, settings.Port, true).ConfigureAwait(false);
            await client.AuthenticateAsync(settings.Email, settings.Password).ConfigureAwait(false);
            await client.SendAsync(emailMessage).ConfigureAwait(false);
            await client.DisconnectAsync(true).ConfigureAwait(false);
        }
    }
}