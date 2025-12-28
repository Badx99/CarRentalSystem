using CarRentalSystem.Application.Common.Interfaces;
using CarRentalSystem.Application.Common.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CarRentalSystem.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> emailSettings,
        ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendWelcomeEmailAsync(
        string toEmail,
        string customerName,
        CancellationToken cancellationToken = default)
    {
        var subject = "Welcome to Car Rental System!";
        var body = $@"
            <html>
            <body>
                <h2>Welcome, {customerName}!</h2>
                <p>Thank you for registering with our Car Rental System.</p>
                <p>You can now browse our vehicles and make reservations.</p>
                <p>If you have any questions, feel free to contact us.</p>
                <br/>
                <p>Best regards,<br/>Car Rental Team</p>
            </body>
            </html>";

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task SendReservationConfirmationEmailAsync(
        string toEmail,
        string customerName,
        Guid reservationId,
        DateTime startDate,
        DateTime endDate,
        string vehicleInfo,
        CancellationToken cancellationToken = default)
    {
        var subject = "Reservation Confirmed - Car Rental System";
        var body = $@"
            <html>
            <body>
                <h2>Reservation Confirmed!</h2>
                <p>Dear {customerName},</p>
                <p>Your reservation has been confirmed.</p>
                
                <h3>Reservation Details:</h3>
                <ul>
                    <li><strong>Reservation ID:</strong> {reservationId}</li>
                    <li><strong>Vehicle:</strong> {vehicleInfo}</li>
                    <li><strong>Pick-up Date:</strong> {startDate:yyyy-MM-dd}</li>
                    <li><strong>Return Date:</strong> {endDate:yyyy-MM-dd}</li>
                </ul>
                
                <p>Please arrive at our location at least 15 minutes before your pick-up time.</p>
                <p>Don't forget to bring your driver's license and payment method.</p>
                
                <br/>
                <p>Best regards,<br/>Car Rental Team</p>
            </body>
            </html>";

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task SendReservationCancellationEmailAsync(
        string toEmail,
        string customerName,
        Guid reservationId,
        CancellationToken cancellationToken = default)
    {
        var subject = "Reservation Cancelled - Car Rental System";
        var body = $@"
            <html>
            <body>
                <h2>Reservation Cancelled</h2>
                <p>Dear {customerName},</p>
                <p>Your reservation (ID: {reservationId}) has been cancelled.</p>
                <p>If you did not request this cancellation, please contact us immediately.</p>
                <p>We hope to serve you again in the future.</p>
                <br/>
                <p>Best regards,<br/>Car Rental Team</p>
            </body>
            </html>";

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task SendPaymentConfirmationEmailAsync(
        string toEmail,
        string customerName,
        decimal amount,
        Guid reservationId,
        CancellationToken cancellationToken = default)
    {
        var subject = "Payment Received - Car Rental System";
        var body = $@"
            <html>
            <body>
                <h2>Payment Confirmed!</h2>
                <p>Dear {customerName},</p>
                <p>We have received your payment.</p>
                
                <h3>Payment Details:</h3>
                <ul>
                    <li><strong>Amount:</strong> ${amount:F2}</li>
                    <li><strong>Reservation ID:</strong> {reservationId}</li>
                    <li><strong>Date:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm}</li>
                </ul>
                
                <p>Thank you for your payment!</p>
                
                <br/>
                <p>Best regards,<br/>Car Rental Team</p>
            </body>
            </html>";

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    private async Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _emailSettings.SmtpHost,
                _emailSettings.SmtpPort,
                SecureSocketOptions.StartTls,
                cancellationToken);

            if (!string.IsNullOrEmpty(_emailSettings.Username))
            {
                await client.AuthenticateAsync(
                    _emailSettings.Username,
                    _emailSettings.Password,
                    cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            // Don't throw - email failures shouldn't break the application
        }
    }
}