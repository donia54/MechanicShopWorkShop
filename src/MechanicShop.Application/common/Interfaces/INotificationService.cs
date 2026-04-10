namespace MechanicShop.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken);

    Task SendSmsAsync(string to, string message, CancellationToken cancellationToken);
}