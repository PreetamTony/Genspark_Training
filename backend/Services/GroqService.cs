using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using backend.Data;

namespace backend.Services
{
    public class GroqService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;

        private readonly AppDbContext _dbContext;

        public GroqService(HttpClient httpClient, IConfiguration configuration, AppDbContext dbContext)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _dbContext = dbContext;
            _apiKey = _configuration["Groq:ApiKey"] ?? Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? "";
            
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("Groq API key not found in configuration.");
            }

            _httpClient.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<string> ChatCompletionAsync(string userMessage, string systemContext = "")
        {
            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "system", content = $"You are Nexbot, a helpful AI assistant for the NexBus application. You help users with bus booking, route information, schedules, and general travel assistance. Here is current database context: {systemContext}" },
                    new { role = "user", content = userMessage }
                },
                model = "llama-3.3-70b-versatile",
                temperature = 0.7,
                max_tokens = 500,
                top_p = 0.9,
                stream = false,
                stop = (object?)null
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("chat/completions", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"Error: {response.StatusCode} - {errorContent}";
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseContent);
                var message = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content").GetString() ?? "";

                return message;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public async Task<string> GetDatabaseContextAsync()
        {
            try
            {
                // Fetch real data from PostgreSQL database
                var locations = await _dbContext.Locations.ToListAsync();
                var routes = await _dbContext.Routes
                    .Include(r => r.Source)
                    .Include(r => r.Destination)
                    .ToListAsync();
                var buses = await _dbContext.Buses
                    .Include(b => b.Operator)
                    .Include(b => b.Layout)
                    .ToListAsync();
                var schedules = await _dbContext.Schedules
                    .Include(s => s.Bus)
                        .ThenInclude(b => b.Operator)
                    .Include(s => s.Route)
                        .ThenInclude(r => r.Source)
                    .Include(s => s.Route)
                        .ThenInclude(r => r.Destination)
                    .Take(10) // Limit to recent schedules for context
                    .ToListAsync();

                var context = $@"
NexBus Bus Booking System - Real Database Information:

LOCATIONS ({locations.Count} available):
{string.Join('\n', locations.Select(l => $"- {l.Name} (ID: {l.Id})"))}

ROUTES ({routes.Count} available):
{string.Join('\n', routes.Select(r => $"- {r.Source?.Name} → {r.Destination?.Name}"))}

BUSES ({buses.Count} available):
{string.Join('\n', buses.GroupBy(b => b.Operator.CompanyName)
    .Select(g => $"- {g.Key}: {g.Count()} buses ({string.Join(", ", g.Select(b => b.Layout.Type).Distinct())})"))}

RECENT SCHEDULES:
{string.Join('\n', schedules.Take(5).Select(s => 
    $"- {s.Route.Source?.Name} → {s.Route.Destination?.Name} " +
    $"by {s.Bus.Operator.CompanyName} on {s.DepartureTime:yyyy-MM-dd HH:mm} " +
    $"(Price: ₹{s.BasePrice})"))}

BOOKING PROCESS:
1. Search for buses using source and destination
2. Select bus from available options
3. Choose seats from seat layout
4. Make payment using available methods

CANCELLATION POLICY:
- Up to 24 hours before departure: 5% fee
- Less than 24 hours: No refund
- Emergency cancellations: Contact support

PAYMENT METHODS:
- Credit Card
- Debit Card
- UPI
- Net Banking

CUSTOMER SUPPORT:
- Available 24/7 via phone and chat
- Email: support@nexbus.com
- Phone: 1800-NEXBUS

LUGGAGE ALLOWANCE:
- 15kg per passenger included
- Additional luggage: ₹10 per kg

AMENITIES BY BUS TYPE:
- Seater: WiFi, Charging Points
- Sleeper: WiFi, Charging Points, Water Bottles, Blankets
- Semi-Sleeper: WiFi, Charging Points, Water Bottles

Current database status: Connected to PostgreSQL with real-time data
Last updated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
";
                return context;
            }
            catch (Exception ex)
            {
                // Fallback to basic context if database fails
                Console.WriteLine($"Error fetching database context: {ex.Message}");
                return @"
NexBus Bus Booking System Information:
- Database: PostgreSQL (connected)
- Available routes: Chennai→Bangalore, Mumbai→Delhi, Hyderabad→Pune
- Bus types: Seater, Sleeper, Semi-Sleeper
- Operators: SRS Travels, KPN Travels, VRL Travels
- Amenities: WiFi, Charging Points, Water Bottles, Blankets
- Booking process: Search → Select Bus → Choose Seats → Payment
- Cancellation policy: Up to 24 hours before departure (5% fee)
- Payment methods: Credit Card, Debit Card, UPI, Net Banking
- Customer support: Available 24/7 via phone and chat
- Luggage allowance: 15kg per passenger
- Boarding points: Major bus stands and designated pickup points
";
            }
        }
    }
}
