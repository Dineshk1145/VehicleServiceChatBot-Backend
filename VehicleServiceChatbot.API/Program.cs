using System;
using System.Linq;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using VehicleServiceChatbot.API.Middlewares;
using VehicleServiceChatbot.Application.DTOs.Auth;
using VehicleServiceChatbot.Application.Interfaces;
using VehicleServiceChatbot.Application.Services;
using VehicleServiceChatbot.Domain.Entities;
using VehicleServiceChatbot.Persistence.Context;
using VehicleServiceChatbot.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ==============================================================================
// 1. ENVIRONMENT VARIABLES & SENSITIVE INFORMATION CONFIGURATION
// ==============================================================================
// Sensitive credentials can be loaded from environment variables in production,
// with safe fallback to appsettings.json for local development.
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? builder.Configuration["Jwt:Key"]
    ?? "SuperSecretKeyForVehicleServiceChatbotWhichIsAtLeast32BytesLong!";

var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
    ?? builder.Configuration["Jwt:Issuer"]
    ?? "VehicleServiceChatbotAPI";

var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
    ?? builder.Configuration["Jwt:Audience"]
    ?? "VehicleServiceChatbotFrontend";

var allowedOriginsEnv = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS");
var configuredOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173", "https://localhost:5173", "http://localhost:3000", "https://localhost:3000" };

if (!string.IsNullOrWhiteSpace(allowedOriginsEnv))
{
    configuredOrigins = allowedOriginsEnv.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

// ==============================================================================
// 2. DATABASE CONFIGURATION (SQL Server Primary / SQLite Fallback)
// ==============================================================================
bool useSqlServer = false;
if (!string.IsNullOrEmpty(connectionString))
{
    try
    {
        var builderSql = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master"
        };
        using (var connection = new Microsoft.Data.SqlClient.SqlConnection(builderSql.ConnectionString))
        {
            connection.Open();
            useSqlServer = true;
            Console.WriteLine("Successfully connected to SQL Server (master database)!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"SQL Server connection failed: {ex.Message}. Falling back to SQLite database.");
    }
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (useSqlServer)
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        options.UseSqlite("Data Source=vehicleservice.db");
    }
});

// ==============================================================================
// 3. DEPENDENCY INJECTION MAPPINGS
// ==============================================================================
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IServiceBookingRepository, ServiceBookingRepository>();
builder.Services.AddScoped<IServiceBookingService, ServiceBookingService>();
builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
builder.Services.AddScoped<IChatMessageService, ChatMessageService>();
builder.Services.AddScoped<IChatbotService, ChatbotService>();

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, DummyPaymentService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IInvoiceService, PdfInvoiceService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IServicePackageRepository, ServicePackageRepository>();

// ==============================================================================
// 4. SECURE CORS POLICY
// ==============================================================================
// Security: Restrict allowed origins to explicitly trusted hosts rather than open wildcard.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(configuredOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ==============================================================================
// 5. RATE LIMITING & THROTTLING (ASP.NET Core .NET 8)
// ==============================================================================
// Prevents brute-force attacks, credential stuffing, and bot/DDoS abuse.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Global limiter: 100 requests per minute per IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "global_client",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            }));

    // Strict Auth Limiter: 10 login/register requests per minute per IP to stop brute-forcing
    options.AddPolicy("AuthLimiter", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "auth_client",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 2
            }));

    // Chatbot Limiter: 20 queries per minute per User/IP to prevent assistant spam
    options.AddPolicy("ChatbotLimiter", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "chat_client",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 5
            }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            "{\"statusCode\":429,\"message\":\"Rate limit exceeded. Too many requests. Please try again in a minute.\"}",
            cancellationToken: token);
    };
});

