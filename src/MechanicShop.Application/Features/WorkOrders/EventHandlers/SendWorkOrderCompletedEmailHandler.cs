using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.WorkOrders.Events;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.EventHandlers;

public sealed class SendWorkOrderCompletedEmailHandler : INotificationHandler<WorkOrderCompleted>
{
    private readonly IAppDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly ILogger<SendWorkOrderCompletedEmailHandler> _logger;

    public SendWorkOrderCompletedEmailHandler(
        IAppDbContext dbContext,
        INotificationService notificationService,
        ILogger<SendWorkOrderCompletedEmailHandler> logger)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(WorkOrderCompleted notification, CancellationToken cancellationToken)
    {
        var workOrder = await _dbContext.WorkOrders
            .Include(order => order.Vehicle)
            .FirstOrDefaultAsync(order => order.Id == notification.WorkOrderId, cancellationToken);

        if (workOrder?.Vehicle is null)
        {
            _logger.LogInformation(
                "WorkOrder completed notification skipped. Vehicle not found for WorkOrderId: {WorkOrderId}",
                notification.WorkOrderId);
            return;
        }

        var customer = await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.Id == workOrder.Vehicle.CustomerId, cancellationToken);

        if (customer is null)
        {
            _logger.LogInformation(
                "WorkOrder completed notification skipped. Customer not found for WorkOrderId: {WorkOrderId}",
                notification.WorkOrderId);
            return;
        }

        var payload = BuildNotificationData(notification.WorkOrderId, customer.Name, workOrder.Vehicle.VehicleInfo);

        if (HasValue(customer.Email))
        {
            await _notificationService.SendEmailAsync(
                customer.Email,
                payload.EmailSubject,
                payload.EmailBody,
                cancellationToken);
        }

        if (HasValue(customer.PhoneNumber))
        {
            await _notificationService.SendSmsAsync(
                customer.PhoneNumber,
                payload.SmsBody,
                cancellationToken);
        }
    }

    private static CompletedNotificationData BuildNotificationData(Guid workOrderId, string customerName, string vehicleInfo)
    {
        var emailSubject = $"Work order {workOrderId} completed";
        var emailBody = $"Hello {customerName}, your work order for {vehicleInfo} has been completed.";
        var smsBody = $"Work order {workOrderId} for {vehicleInfo} is completed.";

        return new CompletedNotificationData(emailSubject, emailBody, smsBody);
    }

    private static bool HasValue(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    private sealed record CompletedNotificationData(string EmailSubject, string EmailBody, string SmsBody);
}