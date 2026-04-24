using Microsoft.EntityFrameworkCore;
using backend.Models;
using BusRoute = backend.Models.Route;

namespace backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<OperatorProfile> OperatorProfiles { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<BusRoute> Routes { get; set; }
        public DbSet<Layout> Layouts { get; set; }
        public DbSet<Bus> Buses { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingSeat> BookingSeats { get; set; }
        public DbSet<PlatformConfig> PlatformConfigs { get; set; }

        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<BoardingPoint> BoardingPoints { get; set; }
        public DbSet<DroppingPoint> DroppingPoints { get; set; }
        public DbSet<BusReview> BusReviews { get; set; }
        public DbSet<RestStop> RestStops { get; set; }
        public DbSet<Passenger> Passengers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure PostgreSQL to handle DateTime properly
            if (Database.IsNpgsql())
            {
                modelBuilder.Entity<Schedule>()
                    .Property(s => s.DepartureTime)
                    .HasColumnType("timestamp with time zone");

                modelBuilder.Entity<Schedule>()
                    .Property(s => s.ArrivalTime)
                    .HasColumnType("timestamp with time zone");

                modelBuilder.Entity<Booking>()
                    .Property(b => b.BookingDate)
                    .HasColumnType("timestamp with time zone");
            }

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // BookingSeat composite key
            modelBuilder.Entity<BookingSeat>()
                .HasKey(bs => new { bs.BookingId, bs.SeatId });

            // Route → Source/Destination
            modelBuilder.Entity<BusRoute>()
                .HasOne(r => r.Source)
                .WithMany()
                .HasForeignKey(r => r.SourceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BusRoute>()
                .HasOne(r => r.Destination)
                .WithMany()
                .HasForeignKey(r => r.DestinationId)
                .OnDelete(DeleteBehavior.Restrict);

            // OperatorProfile head office
            modelBuilder.Entity<OperatorProfile>()
                .HasOne(op => op.HeadOfficeLocation)
                .WithMany()
                .HasForeignKey(op => op.HeadOfficeLocationId)
                .OnDelete(DeleteBehavior.SetNull);

            // Bus → OperatorProfile
            modelBuilder.Entity<Bus>()
                .HasOne(b => b.Operator)
                .WithMany()
                .HasForeignKey(b => b.OperatorProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking → User
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
