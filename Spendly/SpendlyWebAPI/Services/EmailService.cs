using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SpendlyWebAPI.Dal.Repo;

namespace SpendlyWebAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendInviteAsync(string toEmail, string invitationToken)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Spendly", _config["Email:From"]));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "Pozivnica za Spendly grupu";

            var inviteLink = $"{_config["App:BaseUrl"]}/accept-invite?token={invitationToken}";

            message.Body = new TextPart("html")
            {
                Text = $"<p>Pozvani ste u grupu!</p>" +
                       $"<p>Kliknite ovdje: <a href='{inviteLink}'>Prihvati poziv</a></p>"
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(_config["Email:Smtp"], int.Parse(_config["Email:Port"]), SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}