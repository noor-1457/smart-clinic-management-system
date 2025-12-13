namespace smart_clinic_management.Services;

public interface ILowStockAlertService
{
    Task HandleLowStockAsync(string medicineName, int currentQuantity, int threshold);
}

