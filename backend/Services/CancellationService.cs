using backend.Models;

namespace backend.Services
{
    public interface ICancellationService
    {
        decimal CalculateRefund(decimal totalPaid, DateTime departureTime, bool isOperatorOrAdminCancel);
    }

    public class CancellationService : ICancellationService
    {
        public decimal CalculateRefund(decimal totalPaid, DateTime departureTime, bool isOperatorOrAdminCancel)
        {
            if (isOperatorOrAdminCancel)
                return 0m; // No refund when admin or operator initiates cancellation

            var hoursUntilDeparture = (departureTime - DateTime.UtcNow).TotalHours;

            if (hoursUntilDeparture > 24)
                return totalPaid;          // 100% refund
            if (hoursUntilDeparture >= 12)
                return totalPaid * 0.5m;   // 50% refund
            return 0;                      // No refund
        }
    }
}
