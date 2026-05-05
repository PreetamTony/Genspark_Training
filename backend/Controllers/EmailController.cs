using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using NexBus.Services;
using RouteModel = backend.Models.Route;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NexBus.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public EmailController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost("send-booking-confirmation")]
        public async Task<IActionResult> SendBookingConfirmation([FromBody] SendBookingConfirmationRequest request)
        {
            try
            {
                // Get the booking with all related data
                var booking = await _context.Bookings
                    .Include(b => b.Schedule)
                        .ThenInclude(s => s.Route)
                        .ThenInclude(r => r.Source)
                    .Include(b => b.Schedule)
                        .ThenInclude(s => s.Route)
                        .ThenInclude(r => r.Destination)
                    .Include(b => b.Schedule)
                        .ThenInclude(s => s.Bus)
                        .ThenInclude(bus => bus.Operator)
                    .Include(b => b.BookingSeats)
                        .ThenInclude(bs => bs.Seat)
                    .FirstOrDefaultAsync(b => b.Id == request.BookingId);

                if (booking == null)
                {
                    return NotFound(new { success = false, message = "Booking not found" });
                }

                // Send the confirmation email
                await _emailService.SendBookingConfirmationEmailAsync(
                    booking, 
                    booking.BookingSeats.ToList(), 
                    request.UserEmail, 
                    request.UserName
                );

                return Ok(new { 
                    success = true, 
                    message = "Booking confirmation email sent successfully",
                    bookingId = booking.Id,
                    emailSentTo = request.UserEmail,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = "Failed to send booking confirmation email",
                    error = ex.Message
                });
            }
        }

        [HttpPost("test-email")]
        public async Task<IActionResult> SendTestEmail([FromBody] TestEmailRequest request)
        {
            try
            {
                // Create a mock booking for testing
                var mockBooking = new Booking
                {
                    Id = 999999,
                    BookingDate = DateTime.UtcNow,
                    TotalPrice = 550.00m,
                    ConvenienceFee = 50.00m,
                    Schedule = new Schedule
                    {
                        DepartureTime = DateTime.Now.AddDays(1),
                        ArrivalTime = DateTime.Now.AddDays(1).AddHours(6),
                        Route = new RouteModel 
                        { 
                            Source = new Location { Name = "Chennai" },
                            Destination = new Location { Name = "Bangalore" }
                        },
                        Bus = new Bus 
                        { 
                            BusType = "Volvo AC Sleeper",
                            Operator = new OperatorProfile { CompanyName = "NexBus Travels" }
                        }
                    },
                    BookingSeats = new List<BookingSeat>
                    {
                        new BookingSeat { Seat = new Seat { SeatNumber = "A1" } },
                        new BookingSeat { Seat = new Seat { SeatNumber = "A2" } }
                    }
                };

                await _emailService.SendBookingConfirmationEmailAsync(
                    mockBooking, 
                    mockBooking.BookingSeats.ToList(), 
                    request.ToEmail, 
                    request.ToName
                );

                return Ok(new { 
                    success = true, 
                    message = "Test email sent successfully",
                    emailSentTo = request.ToEmail,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = "Failed to send test email",
                    error = ex.Message
                });
            }
        }
    }

    public class SendBookingConfirmationRequest
    {
        public int BookingId { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
    }

    public class TestEmailRequest
    {
        public string ToEmail { get; set; }
        public string ToName { get; set; }
    }
}
