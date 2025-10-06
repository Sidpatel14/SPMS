using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var emailSettings = _config.GetSection("EmailSettings");

        string? smtpServer = emailSettings["SmtpServer"];
        string? portString = emailSettings["Port"];
        string? username = emailSettings["Username"];
        string? password = emailSettings["Password"];
        string? enableSslString = emailSettings["EnableSSL"];
        string? senderEmail = emailSettings["SenderEmail"];
        string? senderName = emailSettings["SenderName"];

        if (smtpServer == null)
            throw new InvalidOperationException("SMTP server is not configured.");
        if (portString == null)
            throw new InvalidOperationException("SMTP port is not configured.");
        if (username == null)
            throw new InvalidOperationException("SMTP username is not configured.");
        if (password == null)
            throw new InvalidOperationException("SMTP password is not configured.");
        if (enableSslString == null)
            throw new InvalidOperationException("SMTP EnableSSL is not configured.");
        if (senderEmail == null)
            throw new InvalidOperationException("Sender email is not configured.");
        if (senderName == null)
            throw new InvalidOperationException("Sender name is not configured.");

        using (var client = new SmtpClient(smtpServer, int.Parse(portString)))
        {
            client.Credentials = new NetworkCredential(username, password);
            client.EnableSsl = bool.Parse(enableSslString);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}
