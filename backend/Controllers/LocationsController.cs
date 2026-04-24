using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILocationService _locationService;
        
        public LocationsController(AppDbContext context, ILocationService locationService) 
        { 
            _context = context;
            _locationService = locationService;
        }

        // Public — used for pre-population and fuzzy search
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetLocations([FromQuery] string? q)
        {
            try
            {
                var allLocations = await _context.Locations.ToListAsync();
                
                if (string.IsNullOrEmpty(q))
                {
                    // Return all locations if no query
                    var locations = allLocations
                        .Select(l => new { l.Id, l.Name, l.State })
                        .Take(50)
                        .OrderBy(l => l.Name)
                        .ToList();
                    return Ok(locations);
                }

                if (q.Length < 2)
                {
                    return BadRequest(new { message = "Search query must be at least 2 characters long." });
                }

                // Use fuzzy matching with LocationService
                var normalizedQuery = _locationService.NormalizeLocationName(q);
                var matchedLocations = new List<Location>();
                var matchedLocationObjects = new List<object>();

                // First try exact match with normalized name
                var exactMatch = allLocations.FirstOrDefault(l => 
                    l.Name.Equals(normalizedQuery, StringComparison.OrdinalIgnoreCase));
                if (exactMatch != null)
                {
                    matchedLocations.Add(exactMatch);
                    matchedLocationObjects.Add(new { exactMatch.Id, exactMatch.Name, exactMatch.State });
                }

                // Then try partial matches and fuzzy matches
                var lowerQuery = q.ToLower();
                var partialMatches = allLocations
                    .Where(l => l.Name.ToLower().Contains(lowerQuery))
                    .ToList();

                // Add partial matches that aren't already added
                foreach (var match in partialMatches)
                {
                    if (!matchedLocations.Any(m => m.Id == match.Id))
                    {
                        matchedLocations.Add(match);
                        matchedLocationObjects.Add(new { match.Id, match.Name, match.State });
                    }
                }

                // If still no results, try very fuzzy matching
                if (matchedLocations.Count == 0)
                {
                    var fuzzyMatches = allLocations
                        .Where(l => _locationService.NormalizeLocationName(l.Name) == normalizedQuery)
                        .Take(10)
                        .ToList();
                    
                    foreach (var match in fuzzyMatches)
                    {
                        if (!matchedLocations.Any(m => m.Id == match.Id))
                        {
                            matchedLocations.Add(match);
                            matchedLocationObjects.Add(new { match.Id, match.Name, match.State });
                        }
                    }
                }

                return Ok(matchedLocationObjects.Take(50).OrderBy(l => ((dynamic)l).Name));
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while searching locations." });
            }
        }

        [HttpGet("popular")]
        public async Task<ActionResult<IEnumerable<object>>> GetPopular()
        {
            try
            {
                // Return top 8 locations by booking frequency (or all if fewer)
                var popular = await _context.Locations
                    .OrderBy(l => l.Name)
                    .Take(8)
                    .Select(l => new { l.Id, l.Name, l.State })
                    .ToListAsync();
                return Ok(popular);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while fetching popular locations." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Location>> PostLocation([FromBody] Location location)
        {
            try
            {
                if (location == null)
                {
                    return BadRequest(new { message = "Location data is required." });
                }

                if (string.IsNullOrWhiteSpace(location.Name))
                {
                    return BadRequest(new { message = "Location name is required." });
                }

                if (location.Name.Length < 2 || location.Name.Length > 100)
                {
                    return BadRequest(new { message = "Location name must be between 2 and 100 characters." });
                }

                // Check for duplicate location
                var existingLocation = await _context.Locations
                    .FirstOrDefaultAsync(l => l.Name.ToLower() == location.Name.ToLower());
                
                if (existingLocation != null)
                {
                    return Conflict(new { message = "A location with this name already exists." });
                }

                _context.Locations.Add(location);
                await _context.SaveChangesAsync();
                
                return CreatedAtAction(nameof(GetLocations), new { id = location.Id }, location);
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { message = "Failed to save location. Please try again." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
    }
}
