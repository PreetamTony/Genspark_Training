using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LayoutsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public LayoutsController(AppDbContext context) { _context = context; }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetLayouts()
        {
            return await _context.Layouts
                .Select(l => new { l.Id, l.Name, l.Type, l.TotalCapacity, l.SeatConfigurationJson })
                .ToListAsync();
        }

        [HttpPost]
        [Authorize(Roles = "Operator,Admin")]
        public async Task<IActionResult> PostLayout([FromBody] Layout layout)
        {
            _context.Layouts.Add(layout);
            await _context.SaveChangesAsync();
            return Ok(new { layout.Id, message = "Layout created." });
        }

        // Seed standard layouts
        [HttpPost("seed-defaults")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SeedDefaults()
        {
            if (await _context.Layouts.AnyAsync())
                return BadRequest("Layouts already seeded.");

            var layouts = new List<Layout>
            {
                new Layout
                {
                    Name = "2+2 Seater (40 seats)",
                    Type = "Seater",
                    TotalCapacity = 40,
                    SeatConfigurationJson = GenerateSeaterConfig(10, 4) // 10 rows, 4 cols
                },
                new Layout
                {
                    Name = "2+1 Sleeper (30 berths)",
                    Type = "Sleeper",
                    TotalCapacity = 30,
                    SeatConfigurationJson = GenerateSleeperConfig(10, 3) // 10 rows, 3 cols
                },
                new Layout
                {
                    Name = "1+1 Luxury (20 seats)",
                    Type = "Seater",
                    TotalCapacity = 20,
                    SeatConfigurationJson = GenerateSeaterConfig(10, 2)
                }
            };

            _context.Layouts.AddRange(layouts);
            await _context.SaveChangesAsync();
            return Ok("Default layouts seeded.");
        }

        private static string GenerateSeaterConfig(int rows, int cols)
        {
            var seats = new List<object>();
            var colLabels = new[] { "A", "B", "C", "D" };
            for (int r = 1; r <= rows; r++)
                for (int c = 0; c < cols; c++)
                    seats.Add(new { row = r, col = colLabels[c], label = $"{r}{colLabels[c]}" });
            return System.Text.Json.JsonSerializer.Serialize(seats);
        }

        private static string GenerateSleeperConfig(int rows, int cols)
        {
            var seats = new List<object>();
            var colLabels = new[] { "L", "M", "U" };
            for (int r = 1; r <= rows; r++)
                for (int c = 0; c < cols; c++)
                    seats.Add(new { row = r, col = colLabels[c], label = $"{r}{colLabels[c]}", type = c == 2 ? "upper" : "lower" });
            return System.Text.Json.JsonSerializer.Serialize(seats);
        }
    }
}
