using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public interface ILocationService
    {
        Task<Location?> FindLocationAsync(string searchQuery);
        Task<List<Location>> GetAllLocationsAsync();
        string NormalizeLocationName(string locationName);
    }

    public class LocationService : ILocationService
    {
        private readonly AppDbContext _context;
        
        // Dictionary of location aliases and variations
        private readonly Dictionary<string, string> _locationAliases = new()
        {
            // Bangalore variations
            { "bengaluru", "Bangalore" },
            { "bengalooru", "Bangalore" },
            { "bangalore", "Bangalore" },
            { "bangaluru", "Bangalore" },
            { "blore", "Bangalore" },
            { "bengaloor", "Bangalore" },
            
            // Chennai variations
            { "chennai", "Chennai" },
            { "madras", "Chennai" },
            { "madrasapattinam", "Chennai" },
            { "chenai", "Chennai" },
            
            // Mumbai variations
            { "mumbai", "Mumbai" },
            { "bombay", "Mumbai" },
            { "mambai", "Mumbai" },
            { "mumbhai", "Mumbai" },
            
            // Delhi variations
            { "delhi", "Delhi" },
            { "new delhi", "Delhi" },
            { "dilli", "Delhi" },
            { "ndelhi", "Delhi" },
            
            // Kolkata variations
            { "kolkata", "Kolkata" },
            { "calcutta", "Kolkata" },
            { "kalkata", "Kolkata" },
            { "kol kata", "Kolkata" },
            
            // Pune variations
            { "pune", "Pune" },
            { "poona", "Pune" },
            { "punya", "Pune" },
            
            // Hyderabad variations
            { "hyderabad", "Hyderabad" },
            { "hyd", "Hyderabad" },
            { "hderabad", "Hyderabad" },
            { "hyderbad", "Hyderabad" },
            
            // Ahmedabad variations
            { "ahmedabad", "Ahmedabad" },
            { "ahmadabad", "Ahmedabad" },
            { "amdavad", "Ahmedabad" },
            
            // Jaipur variations
            { "jaipur", "Jaipur" },
            { "jaypur", "Jaipur" },
            { "jeypur", "Jaipur" },
            { "pink city", "Jaipur" },
            
            // Lucknow variations
            { "lucknow", "Lucknow" },
            { "laknau", "Lucknow" },
            
            // Coimbatore variations
            { "coimbatore", "Coimbatore" },
            { "kovai", "Coimbatore" },
            { "coimbator", "Coimbatore" },
            
            // Madurai variations
            { "madurai", "Madurai" },
            { "madhurai", "Madurai" },
            
            // Trichy variations
            { "trichy", "Trichy" },
            { "tiruchirappalli", "Trichy" },
            { "trichirapalli", "Trichy" },
            { "tiruchy", "Trichy" },
            
            // Salem variations
            { "salem", "Salem" },
            { "selam", "Salem" },
            { "salam", "Salem" },
            
            // Tirupur variations
            { "tirupur", "Tirupur" },
            { "tirupoor", "Tirupur" },
            
            // Erode variations
            { "erode", "Erode" },
            { "erod", "Erode" },
            { "iyerode", "Erode" },
            
            // Vellore variations
            { "vellore", "Vellore" },
            { "velur", "Vellore" },
            { "velor", "Vellore" },
            
            // Tirunelveli variations
            { "tirunelveli", "Tirunelveli" },
            { "neli", "Tirunelveli" },
            { "tirunelvelli", "Tirunelveli" },
            
            // Thanjavur variations
            { "thanjavur", "Thanjavur" },
            { "tanjore", "Thanjavur" },
            { "thanjavoor", "Thanjavur" }
        };

        public LocationService(AppDbContext context)
        {
            _context = context;
        }

        public string NormalizeLocationName(string locationName)
        {
            if (string.IsNullOrWhiteSpace(locationName))
                return locationName;

            // Remove extra spaces and convert to lowercase
            var normalized = locationName.Trim().ToLowerInvariant();
            
            // Remove common suffixes and prefixes
            normalized = normalized.Replace(" city", "")
                                   .Replace(" town", "")
                                   .Replace(" district", "")
                                   .Replace(" district", "")
                                   .Replace(" railway station", "")
                                   .Replace(" airport", "")
                                   .Replace(" bus stand", "")
                                   .Replace(" bus station", "")
                                   .Trim();

            // Check if it's in our aliases dictionary
            if (_locationAliases.TryGetValue(normalized, out var standardName))
            {
                return standardName;
            }

            // Try fuzzy matching using Levenshtein distance
            return FindClosestMatch(normalized) ?? locationName;
        }

        private string? FindClosestMatch(string search)
        {
            string? bestMatch = null;
            int bestDistance = int.MaxValue;

            foreach (var alias in _locationAliases.Keys)
            {
                var distance = LevenshteinDistance(search, alias);
                if (distance < bestDistance && distance <= 2) // Allow up to 2 character differences
                {
                    bestDistance = distance;
                    bestMatch = _locationAliases[alias];
                }
            }

            return bestMatch;
        }

        private int LevenshteinDistance(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1)) return string.IsNullOrEmpty(s2) ? 0 : s2.Length;
            if (string.IsNullOrEmpty(s2)) return s1.Length;

            int[,] dp = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                dp[i, 0] = i;

            for (int j = 0; j <= s2.Length; j++)
                dp[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                    dp[i, j] = Math.Min(
                        Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                        dp[i - 1, j - 1] + cost);
                }
            }

            return dp[s1.Length, s2.Length];
        }

        public async Task<Location?> FindLocationAsync(string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return null;

            var normalizedSearch = NormalizeLocationName(searchQuery);
            
            // First try exact match with normalized name
            var location = await _context.Locations
                .FirstOrDefaultAsync(l => l.Name.Equals(normalizedSearch, StringComparison.OrdinalIgnoreCase));

            if (location != null)
                return location;

            // If no exact match, try partial match
            location = await _context.Locations
                .FirstOrDefaultAsync(l => l.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                                       searchQuery.Contains(l.Name, StringComparison.OrdinalIgnoreCase));

            return location;
        }

        public async Task<List<Location>> GetAllLocationsAsync()
        {
            return await _context.Locations.OrderBy(l => l.Name).ToListAsync();
        }
    }
}
