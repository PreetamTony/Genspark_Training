using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using backend.Models;

namespace NexBus.Services
{
    public interface IEmailService
    {
        Task SendBookingConfirmationEmailAsync(Booking booking, List<BookingSeat> bookingSeats, string userEmail, string userName);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPass;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            
            // SMTP Configuration
            _smtpHost = "smtp.gmail.com";
            _smtpPort = 587;
            _smtpUser = "nexbus4u@gmail.com";
            _smtpPass = "nexbus19285";
            _fromEmail = "nexbus4u@gmail.com";
            _fromName = "NexBus";
        }

        public async Task SendBookingConfirmationEmailAsync(Booking booking, List<BookingSeat> bookingSeats, string userEmail, string userName)
        {
            try
            {
                using var client = new SmtpClient(_smtpHost, _smtpPort)
                {
                    Credentials = new NetworkCredential(_smtpUser, _smtpPass),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_fromEmail, _fromName),
                    Subject = $"NexBus Booking Confirmation - #{booking.Id}",
                    Body = GenerateEmailBody(booking, bookingSeats, userName),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(userEmail);

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Log error silently for debugging (remove in production)
                // Check if it's an authentication error
                if (ex.Message.Contains("Authentication Required") || ex.Message.Contains("5.7.0"))
                {
                    // Gmail SMTP authentication failed - will fallback to simulated email
                }
                
                throw;
            }
        }

        private string GenerateEmailBody(Booking booking, List<BookingSeat> bookingSeats, string userName)
        {
            var bookingDate = booking.BookingDate.ToString("dd MMM yyyy");
            var travelDate = booking.Schedule?.DepartureTime.ToString("dd MMM yyyy") ?? "Not specified";
            var departureTime = booking.Schedule?.DepartureTime.ToString("hh:mm tt") ?? "Not specified";
            var arrivalTime = booking.Schedule?.ArrivalTime.ToString("hh:mm tt") ?? "Not specified";
            var route = $"{booking.Schedule?.Route?.Source?.Name} → {booking.Schedule?.Route?.Destination?.Name}";
            var busOperator = booking.Schedule?.Bus?.Operator?.CompanyName ?? "NexBus";
            var busType = booking.Schedule?.Bus?.BusType ?? "Not specified";
            var seatNumbers = bookingSeats.Select(bs => bs.Seat?.SeatNumber).ToList();
            
            var seatNumbersStr = string.Join(", ", seatNumbers);
            var totalAmount = booking.TotalPrice.ToString("F2");
            var convenienceFee = booking.ConvenienceFee.ToString("F2");
            var baseFare = (booking.TotalPrice - booking.ConvenienceFee).ToString("F2");

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>NexBus Booking Confirmation</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #6366f1, #8b5cf6); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9fafb; padding: 30px; border-radius: 0 0 10px 10px; }}
        .booking-info {{ background: white; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #6366f1; }}
        .trip-details {{ background: white; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #10b981; }}
        .seat-info {{ background: white; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #f59e0b; }}
        .price-info {{ background: white; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #ef4444; }}
        .footer {{ text-align: center; margin-top: 30px; color: #6b7280; font-size: 14px; }}
        h1 {{ margin: 0; font-size: 28px; }}
        h2 {{ color: #1f2937; margin-top: 0; }}
        .label {{ font-weight: bold; color: #374151; }}
        .value {{ color: #6b7280; }}
        .highlight {{ background: #fef3c7; padding: 2px 6px; border-radius: 4px; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>🚌 NexBus Booking Confirmed!</h1>
        <p>Your journey details are ready</p>
    </div>
    
    <div class='content'>
        <div class='booking-info'>
            <h2>📋 Booking Information</h2>
            <p><span class='label'>Booking ID:</span> <span class='value highlight'>#{booking.Id}</span></p>
            <p><span class='label'>Booking Date:</span> <span class='value'>{bookingDate}</span></p>
            <p><span class='label'>Passenger Name:</span> <span class='value'>{userName}</span></p>
        </div>

        <div class='trip-details'>
            <h2>🛣️ Trip Details</h2>
            <p><span class='label'>Route:</span> <span class='value'>{route}</span></p>
            <p><span class='label'>Travel Date:</span> <span class='value'>{travelDate}</span></p>
            <p><span class='label'>Departure Time:</span> <span class='value'>{departureTime}</span></p>
            <p><span class='label'>Arrival Time:</span> <span class='value'>{arrivalTime}</span></p>
            <p><span class='label'>Bus Operator:</span> <span class='value'>{busOperator}</span></p>
            <p><span class='label'>Bus Type:</span> <span class='value'>{busType}</span></p>
        </div>

        <div class='seat-info'>
            <h2>🪑 Seat Information</h2>
            <p><span class='label'>Seat Numbers:</span> <span class='value highlight'>{seatNumbersStr}</span></p>
            <p><span class='label'>Total Seats:</span> <span class='value'>{bookingSeats.Count}</span></p>
        </div>

        <div class='price-info'>
            <h2>💰 Payment Details</h2>
            <p><span class='label'>Base Fare:</span> <span class='value'>₹{baseFare}</span></p>
            <p><span class='label'>Convenience Fee:</span> <span class='value'>₹{convenienceFee}</span></p>
            <p><span class='label'>Total Amount Paid:</span> <span class='value' style='font-size: 18px; font-weight: bold; color: #059669;'>₹{totalAmount}</span></p>
        </div>

        <div style='background: #fef3c7; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #f59e0b;'>
            <h3 style='margin-top: 0; color: #92400e;'>📝 Important Information</h3>
            <ul style='margin: 10px 0; padding-left: 20px;'>
                <li>Please arrive at the boarding point at least 15 minutes before departure</li>
                <li>Carry a valid ID proof for verification</li>
                <li>Show this booking confirmation at the boarding point</li>
                <li>For any queries, contact our customer support</li>
            </ul>
        </div>
    </div>

    <div class='footer'>
        <p><strong>Thank you for choosing NexBus! 🚌</strong></p>
        <p>Have a safe and comfortable journey</p>
        <p style='font-size: 12px; margin-top: 20px;'>
            This is an automated confirmation email. Please do not reply to this message.
        </p>
    </div>
</body>
</html>";
        }
    }
}
