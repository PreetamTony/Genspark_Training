using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public BusesController(AppDbContext context) { _context = context; }

        [HttpGet]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<ActionResult<IEnumerable<object>>> GetBuses()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var query = _context.Buses
                .Include(b => b.Operator)
                .Include(b => b.Layout)
                .AsQueryable();

            if (role == "Operator")
            {
                var profile = await _context.OperatorProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
                if (profile == null) return NotFound("Operator profile not found.");
                query = query.Where(b => b.OperatorProfileId == profile.Id);
            }

            return await query.Select(b => new
            {
                b.Id, b.RegistrationNumber, Status = b.Status.ToString(),
                Layout = new { b.Layout.Id, b.Layout.Name, b.Layout.TotalCapacity, b.Layout.Type },
                Operator = new { b.Operator.Id, b.Operator.CompanyName }
            }).ToListAsync();
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<object>>> GetPendingBuses()
        {
            return await _context.Buses
                .Include(b => b.Operator).Include(b => b.Layout)
                .Where(b => b.Status == BusStatus.PendingApproval)
                .Select(b => new { b.Id, b.RegistrationNumber, b.Operator.CompanyName, b.Layout.Name })
                .ToListAsync();
        }

        [HttpPost("request")]
        [Authorize(Roles = "Operator")]
        public async Task<IActionResult> RequestBus([FromBody] BusRequestDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var profile = await _context.OperatorProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return NotFound("Operator profile not found.");

            if (profile.Status != OperatorStatus.Active)
                return BadRequest("Your operator account must be approved before adding buses.");

            var bus = new Bus
            {
                OperatorProfileId = profile.Id,
                LayoutId = dto.LayoutId,
                RegistrationNumber = dto.RegistrationNumber,
                Status = BusStatus.PendingApproval
            };
            _context.Buses.Add(bus);
            await _context.SaveChangesAsync();

            // Create seats from layout
            var layout = await _context.Layouts.FindAsync(dto.LayoutId);
            if (layout != null)
            {
                var seats = System.Text.Json.JsonSerializer.Deserialize<List<System.Text.Json.JsonElement>>(layout.SeatConfigurationJson);
                foreach (var seat in seats!)
                {
                    _context.Seats.Add(new Seat { BusId = bus.Id, SeatNumber = seat.GetProperty("label").GetString()! });
                }
                await _context.SaveChangesAsync();
            }

            return Ok(new { bus.Id, message = "Bus request submitted. Awaiting admin approval." });
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveBus(int id)
        {
            var bus = await _context.Buses.FindAsync(id);
            if (bus == null) return NotFound();
            bus.Status = BusStatus.Active;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Bus approved and activated." });
        }

        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectBus(int id)
        {
            var bus = await _context.Buses.FindAsync(id);
            if (bus == null) return NotFound();
            bus.Status = BusStatus.Rejected;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Bus request rejected." });
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Operator")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateBusStatusDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var profile = await _context.OperatorProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            var bus = await _context.Buses.FirstOrDefaultAsync(b => b.Id == id && b.OperatorProfileId == profile!.Id);
            if (bus == null) return NotFound("Bus not found or you don't own it.");

            bus.Status = dto.Status;
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Bus status updated to {dto.Status}." });
        }

        [HttpGet("bookings")]
        [Authorize(Roles = "Operator")]
        public async Task<IActionResult> GetOperatorBookings()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var profile = await _context.OperatorProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return NotFound("Operator profile not found.");

            var bookings = await _context.Bookings
                .Include(b => b.Schedule).ThenInclude(s => s.Route).ThenInclude(r => r.Source)
                .Include(b => b.Schedule).ThenInclude(s => s.Route).ThenInclude(r => r.Destination)
                .Include(b => b.Schedule).ThenInclude(s => s.Bus)
                .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
                .Include(b => b.User)
                .Where(b => b.Schedule.Bus.OperatorProfileId == profile.Id)
                .OrderByDescending(b => b.BookingDate)
                .Select(b => new
                {
                    b.Id,
                    Status = b.Status.ToString(),
                    b.TotalPrice,
                    b.ConvenienceFee,
                    b.BookingDate,
                    From = b.Schedule.Route.Source.Name,
                    To = b.Schedule.Route.Destination.Name,
                    DepartureTime = b.Schedule.DepartureTime,
                    Operator = b.Schedule.Bus.Operator.CompanyName,
                    Passenger = new { b.User.Name, b.User.Email },
                    Seats = b.BookingSeats.Select(bs => bs.Seat.SeatNumber)
                })
                .ToListAsync();

            return Ok(bookings);
        }

        [HttpPut("head-office")]
        [Authorize(Roles = "Operator")]
        public async Task<IActionResult> UpdateHeadOffice([FromBody] UpdateHeadOfficeDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var profile = await _context.OperatorProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return NotFound("Operator profile not found.");

            var location = await _context.Locations.FindAsync(dto.LocationId);
            if (location == null) return NotFound("Location not found.");

            profile.HeadOfficeLocationId = dto.LocationId;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Head office location updated." });
        }
    }

    public class BusRequestDto
    {
        public string RegistrationNumber { get; set; } = string.Empty;
        public int LayoutId { get; set; }
    }

    public class UpdateBusStatusDto
    {
        public BusStatus Status { get; set; }
    }
}
