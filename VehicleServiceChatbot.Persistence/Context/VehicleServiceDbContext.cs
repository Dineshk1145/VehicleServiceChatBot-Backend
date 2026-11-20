using Microsoft.EntityFrameworkCore;
using VehicleServiceChatbot.Domain.Entities;

namespace VehicleServiceChatbot.Persistence.Context
{
    public class VehicleServiceDbContext : DbContext
    {
        public VehicleServiceDbContext(DbContextOptions<VehicleServiceDbContext> options)
            : base(options)
        {
        }

        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<ServicePackage> ServicePackages { get; set; } = null!;
        public DbSet<ChatSession> ChatSessions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Entity Primary Keys
            modelBuilder.Entity<Booking>().HasKey(b => b.Id);
            modelBuilder.Entity<ServicePackage>().HasKey(s => s.Id);
            modelBuilder.Entity<ChatSession>().HasKey(c => c.SessionId);

            // Seed Default Service Packages
            modelBuilder.Entity<ServicePackage>().HasData(
                new ServicePackage
                {
                    Id = "oil-change",
                    Name = "Basic Oil & Filter Change",
                    Price = 49.99m,
                    Duration = "45 mins",
                    Description = "Premium synthetic blend oil, filter replacement, fluid top-off, and 15-point safety inspection."
                },
                new ServicePackage
                {
                    Id = "general-service",
                    Name = "General Routine Service",
                    Price = 119.99m,
                    Duration = "1.5 hours",
                    Description = "Comprehensive vehicle diagnostic scan, oil & filter change, tire rotation, brake check, and battery health test."
                },
                new ServicePackage
                {
                    Id = "brake-service",
                    Name = "Advanced Brake Service",
                    Price = 179.99m,
                    Duration = "2 hours",
                    Description = "Brake pad replacement, rotor inspection, brake fluid flush, and dynamic road testing."
                },
                new ServicePackage
                {
                    Id = "ac-tuneup",
                    Name = "AC Tuning & Recharge",
                    Price = 89.99m,
                    Duration = "1 hour",
                    Description = "AC pressure check, leak inspection using UV dye, refrigerant recharge, and cabin air filter evaluation."
                },
                new ServicePackage
                {
                    Id = "car-detail",
                    Name = "Full Detail & Sanitization",
                    Price = 149.99m,
                    Duration = "3 hours",
                    Description = "Complete exterior hand wash, clay bar treatment, protective wax, deep interior vacuuming, steam cleaning, and sanitization."
                }
            );
        }
    }
}
