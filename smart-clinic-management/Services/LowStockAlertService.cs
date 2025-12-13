using Microsoft.Extensions.Logging;

namespace smart_clinic_management.Services;

public class LowStockAlertService : ILowStockAlertService
{
    private readonly ILogger<LowStockAlertService> _logger;

    public LowStockAlertService(ILogger<LowStockAlertService> logger)
    {
        _logger = logger;
    }

    public Task HandleLowStockAsync(string medicineName, int currentQuantity, int threshold)
    {
        _logger.LogWarning("Low stock detected for {Medicine}. Remaining: {Quantity}, Threshold: {Threshold}", medicineName, currentQuantity, threshold);
        return Task.CompletedTask;
    }
}