// ==============================================================================
// 6. JWT AUTHENTICATION & TOKEN VALIDATION
// ==============================================================================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.FromMinutes(1) // Reduce default 5min grace period to prevent expired token use
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// ==============================================================================
// 7. SWAGGER / OPENAPI SPECIFICATION
// ==============================================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Vehicle Service Chatbot API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ==============================================================================
// 8. DATABASE AUTO-MIGRATION & INITIAL SEEDING
// ==============================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        context.Database.EnsureCreated();
        EnsureDatabaseSchemaUpdated(context);
        
        // Seed default users if table is empty
        if (!context.Users.Any())
        {
            var authService = services.GetRequiredService<IAuthService>();
            
            // Seed Admin User
            await authService.RegisterAsync(new RegisterRequestDto
            {
                Name = "Administrator",
                Email = "admin@autocare.com",
                Password = "admin123"
            });
            
            // Explicitly set role for seeded admin in DB
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@autocare.com");
            if (adminUser != null)
            {
                adminUser.Role = "Admin";
                await context.SaveChangesAsync();
            }

            // Seed Customer User
            await authService.RegisterAsync(new RegisterRequestDto
            {
                Name = "John Doe",
                Email = "user@autocare.com",
                Password = "user123"
            });
            
            Console.WriteLine("Database seeded successfully with default Admin and Customer accounts!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during database initialization/seeding: {ex.Message}");
    }
}

// ==============================================================================
// 9. HTTP REQUEST PIPELINE CONFIGURATION
// ==============================================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Security: Attach standard HTTP security headers (anti-clickjacking, nosniff, etc.)
app.UseMiddleware<SecurityHeadersMiddleware>();

// Global exception handling & logging without leaking stack traces in production
app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

// Enforce strict CORS
app.UseCors("AllowFrontend");

