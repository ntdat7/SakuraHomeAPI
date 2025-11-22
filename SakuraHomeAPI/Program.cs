using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SakuraHomeAPI.Data;
using SakuraHomeAPI.Hubs;
using SakuraHomeAPI.Models.Entities.Identity;
using SakuraHomeAPI.Services.Implementations;
using SakuraHomeAPI.Services.Interfaces;
using SakuraHomeAPI.Tools;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// ✅ Configuration: Support both local and Azure environments
// Load optional local configuration file (only exists in local development)
var localConfigPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
    "SakuraHome",
    "appsettings.Local.json"
);

if (File.Exists(localConfigPath))
{
    builder.Configuration.AddJsonFile(localConfigPath, optional: true, reloadOnChange: true);
    Console.WriteLine($"Loaded local configuration from: {localConfigPath}");
}

// Add environment variables (Azure App Service uses this)
builder.Configuration.AddEnvironmentVariables();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console()
    .WriteTo.File(
        path: builder.Environment.IsProduction()
            ? "D:\\home\\LogFiles\\Application\\sakura-home-.txt"  // Azure path
            : "logs/sakura-home-.txt",  // Local path
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30
    )
    .CreateLogger();

builder.Host.UseSerilog();

Log.Information("Starting Sakura Home API in {Environment} environment", builder.Environment.EnvironmentName);

// Add services to the container.

// Database Configuration - FIXED: Support both local and Azure SQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Database connection string 'DefaultConnection' not found.");
    }

    options.UseSqlServer(connectionString,
        sqlOptions =>
        {
            sqlOptions.MigrationsAssembly("SakuraHomeAPI");
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);

            // Azure SQL specific settings
            if (builder.Environment.IsProduction())
            {
                sqlOptions.CommandTimeout(180); // 3 minutes for production
            }
        });

    // Enable sensitive data logging only in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Identity Configuration - FIXED: Using Guid as key type to match your User model
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    // Password settings - RELAXED for admin user creation
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 3;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    // Sign in settings
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication - FIXED: Better error handling for missing configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtKey = jwtSettings["Key"];

if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Key is not configured. Please set JwtSettings:Key in configuration or environment variables.");
}

var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = builder.Environment.IsProduction();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RequireExpirationTime = true
    };

    // Event handlers for better debugging
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Log.Warning("JWT Authentication failed: {Error}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Log.Debug("JWT Token validated for user: {User}", context.Principal?.Identity?.Name);
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            // Allow JWT token from query string for SignalR
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
                Log.Debug("JWT token received from query string for SignalR connection");
            }

            return Task.CompletedTask;
        }
    };
});

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin", "SuperAdmin"));
    options.AddPolicy("StaffOnly", policy => policy.RequireRole("Staff", "Admin", "SuperAdmin"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
});

// Controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
    });

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Memory Cache
builder.Services.AddMemoryCache();

// CORS - FIXED: Dynamic configuration based on environment
builder.Services.AddCors(options =>
{
    // Development CORS policy
    options.AddPolicy("Development", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000",
                "http://localhost:5173",
                "https://localhost:5173",
                "http://localhost:5174",
                "https://localhost:5174")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetIsOriginAllowedToAllowWildcardSubdomains();
    });

    // Production CORS policy - get from configuration
    var productionOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? new[] { "https://yourdomain.com" };

    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins(productionOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Sakura Home API",
        Version = "v1",
        Description = "API for Japanese Products E-commerce Platform",
        Contact = new OpenApiContact
        {
            Name = "Sakura Home Team",
            Email = "dev@sakurahome.com"
        }
    });

    // JWT Authentication in Swagger
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

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Custom Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IShippingService, ShippingService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<SakuraHomeAPI.Repositories.Interfaces.IProductRepository, SakuraHomeAPI.Repositories.Implementations.ProductRepository>();

// SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.MaximumReceiveMessageSize = 32768;
    options.StreamBufferCapacity = 10;
})
.AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.PayloadSerializerOptions.WriteIndented = false;
    options.PayloadSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

// Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

var app = builder.Build();

// ✅ FIXED: Dynamic URL configuration - works on both local and Azure
if (!builder.Environment.IsProduction())
{
    // Local development only
    app.Urls.Clear();
    app.Urls.Add("https://localhost:8080");
}
// Azure App Service handles URLs automatically in production

// Configure the HTTP request pipeline.

// Swagger - Available in both Development and Production (can be disabled in prod if needed)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sakura Home API V1");
        c.RoutePrefix = string.Empty;
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });
}
else
{
    // Production: Enable Swagger but with different route
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sakura Home API V1");
        c.RoutePrefix = "api-docs"; // Access via /api-docs in production
    });

    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCompression();

// Static Files
app.UseStaticFiles();

// CORS - Environment-based
app.UseCors(builder.Environment.IsDevelopment() ? "Development" : "Production");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Health Checks
app.UseHealthChecks("/health");

// Controllers
app.MapControllers();

// SignalR Hubs
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<NotificationHub>("/notificationHub");

// Database Migration and Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        Log.Information("Starting database migration and seeding...");

        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        // Test database connection first
        if (await context.Database.CanConnectAsync())
        {
            Log.Information("Database connection successful");

            // Apply pending migrations automatically
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                Log.Information("Applying {Count} pending migrations", pendingMigrations.Count());
                await context.Database.MigrateAsync();
                Log.Information("Database migrations applied successfully");
            }
            else
            {
                Log.Information("No pending migrations found");
            }
        }
        else
        {
            Log.Error("Cannot connect to database. Please check your connection string.");
            throw new InvalidOperationException("Database connection failed");
        }

        Log.Information("Database migration and seeding completed successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while migrating or seeding the database");

        // Don't crash the app in production
        if (!app.Environment.IsProduction())
        {
            throw;
        }
    }
}

// Import Vietnam Addresses if requested (local development only)
if (args.Length > 0 && args[0] == "--import-addresses")
{
    var csvPath = args.Length > 1 ? args[1] : "Data/Book1.csv";
    Console.WriteLine($"Import CSV từ: {csvPath}");

    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var importer = new AddressImporter(context);

    await importer.ImportFromCsvAsync(csvPath);
    Console.WriteLine("Nhấn Enter để thoát...");
    Console.ReadLine();
    return;
}

// Graceful shutdown
app.Lifetime.ApplicationStopping.Register(() =>
{
    Log.Information("Application is shutting down...");
});

try
{
    Log.Information("Starting Sakura Home API on {Environment} environment at {Time}",
        app.Environment.EnvironmentName,
        DateTime.UtcNow);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}