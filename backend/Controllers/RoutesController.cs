using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusRoute = backend.Models.Route;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoutesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public RoutesController(AppDbContext context) { _context = context; }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetRoutes()
        {
            return await _context.Routes
                .Include(r => r.Source)
                .Include(r => r.Destination)
                .Select(r => new
                {
                    r.Id,
                    Source = new { r.Source.Id, r.Source.Name },
                    Destination = new { r.Destination.Id, r.Destination.Name },
                    r.Stops
                })
                .ToListAsync();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PostRoute([FromBody] CreateRouteDto dto)
        {
            var route = new BusRoute
            {
                SourceId = dto.SourceId,
                DestinationId = dto.DestinationId,
                Stops = dto.Stops
            };
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();
            return Ok(new { route.Id, message = "Route created." });
        }
    }

    public class CreateRouteDto
    {
        public int SourceId { get; set; }
        public int DestinationId { get; set; }
        public string? Stops { get; set; }
    }
}
