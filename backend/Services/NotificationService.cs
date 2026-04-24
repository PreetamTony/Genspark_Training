namespace backend.Services
{
    /// <summary>
    /// Mock email/SMS service — logs to console for demo purposes.
    /// In production, swap with SendGrid / Twilio.
    /// </summary>
    public interface INotificationService
    {
        Task SendBookingConfirmationAsync(string email, string userName, int bookingId, string from, string to, DateTime departure, IEnumerable<string> seatNumbers, decimal total);
        Task SendCancellationNoticeAsync(string email, string userName, int bookingId, decimal refundAmount, string reason);
        Task SendRouteCancellationNoticeAsync(string email, string userName, string routeInfo, DateTime departure);
    }

    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public Task SendBookingConfirmationAsync(string email, string userName, int bookingId,
            string from, string to, DateTime departure, IEnumerable<string> seatNumbers, decimal total)
        {
            _logger.LogInformation(
                "[EMAIL MOCK] To: {Email} | Subject: Booking Confirmed #{BookingId}\n" +
                "Dear {User}, your trip from {From} → {To} on {Dep} is confirmed!\n" +
                "Seats: {Seats} | Total Paid: ₹{Total}",
                email, bookingId, userName, from, to,
                departure.ToString("dd MMM yyyy HH:mm"),
                string.Join(", ", seatNumbers), total);
            return Task.CompletedTask;
        }

        public Task SendCancellationNoticeAsync(string email, string userName, int bookingId, decimal refundAmount, string reason)
        {
            _logger.LogInformation(
                "[EMAIL MOCK] To: {Email} | Subject: Booking #{BookingId} Cancelled\n" +
                "Dear {User}, your booking has been cancelled. Reason: {Reason}. Refund: ₹{Refund}",
                email, bookingId, userName, reason, refundAmount);
            return Task.CompletedTask;
        }

        public Task SendRouteCancellationNoticeAsync(string email, string userName, string routeInfo, DateTime departure)
        {
            _logger.LogInformation(
                "[EMAIL MOCK] To: {Email} | Subject: Route Cancelled\n" +
                "Dear {User}, the trip ({Route}) on {Dep} has been cancelled. Please contact support.",
                email, userName, routeInfo, departure.ToString("dd MMM yyyy HH:mm"));
            return Task.CompletedTask;
        }
    }
}
