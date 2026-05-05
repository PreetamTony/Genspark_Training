using backend.Data;
using backend.Models;

public static class DataSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Locations.Any()) return; // Already seeded

        // Locations
        var chennai = new Location { Name = "Chennai", State = "Tamil Nadu" };
        var bangalore = new Location { Name = "Bangalore", State = "Karnataka" };
        var mumbai = new Location { Name = "Mumbai", State = "Maharashtra" };
        var pune = new Location { Name = "Pune", State = "Maharashtra" };
        var hyderabad = new Location { Name = "Hyderabad", State = "Telangana" };
        var delhi = new Location { Name = "Delhi", State = "Delhi" };
        db.Locations.AddRange(chennai, bangalore, mumbai, pune, hyderabad, delhi);
        db.SaveChanges();

        // Routes
        var route1 = new backend.Models.Route { SourceId = chennai.Id, DestinationId = bangalore.Id };
        var route2 = new backend.Models.Route { SourceId = mumbai.Id, DestinationId = pune.Id };
        var route3 = new backend.Models.Route { SourceId = hyderabad.Id, DestinationId = bangalore.Id };
        db.Routes.AddRange(route1, route2, route3);
        db.SaveChanges();

        // Layouts
        var seater = new Layout
        {
            Name = "2+2 Seater (40 seats)", Type = "Seater", TotalCapacity = 40,
            SeatConfigurationJson = GenerateSeaterConfig(10, new[] { "A", "B", "C", "D" })
        };
        var sleeper = new Layout
        {
            Name = "2+1 Sleeper (30 berths)", Type = "Sleeper", TotalCapacity = 30,
            SeatConfigurationJson = GenerateSeaterConfig(10, new[] { "L", "M", "U" })
        };
        db.Layouts.AddRange(seater, sleeper);
        db.SaveChanges();

        // Admin user
        var admin = new User { Name = "Admin", Email = "admin@nexbus.com", Role = Role.Admin, PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123") };
        db.Users.Add(admin);
        db.SaveChanges();

        // Operator user
        var opUser = new User { Name = "SRS Travels", Email = "operator@nexbus.com", Role = Role.Operator, PasswordHash = BCrypt.Net.BCrypt.HashPassword("operator123") };
        db.Users.Add(opUser);
        db.SaveChanges();

        var opProfile = new OperatorProfile { 
            UserId = opUser.Id, 
            CompanyName = "SRS Travels", 
            Status = OperatorStatus.Active,
            HeadOfficeLocationId = chennai.Id // Set Chennai as head office
        };
        db.OperatorProfiles.Add(opProfile);
        db.SaveChanges();

        // Sample bus with comprehensive features
        var bus = new Bus 
        { 
            OperatorProfileId = opProfile.Id, 
            LayoutId = seater.Id, 
            RegistrationNumber = "TN01AB1234", 
            Status = BusStatus.Active,
            // Bus Features
            HasWaterBottle = true,
            HasBlankets = true,
            HasChargingPoint = true,
            HasCCTV = true,
            HasToilet = false,
            HasWiFi = true,
            HasReadingLight = true,
            HasEmergencyExit = true,
            HasGPS = true,
            // Bus Type
            BusType = "Volvo Multi-Axle A/C Semi Sleeper (2+2)",
            // Ratings and Performance
            Rating = 4.9,
            TotalRatings = 1116,
            OnTimeTrips = 940,
            TotalTrips = 950,
            // Policies
            CancellationPolicy = "Before 25th Apr 11:10 AM - 85%; From 25th Apr 11:10 AM Until 25th Apr 03:10 PM - 70%; From 25th Apr 03:10 PM Until 25th Apr 07:10 PM - 40%; From 25th Apr 07:10 PM Until 25th Apr 11:10 PM - 5%",
            ReschedulePolicy = "Before 25th Apr 04:10 PM - FREE",
            ChildPolicy = "Children above the age of 3 will need a ticket",
            LuggagePolicy = "1 pieces of luggage will be accepted free of charge per passenger. Excess items will be chargeable",
            PetPolicy = "Pets are not allowed",
            ToiletPolicy = "In Bus Toilet with Only Urinal Facility",
            LiquorPolicy = "Carrying or consuming liquor inside the bus is prohibited. Bus operator reserves the right to deboard drunk passengers."
        };
        db.Buses.Add(bus);
        db.SaveChanges();

        // Generate seats for bus
        var seatConfig = System.Text.Json.JsonSerializer.Deserialize<List<System.Text.Json.JsonElement>>(seater.SeatConfigurationJson)!;
        foreach (var s in seatConfig)
            db.Seats.Add(new Seat { BusId = bus.Id, SeatNumber = s.GetProperty("label").GetString()! });
        db.SaveChanges();

        // Tomorrow's schedule on Chennai → Bangalore route
        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        db.Schedules.Add(new Schedule
        {
            BusId = bus.Id, RouteId = route1.Id,
            DepartureTime = tomorrow.AddHours(21),  // 9 PM
            ArrivalTime = tomorrow.AddHours(27),    // 3 AM next day
            BasePrice = 750, Status = ScheduleStatus.Scheduled,
            PickupPoint = "Koyambedu Bus Stand", DropPoint = "Majestic Bus Stand"
        });

        // Day after tomorrow schedule
        var dayAfter = tomorrow.AddDays(1);
        db.Schedules.Add(new Schedule
        {
            BusId = bus.Id, RouteId = route1.Id,
            DepartureTime = dayAfter.AddHours(8),   // 8 AM
            ArrivalTime = dayAfter.AddHours(13.5),  // 1:30 PM
            BasePrice = 600, Status = ScheduleStatus.Scheduled,
            PickupPoint = "Koyambedu Bus Stand", DropPoint = "Silk Board"
        });

        // Add sample boarding points for the first schedule
        var schedule1 = db.Schedules.First(s => s.DepartureTime.Date == tomorrow.Date);
        var boardingPoints = new[]
        {
            new BoardingPoint { ScheduleId = schedule1.Id, LocationId = chennai.Id, PointName = "Chennai", Address = "Koyambedu Bus Stand", Time = TimeSpan.FromHours(21), Order = 1 },
            new BoardingPoint { ScheduleId = schedule1.Id, LocationId = chennai.Id, PointName = "Padur", Address = "Opp To Indian Oil Bunk - towards siruseri", Time = TimeSpan.FromHours(21.08), Order = 2 },
            new BoardingPoint { ScheduleId = schedule1.Id, LocationId = chennai.Id, PointName = "Siruseri", Address = "Infront of HDFC ATM, Opp to Adyar anandha Bhavan - Siruseri", Time = TimeSpan.FromHours(21.17), Order = 3 },
            new BoardingPoint { ScheduleId = schedule1.Id, LocationId = chennai.Id, PointName = "Navalur", Address = "Infront Of Vivera Mall - towards sholinganallur", Time = TimeSpan.FromHours(21.25), Order = 4 }
        };
        db.BoardingPoints.AddRange(boardingPoints);

        // Add sample dropping points for the first schedule
        var droppingPoints = new[]
        {
            new DroppingPoint { ScheduleId = schedule1.Id, LocationId = bangalore.Id, PointName = "Bangalore", Address = "Majestic Bus Stand", Time = TimeSpan.FromHours(3), Order = 1 },
            new DroppingPoint { ScheduleId = schedule1.Id, LocationId = bangalore.Id, PointName = "Attibele", Address = "After Attibele Toll Gate towards Hosur", Time = TimeSpan.FromHours(3.42), Order = 2 },
            new DroppingPoint { ScheduleId = schedule1.Id, LocationId = bangalore.Id, PointName = "Bommasandra", Address = "Opp To Narayana Hospital", Time = TimeSpan.FromHours(3.42), Order = 3 },
            new DroppingPoint { ScheduleId = schedule1.Id, LocationId = bangalore.Id, PointName = "Chandapura", Address = "Infront Of R K Dhaba and Restaurant, Oppsite To Shell Petrol Bunk", Time = TimeSpan.FromHours(3.5), Order = 4 }
        };
        db.DroppingPoints.AddRange(droppingPoints);

        // Add sample bus reviews
        var reviews = new[]
        {
            new BusReview { BusId = bus.Id, UserId = 1, Rating = 5, Comment = "Excellent service, very comfortable journey", StaffBehavior = 5, Punctuality = 5, Cleanliness = 5, SeatComfort = 5, Driving = 5, AC = 5, LiveTracking = 4, RestStopHygiene = 4 },
            new BusReview { BusId = bus.Id, UserId = 2, Rating = 4, Comment = "Good experience, bus was on time", StaffBehavior = 4, Punctuality = 5, Cleanliness = 4, SeatComfort = 4, Driving = 4, AC = 4, LiveTracking = 4, RestStopHygiene = 3 },
            new BusReview { BusId = bus.Id, UserId = 1, Rating = 5, Comment = "Very clean and well-maintained bus", StaffBehavior = 5, Punctuality = 4, Cleanliness = 5, SeatComfort = 5, Driving = 4, AC = 5, LiveTracking = 5, RestStopHygiene = 4 }
        };
        db.BusReviews.AddRange(reviews);

        // Platform convenience fee
        db.PlatformConfigs.Add(new PlatformConfig { Key = "ConvenienceFee", Value = "50" });

        // Add test coupons for demo purposes
        var coupons = new[]
        {
            new Coupon 
            { 
                OperatorId = opProfile.Id, 
                Code = "SAVE20", 
                DiscountPercent = 20, 
                DiscountAmount = 0,
                ValidFrom = DateTime.UtcNow.AddDays(-1),
                ValidTo = DateTime.UtcNow.AddDays(30),
                IsActive = true
            },
            new Coupon 
            { 
                OperatorId = opProfile.Id, 
                Code = "SAVE10", 
                DiscountPercent = 10, 
                DiscountAmount = 0,
                ValidFrom = DateTime.UtcNow.AddDays(-1),
                ValidTo = DateTime.UtcNow.AddDays(30),
                IsActive = true
            },
            new Coupon 
            { 
                OperatorId = opProfile.Id, 
                Code = "SUMMER", 
                DiscountPercent = 15, 
                DiscountAmount = 0,
                ValidFrom = DateTime.UtcNow.AddDays(-1),
                ValidTo = DateTime.UtcNow.AddDays(30),
                IsActive = true
            }
        };
        db.Coupons.AddRange(coupons);
        db.SaveChanges();
    }

    private static string GenerateSeaterConfig(int rows, string[] cols)
    {
        var seats = new List<object>();
        for (int r = 1; r <= rows; r++)
            for (int c = 0; c < cols.Length; c++)
                seats.Add(new { row = r, col = cols[c], label = $"{r}{cols[c]}" });
        return System.Text.Json.JsonSerializer.Serialize(seats);
    }
}
