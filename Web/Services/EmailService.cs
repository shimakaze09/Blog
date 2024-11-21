using System.Text;
using MailKit;
using Microsoft.Extensions.Options;
using Share.Utils;

namespace Web.Services;

public class EmailService
{
    private const string BlogLink = "<a href=\"http://localhost:5205\">Blog</a>";
    private readonly EmailAccountConfig _emailAccountConfig;
    private readonly ILogger<EmailService> _logger;


    public EmailService(ILogger<EmailService> logger, IOptions<EmailAccountConfig> options)
    {
        _logger = logger;
        _emailAccountConfig = options.Value;
    }

    public async Task<MessageSentEventArgs> SendEmailAsync(string subject, string body, string toName, string toAddress)
    {
        _logger.LogDebug("Sending email, subject: {Subject}, recipient: {ToAddress}", subject, toAddress);
        body += $"<br><p>This message was automatically sent by {BlogLink}, no need to reply.</p>";
        return await EmailUtils.SendEmailAsync(_emailAccountConfig, subject, body, toName, toAddress);
    }

    /// <summary>
    ///     Sends an email verification code
    ///     <returns>Generates a random verification code</returns>
    ///     <param name="mock">Only generate the verification code, do not send the email</param>
    /// </summary>
    public async Task<string> SendOtpMail(string email, bool mock = false)
    {
        var otp = Random.Shared.NextInt64(1000, 9999).ToString();

        var sb = new StringBuilder();
        sb.AppendLine($"<p>Welcome to Blog! Verification code: {otp}</p>");
        sb.AppendLine("<p>If you did not perform any actions, please ignore this email.</p>");

        if (!mock)
            await SendEmailAsync(
                "[Blog] Email Verification Code",
                sb.ToString(),
                email,
                email
            );

        return otp;
    }
}