using System.Text;
using backend.Data;
using backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// Add configuration to read .env file
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

// Swagger with JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BusBooking API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization: Bearer {token}",
        Name = "Authorization", In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey, Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {{
        new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
        Array.Empty<string>()
    }});
});

// Database: Use InMemory for development (no PostgreSQL needed), PostgreSQL for production
var useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDatabase", true);
if (useInMemory)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("BusBookingDb"));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// In-Memory Distributed Cache (swap for AddStackExchangeRedisCache in production)
builder.Services.AddDistributedMemoryCache();

// Register custom services
builder.Services.AddScoped<ISeatLockService, SeatLockService>();
builder.Services.AddScoped<ICancellationService, CancellationService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<NexBus.Services.IEmailService, NexBus.Services.EmailService>();

// JWT Auth
var jwtKey = builder.Configuration["Jwt:Key"]!;
var keyBytes = Encoding.ASCII.GetBytes(jwtKey);
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = true, ValidateAudience = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"]
    };
});

builder.Services.AddAuthorization();

// Add Groq Service
builder.Services.AddHttpClient<GroqService>();
builder.Configuration.AddEnvironmentVariables();

// CORS - Allow any origin for development to avoid port conflicts
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


var app = builder.Build();

app.UseRouting(); // Explicit routing

// Seed default data for development
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
DataSeeder.Seed(db);
Console.WriteLine("Database seeded successfully.");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Console.WriteLine("Backend is ready on http://localhost:5047");
app.Run();