// Enforce Rate Limiting & Throttling
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// ==============================================================================
// HELPER: Schema Auto-Patching
// ==============================================================================
static void EnsureDatabaseSchemaUpdated(ApplicationDbContext context)
{
    try
    {
        bool isSqlServer = context.Database.IsSqlServer();
        bool isSqlite = context.Database.IsSqlite();

        if (isSqlServer)
        {
            string sqlServerPatch = @"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ServiceBookings')
                BEGIN
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ServiceBookings' AND COLUMN_NAME = 'Price')
                        ALTER TABLE [ServiceBookings] ADD [Price] decimal(18,2) NOT NULL DEFAULT 0;
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ServiceBookings' AND COLUMN_NAME = 'Tax')
                        ALTER TABLE [ServiceBookings] ADD [Tax] decimal(18,2) NOT NULL DEFAULT 0;
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ServiceBookings' AND COLUMN_NAME = 'Discount')
                        ALTER TABLE [ServiceBookings] ADD [Discount] decimal(18,2) NOT NULL DEFAULT 0;
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ServiceBookings' AND COLUMN_NAME = 'TotalAmount')
                        ALTER TABLE [ServiceBookings] ADD [TotalAmount] decimal(18,2) NOT NULL DEFAULT 0;
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ServiceBookings' AND COLUMN_NAME = 'PaymentStatus')
                        ALTER TABLE [ServiceBookings] ADD [PaymentStatus] nvarchar(50) NOT NULL DEFAULT 'Pending';
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ServiceBookings' AND COLUMN_NAME = 'Notes')
                        ALTER TABLE [ServiceBookings] ADD [Notes] nvarchar(max) NULL;
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ServiceBookings' AND COLUMN_NAME = 'CreatedAt')
                        ALTER TABLE [ServiceBookings] ADD [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE();
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ServiceBookings' AND COLUMN_NAME = 'UpdatedAt')
                        ALTER TABLE [ServiceBookings] ADD [UpdatedAt] datetime2 NULL;
                END

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Payments')
                BEGIN
                    CREATE TABLE [Payments] (
                        [Id] int IDENTITY(1,1) PRIMARY KEY,
                        [PaymentId] nvarchar(100) NOT NULL,
                        [BookingId] int NOT NULL,
                        [UserId] int NOT NULL,
                        [Amount] decimal(18,2) NOT NULL,
                        [PaymentStatus] nvarchar(50) NOT NULL,
                        [TransactionId] nvarchar(100) NOT NULL,
                        [PaymentDate] datetime2 NOT NULL,
                        [PaymentMethod] nvarchar(50) NOT NULL
                    );
                END

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Notifications')
                BEGIN
                    CREATE TABLE [Notifications] (
                        [Id] int IDENTITY(1,1) PRIMARY KEY,
                        [UserId] int NOT NULL,
                        [BookingId] int NULL,
                        [Message] nvarchar(max) NOT NULL,
                        [IsRead] bit NOT NULL DEFAULT 0,
                        [CreatedAt] datetime2 NOT NULL
                    );
                END

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ServicePackages')
                BEGIN
                    CREATE TABLE [ServicePackages] (
                        [Id] nvarchar(100) PRIMARY KEY,
                        [Name] nvarchar(200) NOT NULL,
                        [Price] decimal(18,2) NOT NULL,
                        [Duration] nvarchar(100) NOT NULL,
                        [Description] nvarchar(max) NOT NULL
                    );
                END
            ";
            context.Database.ExecuteSqlRaw(sqlServerPatch);
        }
        else if (isSqlite)
        {
            try { context.Database.ExecuteSqlRaw("ALTER TABLE ServiceBookings ADD COLUMN Price NUMERIC NOT NULL DEFAULT 0;"); } catch {}
            try { context.Database.ExecuteSqlRaw("ALTER TABLE ServiceBookings ADD COLUMN Tax NUMERIC NOT NULL DEFAULT 0;"); } catch {}
            try { context.Database.ExecuteSqlRaw("ALTER TABLE ServiceBookings ADD COLUMN Discount NUMERIC NOT NULL DEFAULT 0;"); } catch {}
            try { context.Database.ExecuteSqlRaw("ALTER TABLE ServiceBookings ADD COLUMN TotalAmount NUMERIC NOT NULL DEFAULT 0;"); } catch {}
            try { context.Database.ExecuteSqlRaw("ALTER TABLE ServiceBookings ADD COLUMN PaymentStatus TEXT NOT NULL DEFAULT 'Pending';"); } catch {}
            try { context.Database.ExecuteSqlRaw("ALTER TABLE ServiceBookings ADD COLUMN Notes TEXT NULL;"); } catch {}
            try { context.Database.ExecuteSqlRaw("ALTER TABLE ServiceBookings ADD COLUMN CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP;"); } catch {}
            try { context.Database.ExecuteSqlRaw("ALTER TABLE ServiceBookings ADD COLUMN UpdatedAt TEXT NULL;"); } catch {}

            string sqliteTables = @"
                CREATE TABLE IF NOT EXISTS Payments (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PaymentId TEXT NOT NULL,
                    BookingId INTEGER NOT NULL,
                    UserId INTEGER NOT NULL,
                    Amount NUMERIC NOT NULL,
                    PaymentStatus TEXT NOT NULL,
                    TransactionId TEXT NOT NULL,
                    PaymentDate TEXT NOT NULL,
                    PaymentMethod TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Notifications (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER NOT NULL,
                    BookingId INTEGER NULL,
                    Message TEXT NOT NULL,
                    IsRead INTEGER NOT NULL DEFAULT 0,
                    CreatedAt TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS ServicePackages (
                    Id TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Price NUMERIC NOT NULL,
                    Duration TEXT NOT NULL,
                    Description TEXT NOT NULL
                );
            ";
            context.Database.ExecuteSqlRaw(sqliteTables);
        }

        // Seed ServicePackages if empty
        if (!context.ServicePackages.Any())
        {
            context.ServicePackages.AddRange(
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
            context.SaveChanges();
        }

        Console.WriteLine("Database schema auto-patching completed successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database schema auto-patching warning: {ex.Message}");
    }
}
