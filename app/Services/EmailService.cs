using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MimeKit;

namespace app.Services;

public interface IEmailService
{
    public Task SendEmail(String destinationAddress, String subject, String body);
}

public class EmailServiceOptions
{
    public required String SenderName { get; set; }
    public required String SenderAddress { get; set; }
    public required String SMTPServerAddress { get; set; }
    public required int SMTPPort { get; set; }
    public required String SMTPAuthLogin { get; set; }
    public required String SMTPAuthPassword { get; set; }
}

public class EmailService : IEmailService, IEmailSender
{

    private readonly ILogger<IEmailService> _logger;
    private readonly EmailServiceOptions _options;

    public EmailService(
        ILogger<EmailService> logger,
        IOptions<EmailServiceOptions> options
        )
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task SendEmail(String destinationAddress, String subject, String body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.SenderName, _options.SenderAddress));
        message.To.Add(new MailboxAddress(destinationAddress, destinationAddress));
        message.Subject = subject;

        message.Body = new TextPart("html")
        {
            Text = body
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(_options.SMTPServerAddress, _options.SMTPPort, true);
        await client.AuthenticateAsync(_options.SMTPAuthLogin, _options.SMTPAuthPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task SendEmailAsync(String destinationAddress, String subject, String body)
    {
        await SendEmail(destinationAddress, subject, body);
    }
}

