using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notification;
        public AdminController(AppDbContext context, INotificationService notification)
        {
            _context = context; _notification = notification;
        }

        // --- Operator Management ---
        [HttpGet("operators")]
        public async Task<IActionResult> GetOperators()
        {
            return Ok(await _context.OperatorProfiles
                .Include(op => op.User)
                .Include(op => op.HeadOfficeLocation)
                .Select(op => new
                {
                    op.Id, op.CompanyName, Status = op.Status.ToString(),
                    Email = op.User.Email, Name = op.User.Name,
                    HeadOffice = op.HeadOfficeLocation != null ? op.HeadOfficeLocation.Name : null
                }).ToListAsync());
        }

        [HttpPut("operators/{id}/approve")]
        public async Task<IActionResult> ApproveOperator(int id)
        {
            var profile = await _context.OperatorProfiles.FindAsync(id);
            if (profile == null) return NotFound();
            profile.Status = OperatorStatus.Active;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Operator approved." });
        }

        [HttpPut("operators/{id}/reject")]
        public async Task<IActionResult> RejectOperator(int id)
        {
            var profile = await _context.OperatorProfiles.FindAsync(id);
            if (profile == null) return NotFound();
            profile.Status = OperatorStatus.Rejected;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Operator rejected." });
        }

        [HttpPut("operators/{id}/disable")]
        public async Task<IActionResult> DisableOperator(int id)
        {
            var profile = await _context.OperatorProfiles.FindAsync(id);
            if (profile == null) return NotFound();
            profile.Status = OperatorStatus.Disabled;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Operator disabled." });
        }

        [HttpPut("operators/{id}/enable")]
        public async Task<IActionResult> EnableOperator(int id)
        {
            var profile = await _context.OperatorProfiles.FindAsync(id);
            if (profile == null) return NotFound();
            profile.Status = OperatorStatus.Active;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Operator enabled." });
        }

        // --- Revenue Dashboard ---
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue()
        {
            var feeConfig = await _context.PlatformConfigs.FirstOrDefaultAsync(c => c.Key == "ConvenienceFee");
            var feePerSeat = feeConfig != null ? decimal.Parse(feeConfig.Value) : 50m;

            var totalRevenue = await _context.Bookings
                .Where(b => b.Status == BookingStatus.Confirmed)
                .SumAsync(b => (decimal?)b.TotalPrice) ?? 0;

            var platformFees = await _context.Bookings
                .Where(b => b.Status == BookingStatus.Confirmed)
                .SumAsync(b => (decimal?)b.ConvenienceFee) ?? 0;

            var operatorRevenue = await _context.Bookings
                .Include(b => b.Schedule).ThenInclude(s => s.Bus).ThenInclude(b => b.Operator)
                .Where(b => b.Status == BookingStatus.Confirmed)
                .GroupBy(b => b.Schedule.Bus.Operator.CompanyName)
                .Select(g => new { Operator = g.Key, Revenue = g.Sum(b => b.TotalPrice - b.ConvenienceFee) })
                .ToListAsync();

            // Mock AI/ML demand forecast
            var mockDemandForecast = new[]
            {
                new { Route = "Chennai → Bangalore", Predicted = 87, Confidence = "High" },
                new { Route = "Mumbai → Pune", Predicted = 65, Confidence = "Medium" },
                new { Route = "Delhi → Agra", Predicted = 92, Confidence = "High" }
            };

            return Ok(new { totalRevenue, platformFees, operatorRevenue, demandForecast = mockDemandForecast });
        }

        [HttpGet("schedules")]
        public async Task<IActionResult> GetSchedules()
        {
            var schedules = await _context.Schedules
                .Include(s => s.Route).ThenInclude(r => r.Source)
                .Include(s => s.Route).ThenInclude(r => r.Destination)
                .Include(s => s.Bus).ThenInclude(b => b.Operator)
                .Select(s => new
                {
                    s.Id,
                    s.DepartureTime,
                    s.ArrivalTime,
                    s.BasePrice,
                    Status = s.Status.ToString(),
                    PickupPoint = s.PickupPoint,
                    DropPoint = s.DropPoint,
                    Operator = s.Bus.Operator.CompanyName,
                    Route = new { Source = s.Route.Source.Name, Destination = s.Route.Destination.Name }
                })
                .ToListAsync();
            return Ok(schedules);
        }

        // --- Platform Config (Convenience Fee) ---
        [HttpGet("config")]
        public async Task<IActionResult> GetConfig()
        {
            return Ok(await _context.PlatformConfigs.ToListAsync());
        }

        [HttpPut("config/convenience-fee")]
        public async Task<IActionResult> SetConvenienceFee([FromBody] SetFeeDto dto)
        {
            var config = await _context.PlatformConfigs.FirstOrDefaultAsync(c => c.Key == "ConvenienceFee");
            if (config == null)
                _context.PlatformConfigs.Add(new PlatformConfig { Key = "ConvenienceFee", Value = dto.Fee.ToString() });
            else
                config.Value = dto.Fee.ToString();
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Convenience fee set to ₹{dto.Fee}." });
        }

        // --- Cancel Schedule (Emergency) ---
        [HttpPut("schedules/{id}/cancel")]
        public async Task<IActionResult> CancelSchedule(int id, [FromBody] CancelScheduleDto dto)
        {
            var schedule = await _context.Schedules
                .Include(s => s.Route).ThenInclude(r => r.Source)
                .Include(s => s.Route).ThenInclude(r => r.Destination)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (schedule == null) return NotFound();

            schedule.Status = ScheduleStatus.Cancelled;
            await _context.SaveChangesAsync();

            var routeInfo = $"{schedule.Route.Source.Name} → {schedule.Route.Destination.Name}";

            // Notify operator
            var operatorUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == schedule.Bus.Operator.UserId);
            if (operatorUser != null)
            {
                await _notification.SendRouteCancellationNoticeAsync(
                    operatorUser.Email, operatorUser.Name, routeInfo, schedule.DepartureTime);
            }

            // Notify all passengers
            var affectedBookings = await _context.Bookings
                .Include(b => b.User)
                .Where(b => b.ScheduleId == id && b.Status == BookingStatus.Confirmed)
                .ToListAsync();

            foreach (var booking in affectedBookings)
            {
                booking.Status = BookingStatus.Cancelled;
                await _notification.SendRouteCancellationNoticeAsync(
                    booking.User.Email, booking.User.Name, routeInfo, schedule.DepartureTime);
            }
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Schedule cancelled. {affectedBookings.Count} passengers notified." });
        }

        // --- Update Bus Features ---
        [HttpPost("buses/{busId}/update-features")]
        public async Task<IActionResult> UpdateBusFeatures(int busId)
        {
            var bus = await _context.Buses.FindAsync(busId);
            if (bus == null) return NotFound();

            // Update with comprehensive features
            bus.HasWaterBottle = true;
            bus.HasBlankets = true;
            bus.HasChargingPoint = true;
            bus.HasCCTV = true;
            bus.HasToilet = false;
            bus.HasWiFi = true;
            bus.HasReadingLight = true;
            bus.HasEmergencyExit = true;
            bus.HasGPS = true;
            bus.BusType = "Volvo Multi-Axle A/C Semi Sleeper (2+2)";
            bus.Rating = 4.9;
            bus.TotalRatings = 1116;
            bus.OnTimeTrips = 940;
            bus.TotalTrips = 950;
            bus.CancellationPolicy = "Before 25th Apr 11:10 AM - 85%; From 25th Apr 11:10 AM Until 25th Apr 03:10 PM - 70%; From 25th Apr 03:10 PM Until 25th Apr 07:10 PM - 40%; From 25th Apr 07:10 PM Until 25th Apr 11:10 PM - 5%";
            bus.ReschedulePolicy = "Before 25th Apr 04:10 PM - FREE";
            bus.ChildPolicy = "Children above the age of 3 will need a ticket";
            bus.LuggagePolicy = "1 pieces of luggage will be accepted free of charge per passenger. Excess items will be chargeable";
            bus.PetPolicy = "Pets are not allowed";
            bus.ToiletPolicy = "In Bus Toilet with Only Urinal Facility";
            bus.LiquorPolicy = "Carrying or consuming liquor inside the bus is prohibited. Bus operator reserves the right to deboard drunk passengers.";

            await _context.SaveChangesAsync();
            return Ok(new { message = "Bus features updated successfully" });
        }

        // --- Mock Smart Pricing ---
        [HttpGet("smart-pricing/{scheduleId}")]
        public async Task<IActionResult> GetSmartPricing(int scheduleId)
        {
            var schedule = await _context.Schedules.FindAsync(scheduleId);
            if (schedule == null) return NotFound();
            // Mock ML: surge 20% if within 3 days
            var daysLeft = (schedule.DepartureTime - DateTime.UtcNow).TotalDays;
            var factor = daysLeft < 3 ? 1.2m : 1.0m;
            return Ok(new
            {
                BasePrice = schedule.BasePrice,
                RecommendedPrice = Math.Round(schedule.BasePrice * factor, 2),
                Reason = daysLeft < 3 ? "High demand — departure within 3 days." : "Normal demand.",
                Confidence = "85%"
            });
        }
    }

    public class SetFeeDto { public decimal Fee { get; set; } }
    public class CancelScheduleDto { public string? Reason { get; set; } }
}
