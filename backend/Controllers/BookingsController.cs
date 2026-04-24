using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class BookingsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISeatLockService _seatLock;
        private readonly ICancellationService _cancellation;
        private readonly INotificationService _notification;

        public BookingsController(AppDbContext context, ISeatLockService seatLock,
            ICancellationService cancellation, INotificationService notification)
        {
            _context = context;
            _seatLock = seatLock;
            _cancellation = cancellation;
            _notification = notification;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpPost("lock-seat")]
        public async Task<IActionResult> LockSeat([FromBody] LockSeatDto dto)
        {
            var userId = GetUserId();
            var locked = await _seatLock.TryLockSeatAsync(dto.ScheduleId, dto.SeatId, userId, TimeSpan.FromMinutes(10));
            if (!locked)
                return BadRequest(new { message = "Seat is currently held by another user. Please choose a different seat." });
            return Ok(new { message = "Seat locked for 10 minutes." });
        }

        [HttpPost("unlock-seat")]
        public async Task<IActionResult> UnlockSeat([FromBody] LockSeatDto dto)
        {
            var userId = GetUserId();
            var owner = await _seatLock.GetSeatLockOwnerAsync(dto.ScheduleId, dto.SeatId);
            if (owner != userId)
                return BadRequest(new { message = "Cannot release a lock you do not own." });

            await _seatLock.ReleaseSeatAsync(dto.ScheduleId, dto.SeatId);
            return Ok(new { message = "Seat lock released." });
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
        {
            var userId = GetUserId();

            var schedule = await _context.Schedules
                .Include(s => s.Route).ThenInclude(r => r.Source)
                .Include(s => s.Route).ThenInclude(r => r.Destination)
                .FirstOrDefaultAsync(s => s.Id == dto.ScheduleId);
            if (schedule == null) return NotFound("Schedule not found.");

            // Validate each seat is still locked by THIS user
            foreach (var seatId in dto.SeatIds)
            {
                var isLockedByOther = await _seatLock.IsSeatLockedAsync(dto.ScheduleId, seatId, excludeUserId: userId);
                if (isLockedByOther)
                    return BadRequest(new { message = $"Seat {seatId} is no longer locked. Please re-select." });

                // Check not already booked in DB
                var alreadyBooked = await _context.BookingSeats
                    .Include(bs => bs.Booking)
                    .AnyAsync(bs => bs.SeatId == seatId && bs.Booking.ScheduleId == dto.ScheduleId && bs.Booking.Status == BookingStatus.Confirmed);
                if (alreadyBooked)
                    return BadRequest(new { message = $"Seat {seatId} is already booked." });
            }

            // Get platform convenience fee
            var feeConfig = await _context.PlatformConfigs.FirstOrDefaultAsync(c => c.Key == "ConvenienceFee");
            var feePerSeat = feeConfig != null ? decimal.Parse(feeConfig.Value) : 50m;

            var seatCount = dto.SeatIds.Count;
            var convenienceFee = feePerSeat * seatCount;
            var baseTotal = (schedule.BasePrice * seatCount) + convenienceFee;

            decimal discount = 0;
            string? couponMessage = null;
            // TODO: Re-enable coupon functionality after fixing DTO issues
            // if (!string.IsNullOrWhiteSpace(dto.CouponCode))
            // {
            //     // Find valid coupon for this operator
            //     var coupon = await _context.Coupons
            //         .Where(c => c.Code == dto.CouponCode && c.OperatorId == schedule.Bus.OperatorProfileId && c.IsActive)
            //         .FirstOrDefaultAsync();
            //     if (coupon == null)
            //     {
            //         couponMessage = "Invalid or expired coupon code.";
            //     }
            //     else if ((coupon.ValidFrom.HasValue && coupon.ValidFrom.Value > DateTime.UtcNow) ||
            //              (coupon.ValidTo.HasValue && coupon.ValidTo.Value < DateTime.UtcNow))
            //     {
            //         couponMessage = "Coupon is not valid at this time.";
            //     }
            //     else
            //     {
            //         // Apply discount
            //         if (coupon.DiscountPercent.HasValue && coupon.DiscountPercent.Value > 0)
            //         {
            //             discount = baseTotal * (coupon.DiscountPercent.Value / 100m);
            //         }
            //         else
            //         {
            //             discount = coupon.DiscountAmount;
            //         }
            //         // Ensure discount does not exceed baseTotal
            //         if (discount > baseTotal) discount = baseTotal;
            //         couponMessage = $"Coupon applied. Discount: {discount:C}.";
            //     }
            // }

            var totalPrice = baseTotal - discount;

            var booking = new Booking
            {
                UserId = userId,
                ScheduleId = dto.ScheduleId,
                Status = BookingStatus.Confirmed,
                TotalPrice = totalPrice,
                ConvenienceFee = convenienceFee,
                // CouponCode = dto.CouponCode, // TODO: Re-enable after fixing DTO issues
                BookingDate = DateTime.UtcNow
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            foreach (var seatId in dto.SeatIds)
            {
                _context.BookingSeats.Add(new BookingSeat { BookingId = booking.Id, SeatId = seatId });
                await _seatLock.ReleaseSeatAsync(dto.ScheduleId, seatId);
            }
            await _context.SaveChangesAsync();

            // Send confirmation email
            var user = await _context.Users.FindAsync(userId);
            var seatNumbers = await _context.Seats
                .Where(s => dto.SeatIds.Contains(s.Id))
                .Select(s => s.SeatNumber).ToListAsync();

            await _notification.SendBookingConfirmationAsync(
                user!.Email, user.Name, booking.Id,
                schedule.Route.Source.Name, schedule.Route.Destination.Name,
                schedule.DepartureTime, seatNumbers, totalPrice);

            return Ok(new { bookingId = booking.Id, totalPrice, convenienceFee, discount, couponMessage, message = "Booking confirmed! Ticket sent to email." });
        }

        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = GetUserId();
            var bookings = await _context.Bookings
                .Include(b => b.Schedule).ThenInclude(s => s.Route).ThenInclude(r => r.Source)
                .Include(b => b.Schedule).ThenInclude(s => s.Route).ThenInclude(r => r.Destination)
                .Include(b => b.Schedule).ThenInclude(s => s.Bus).ThenInclude(b => b.Operator)
                .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .Select(b => new
                {
                    b.Id, Status = b.Status.ToString(), b.TotalPrice, b.ConvenienceFee, b.BookingDate,
                    From = b.Schedule.Route.Source.Name,
                    To = b.Schedule.Route.Destination.Name,
                    DepartureTime = b.Schedule.DepartureTime,
                    Operator = b.Schedule.Bus.Operator.CompanyName,
                    Seats = b.BookingSeats.Select(bs => bs.Seat.SeatNumber)
                }).ToListAsync();
            return Ok(bookings);
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var userId = GetUserId();
            var booking = await _context.Bookings
                .Include(b => b.Schedule)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            if (booking == null) return NotFound("Booking not found.");
            if (booking.Status == BookingStatus.Cancelled)
                return BadRequest("Booking is already cancelled.");

            var refund = _cancellation.CalculateRefund(booking.TotalPrice, booking.Schedule.DepartureTime, false);
            booking.Status = BookingStatus.Cancelled;
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);
            await _notification.SendCancellationNoticeAsync(user!.Email, user.Name, booking.Id, refund, "User requested cancellation");

            return Ok(new { message = "Booking cancelled.", refundAmount = refund });
        }
    }

    public class LockSeatDto { public int ScheduleId { get; set; } public int SeatId { get; set; } }
    public class CreateBookingDto { public int ScheduleId { get; set; } public List<int> SeatIds { get; set; } = new(); }
}
