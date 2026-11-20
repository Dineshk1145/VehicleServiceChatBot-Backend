using Microsoft.EntityFrameworkCore;
using VehicleServiceChatbot.Domain.Entities;

namespace VehicleServiceChatbot.Persistence.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Vehicle> Vehicles { get; set; } = null!;
    public DbSet<ServiceBooking> ServiceBookings { get; set; } = null!;
    public DbSet<ChatMessage> ChatMessages { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<ServicePackage> ServicePackages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Decimal Precisions
        modelBuilder.Entity<ServiceBooking>(entity =>
        {
            entity.Property(b => b.Price).HasPrecision(18, 2);
            entity.Property(b => b.Tax).HasPrecision(18, 2);
            entity.Property(b => b.Discount).HasPrecision(18, 2);
            entity.Property(b => b.TotalAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(p => p.Amount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<ServicePackage>(entity =>
        {
            entity.Property(s => s.Price).HasPrecision(18, 2);
        });

        // Seed Default Service Packages
        modelBuilder.Entity<ServicePackage>().HasData(
            new ServicePackage
            {
                Id = "oil-change",
                Name = "Basic Oil & Filter Change",
                Price = 49.99m,
                Duration = "45 mins",
                Description = "Premium synthetic blend oil, filter replacement, fluid top-off, and safety inspection."
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
                Description = "Complete exterior hand wash, clay bar treatment, protective wax, deep interior vacuuming, and sanitization."
            },
            new ServicePackage
            {
                Id = "engine-diag",
                Name = "Full Engine Diagnostics",
                Price = 99.99m,
                Duration = "1 hour",
                Description = "Full electronic computer diagnostic scan, sensor check, emission testing, and report."
            }
        );
    }
}
