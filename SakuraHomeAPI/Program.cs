using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SakuraHomeAPI.Data;
//using SakuraHomeAPI.Extensions;
//using SakuraHomeAPI.Middlewares;
using SakuraHomeAPI.Models.Entities.Identity;
//using SakuraHomeAPI.Services.Implementations;
//using SakuraHomeAPI.Services.Interfaces;
using Serilog;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/sakura-home-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.

// Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("SakuraHomeAPI")));

// Identity Configuration
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
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

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
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
        ClockSkew = TimeSpan.Zero
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
    });

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Memory Cache
builder.Services.AddMemoryCache();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://yourdomain.com", "https://admin.yourdomain.com")
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
//builder.Services.AddScoped<ITokenService, TokenService>();
//builder.Services.AddScoped<IUserService, UserService>();
//builder.Services.AddScoped<IProductService, ProductService>();
//builder.Services.AddScoped<ICategoryService, CategoryService>();
//builder.Services.AddScoped<IBrandService, BrandService>();
//builder.Services.AddScoped<IOrderService, OrderService>();
//builder.Services.AddScoped<ICartService, CartService>();
//builder.Services.AddScoped<IWishlistService, WishlistService>();
//builder.Services.AddScoped<IReviewService, ReviewService>();
//builder.Services.AddScoped<INotificationService, NotificationService>();
//builder.Services.AddScoped<IEmailService, EmailService>();
//builder.Services.AddScoped<IFileService, FileService>();
//builder.Services.AddScoped<ITranslationService, TranslationService>();
//builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

//// Repository Pattern
//builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
//builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

//// Background Services
//builder.Services.AddHostedService<EmailProcessorService>();
//builder.Services.AddHostedService<NotificationProcessorService>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

// Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.

// Exception Handling
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sakura Home API V1");
        c.RoutePrefix = string.Empty; // Swagger at root
    });
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Custom Middlewares
//app.UseMiddleware<ExceptionMiddleware>();
//app.UseMiddleware<RequestLoggingMiddleware>();
//app.UseMiddleware<RateLimitingMiddleware>();

//// Security Headers
//app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseHttpsRedirection();
app.UseResponseCompression();

// Static Files
app.UseStaticFiles();

// CORS
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
}
else
{
    app.UseCors("Production");
}

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Health Checks
app.UseHealthChecks("/health");

// Controllers
app.MapControllers();

// Database Migration and Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

        // Apply pending migrations
        context.Database.Migrate();

        // Seed data
       // await SeedData.Initialize(services, userManager, roleManager);

        Log.Information("Database migration and seeding completed successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while migrating or seeding the database");
        throw;
    }
}

// Graceful shutdown
app.Lifetime.ApplicationStopping.Register(() =>
{
    Log.Information("Application is shutting down...");
});

try
{
    Log.Information("Starting Sakura Home API");
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