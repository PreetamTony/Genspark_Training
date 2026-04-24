using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchedulesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notification;
        private readonly ILocationService _locationService;
        
        public SchedulesController(AppDbContext context, INotificationService notification, ILocationService locationService)
        {
            _context = context;
            _notification = notification;
            _locationService = locationService;
        }

        /// <summary>
        /// Test endpoint to verify database connection
        /// </summary>
        [HttpGet("test")]
        public async Task<ActionResult<object>> Test()
        {
            try
            {
                var scheduleCount = await _context.Schedules.CountAsync();
                var locationCount = await _context.Locations.CountAsync();
                return Ok(new { message = "Database connection working", schedules = scheduleCount, locations = locationCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Public search — returns schedules with available seat count per bus.
        /// Supports fuzzy location matching.
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<object>>> Search(
            [FromQuery] string source,
            [FromQuery] string destination,
            [FromQuery] string date)
        {
            try
            {
                Console.WriteLine($"RAW Search request: {source} -> {destination} on date string: '{date}'");
                
                if (!DateTime.TryParse(date, out DateTime parsedDate))
                {
                    // Try fallback formats if standard parsing fails
                    if (!DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out parsedDate))
                    {
                        Console.WriteLine($"Failed to parse date: {date}");
                        return BadRequest(new { message = "Invalid date format. Expected YYYY-MM-DD." });
                    }
                }

                // Use fuzzy location matching
                var normalizedSource = _locationService.NormalizeLocationName(source ?? "");
                var normalizedDestination = _locationService.NormalizeLocationName(destination ?? "");
                
                Console.WriteLine($"Normalized locations: {normalizedSource} -> {normalizedDestination}");

                // Get real schedules from database
                var allSchedules = await _context.Schedules
                    .Include(s => s.Bus).ThenInclude(b => b.Operator)
                    .Include(s => s.Bus).ThenInclude(b => b.Layout)
                    .Include(s => s.Route).ThenInclude(r => r.Source)
                    .Include(s => s.Route).ThenInclude(r => r.Destination)
                    .Where(s => s.Status == ScheduleStatus.Scheduled && s.Bus.Status == BusStatus.Active)
                    .ToListAsync();

                // Use fuzzy matching - check both original and normalized names
                var filteredSchedules = allSchedules
                    .Where(s => 
                        s.DepartureTime.Date == parsedDate.Date &&
                        (s.Route.Source.Name.Equals(normalizedSource, StringComparison.OrdinalIgnoreCase) ||
                         s.Route.Source.Name.Equals(source, StringComparison.OrdinalIgnoreCase) ||
                         s.Route.Source.Name.ToLower().Contains(source?.ToLower() ?? "")) &&
                        (s.Route.Destination.Name.Equals(normalizedDestination, StringComparison.OrdinalIgnoreCase) ||
                         s.Route.Destination.Name.Equals(destination, StringComparison.OrdinalIgnoreCase) ||
                         s.Route.Destination.Name.ToLower().Contains(destination?.ToLower() ?? "")))
                    .ToList();

                Console.WriteLine($"Found {filteredSchedules.Count} schedules.");

                var results = new List<object>();
                foreach (var s in filteredSchedules)
                {
                    var totalSeats = s.Bus.Layout.TotalCapacity;
                    var availableSeats = totalSeats; // Simplified: assume all seats are available for now

                    // Determine pickup and drop points based on operator head office logic
                    string pickupPoint = s.PickupPoint ?? "";
                    string dropPoint = s.DropPoint ?? "";

                    // If route consists of operator head offices, use them as pickup/drop points
                    if (s.Bus.Operator != null)
                    {
                        var headOffice = s.Bus.Operator.HeadOfficeLocation;
                        if (headOffice != null)
                        {
                            var sourceName = s.Route.Source.Name.ToLower();
                            var destName = s.Route.Destination.Name.ToLower();
                            var headOfficeName = headOffice.Name.ToLower();

                            // If source is operator head office, override pickup point
                            if (sourceName == headOfficeName)
                            {
                                pickupPoint = $"{headOffice.Name} Main Office";
                            }

                            // If destination is operator head office, override drop point  
                            if (destName == headOfficeName)
                            {
                                dropPoint = $"{headOffice.Name} Main Office";
                            }
                        }
                    }

                    // Use defaults if still empty
                    if (string.IsNullOrEmpty(pickupPoint))
                        pickupPoint = $"{s.Route.Source.Name} Bus Stand";
                    if (string.IsNullOrEmpty(dropPoint))
                        dropPoint = $"{s.Route.Destination.Name} Bus Stand";

                    results.Add(new
                    {
                        s.Id,
                        s.DepartureTime,
                        s.ArrivalTime,
                        s.BasePrice,
                        PickupPoint = pickupPoint,
                        DropPoint = dropPoint,
                        AvailableSeats = availableSeats,
                        TotalSeats = totalSeats,
                        Bus = new { 
                            s.Bus.Id, 
                            s.Bus.RegistrationNumber, 
                            s.Bus.Layout.Name, 
                            s.Bus.Layout.Type,
                            // Bus Features
                            Features = new {
                                HasWaterBottle = s.Bus.HasWaterBottle,
                                HasBlankets = s.Bus.HasBlankets,
                                HasChargingPoint = s.Bus.HasChargingPoint,
                                HasCCTV = s.Bus.HasCCTV,
                                HasToilet = s.Bus.HasToilet,
                                HasWiFi = s.Bus.HasWiFi,
                                HasReadingLight = s.Bus.HasReadingLight,
                                HasEmergencyExit = s.Bus.HasEmergencyExit,
                                HasGPS = s.Bus.HasGPS
                            },
                            // Ratings and Performance
                            Rating = s.Bus.Rating,
                            TotalRatings = s.Bus.TotalRatings,
                            OnTimeTrips = s.Bus.OnTimeTrips,
                            TotalTrips = s.Bus.TotalTrips,
                            OnTimePercentage = s.Bus.TotalTrips > 0 ? Math.Round((double)s.Bus.OnTimeTrips / s.Bus.TotalTrips * 100, 1) : 0,
                            // Policies
                            Policies = new {
                                CancellationPolicy = s.Bus.CancellationPolicy,
                                ReschedulePolicy = s.Bus.ReschedulePolicy,
                                ChildPolicy = s.Bus.ChildPolicy,
                                LuggagePolicy = s.Bus.LuggagePolicy,
                                PetPolicy = s.Bus.PetPolicy,
                                ToiletPolicy = s.Bus.ToiletPolicy,
                                LiquorPolicy = s.Bus.LiquorPolicy
                            }
                        },
                        Operator = new { CompanyName = s.Bus.Operator?.CompanyName ?? "Unknown" },
                        Route = new { Source = s.Route.Source.Name, Destination = s.Route.Destination.Name }
                    });
                }

                Console.WriteLine($"Returning {results.Count} results for {source} -> {destination} on {parsedDate:yyyy-MM-dd}");
                return Ok(results);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Search: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { message = "An error occurred while searching schedules." });
            }
        }


        [HttpGet("{id}/seats")]
        public async Task<IActionResult> GetSeatAvailability(int id)
        {
            var schedule = await _context.Schedules
                .Include(s => s.Bus).ThenInclude(b => b.Layout)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (schedule == null) return NotFound();

            var allSeats = await _context.Seats.Where(s => s.BusId == schedule.BusId).ToListAsync();
            var bookedSeatIds = await _context.BookingSeats
                .Include(bs => bs.Booking)
                .Where(bs => bs.Booking.ScheduleId == id && bs.Booking.Status == BookingStatus.Confirmed)
                .Select(bs => bs.SeatId)
                .ToListAsync();

            var seatConfig = System.Text.Json.JsonSerializer.Deserialize<List<System.Text.Json.JsonElement>>(
                schedule.Bus.Layout.SeatConfigurationJson);

            var result = allSeats.Select(seat => new
            {
                seat.Id,
                seat.SeatNumber,
                IsBooked = bookedSeatIds.Contains(seat.Id)
            });

            return Ok(new
            {
                LayoutType = schedule.Bus.Layout.Type,
                SeatConfiguration = seatConfig,
                Seats = result
            });
        }

        [HttpPost]
        [Authorize(Roles = "Operator")]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var profile = await _context.OperatorProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return NotFound("Operator profile not found.");

            var bus = await _context.Buses
                .Include(b => b.Operator)
                .FirstOrDefaultAsync(b => b.Id == dto.BusId && b.OperatorProfileId == profile.Id && b.Status == BusStatus.Active);
            if (bus == null) return BadRequest("Bus not found, not owned by you, or not active.");

            // Auto pickup/drop: check if operator HQ is source or destination
            string? pickupPoint = dto.PickupPoint;
            string? dropPoint = dto.DropPoint;

            if (profile.HeadOfficeLocationId.HasValue)
            {
                var route = await _context.Routes
                    .Include(r => r.Source)
                    .Include(r => r.Destination)
                    .FirstOrDefaultAsync(r => r.Id == dto.RouteId);

                if (route != null)
                {
                    if (route.SourceId == profile.HeadOfficeLocationId)
                        pickupPoint = $"{bus.Operator.CompanyName} Head Office";
                    if (route.DestinationId == profile.HeadOfficeLocationId)
                        dropPoint = $"{bus.Operator.CompanyName} Head Office";
                }
            }

            var schedule = new Schedule
            {
                BusId = dto.BusId,
                RouteId = dto.RouteId,
                DepartureTime = dto.DepartureTime,
                ArrivalTime = dto.ArrivalTime,
                BasePrice = dto.BasePrice,
                Status = ScheduleStatus.Scheduled,
                PickupPoint = pickupPoint,
                DropPoint = dropPoint
            };

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();
            return Ok(new { schedule.Id, message = "Schedule created." });
        }

        [HttpGet("operator/my-schedules")]
        [Authorize(Roles = "Operator")]
        public async Task<IActionResult> GetMySchedules()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var profile = await _context.OperatorProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return NotFound();

            var schedules = await _context.Schedules
                .Include(s => s.Bus)
                .Include(s => s.Route).ThenInclude(r => r.Source)
                .Include(s => s.Route).ThenInclude(r => r.Destination)
                .Where(s => s.Bus.OperatorProfileId == profile.Id)
                .Select(s => new
                {
                    s.Id, s.DepartureTime, s.ArrivalTime, s.BasePrice,
                    Status = s.Status.ToString(),
                    s.Bus.RegistrationNumber,
                    From = s.Route.Source.Name,
                    To = s.Route.Destination.Name
                })
                .ToListAsync();
            return Ok(schedules);
        }

        [HttpPut("operator/{id}/cancel")]
        [Authorize(Roles = "Operator")]
        public async Task<IActionResult> CancelMySchedule(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var profile = await _context.OperatorProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return NotFound("Operator profile not found.");

            var schedule = await _context.Schedules
                .Include(s => s.Bus).ThenInclude(b => b.Operator)
                .Include(s => s.Route).ThenInclude(r => r.Source)
                .Include(s => s.Route).ThenInclude(r => r.Destination)
                .FirstOrDefaultAsync(s => s.Id == id && s.Bus.OperatorProfileId == profile.Id);
            if (schedule == null) return NotFound("Schedule not found or you don't own it.");

            schedule.Status = ScheduleStatus.Cancelled;
            await _context.SaveChangesAsync();

            var routeInfo = $"{schedule.Route.Source.Name} → {schedule.Route.Destination.Name}";
            var operatorUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == schedule.Bus.Operator.UserId);
            if (operatorUser != null)
            {
                await _notification.SendRouteCancellationNoticeAsync(
                    operatorUser.Email, operatorUser.Name, routeInfo, schedule.DepartureTime);
            }

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

            return Ok(new { message = $"Schedule cancelled. {affectedBookings.Count} passengers affected." });
        }
    }

    public class UtcDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateTime = reader.GetDateTime();
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(DateTime.SpecifyKind(value, DateTimeKind.Utc));
        }
    }

    public class CreateScheduleDto
    {
        public int BusId { get; set; }
        public int RouteId { get; set; }
        
        [JsonConverter(typeof(UtcDateTimeConverter))]
        public DateTime DepartureTime { get; set; }
        
        [JsonConverter(typeof(UtcDateTimeConverter))]
        public DateTime ArrivalTime { get; set; }
        
        public decimal BasePrice { get; set; }
        public string? PickupPoint { get; set; }
        public string? DropPoint { get; set; }
    }
}
