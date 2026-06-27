using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Npgsql;
using UrlShortener.Core.Contracts;
using UrlShortener.Core.Services;
using UrlShortener.Infrastructure;
using UrlShortener.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

var frontendUrl = builder.Configuration["AppSettings:FrontendUrl"];
var apiUrl = builder.Configuration["AppSettings:ApiUrl"];
var baseUrl = builder.Configuration["AppSettings:BaseUrl"];
if (string.IsNullOrWhiteSpace(frontendUrl))
    throw new InvalidOperationException("Missing configuration: AppSettings:FrontendUrl");
if (string.IsNullOrWhiteSpace(apiUrl))
    throw new InvalidOperationException("Missing configuration: AppSettings:ApiUrl");
if (string.IsNullOrWhiteSpace(baseUrl))
    throw new InvalidOperationException("Missing configuration: AppSettings:BaseUrl");

// ── JWT Settings ───────────────────────────────────────────────────────────
var jwtKey = builder.Configuration["JwtSettings:Key"]
    ?? throw new InvalidOperationException("Missing configuration: JwtSettings:Key");
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "LinkSwift";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "LinkSwiftClient";
var jwtExpiryHours = int.TryParse(builder.Configuration["JwtSettings:ExpiryHours"], out var h) ? h : 720;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            ClockSkew = TimeSpan.Zero,
        };
    });

// ── CORS ───────────────────────────────────────────────────────────────────
var allowedOrigins = frontendUrl.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Concat(apiUrl.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalDev", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ── Rate Limiting ──────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync("{\"error\": \"Too many requests. Please try again later.\"}", token);
    };

    options.AddPolicy("WriteTrafficPolicy", context =>
    {
        var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown-ip";
        if (ipAddress.Contains(','))
        {
            ipAddress = ipAddress.Split(',')[0].Trim();
        }
        return RateLimitPartition.GetTokenBucketLimiter(ipAddress, _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 10,
            QueueLimit = 0,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            TokensPerPeriod = 5,
            AutoReplenishment = true
        });
    });

    options.AddPolicy("ReadTrafficPolicy", context =>
    {
        var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown-ip";
        if (ipAddress.Contains(','))
        {
            ipAddress = ipAddress.Split(',')[0].Trim();
        }
        return RateLimitPartition.GetSlidingWindowLimiter(ipAddress, _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 100,
            QueueLimit = 0,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 6,
            AutoReplenishment = true
        });
    });
});

builder.Services.AddControllers();

// ── Database ───────────────────────────────────────────────────────────────
var neonConnectionString = builder.Configuration.GetConnectionString("Neon");
var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");

var rawConnectionString = !string.IsNullOrWhiteSpace(neonConnectionString)
    ? neonConnectionString
    : defaultConnectionString;

if (string.IsNullOrWhiteSpace(rawConnectionString))
    throw new InvalidOperationException("Missing connection string: DefaultConnection or Neon");
if (string.IsNullOrWhiteSpace(redisConnectionString))
    throw new InvalidOperationException("Missing connection string: Redis");

var connectionString = ConvertPostgresUriToConnectionString(rawConnectionString);
var formattedRedisConnectionString = ConvertRedisUriToStackExchangeFormat(redisConnectionString);

builder.Services.AddDbContext<UrlShortenerDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddStackExchangeRedisCache(options => options.Configuration = formattedRedisConnectionString);

// ── Application Services ───────────────────────────────────────────────────
builder.Services.AddScoped<IUrlRepository, EntityFrameworkUrlRepository>();
builder.Services.AddScoped<IClickEventRepository, EntityFrameworkClickEventRepository>();
builder.Services.AddScoped<IUserProfileRepository, EntityFrameworkUserProfileRepository>();
builder.Services.AddSingleton<IKeyGenerator, Base62KeyGenerator>();
builder.Services.AddScoped<IUrlShortenerService>(sp =>
{
    var repository = sp.GetRequiredService<IUrlRepository>();
    var generator = sp.GetRequiredService<IKeyGenerator>();
    var clickEvents = sp.GetRequiredService<IClickEventRepository>();
    var cache = sp.GetService<Microsoft.Extensions.Caching.Distributed.IDistributedCache>();
    return new UrlShortenerService(repository, generator, clickEvents, baseUrl, cache);
});
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAnalyticsService>(sp =>
{
    var repository = sp.GetRequiredService<IUrlRepository>();
    var clickEvents = sp.GetRequiredService<IClickEventRepository>();
    return new AnalyticsService(repository, clickEvents, baseUrl);
});
builder.Services.AddScoped<IUserService, UserService>();

// ── Auth Service ───────────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService>(sp =>
    new AuthService(
        sp.GetRequiredService<UrlShortenerDbContext>(),
        jwtKey,
        jwtIssuer,
        jwtAudience,
        jwtExpiryHours
    )
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "LinkSwift API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ── DB Schema ──────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UrlShortenerDbContext>();
    dbContext.Database.EnsureCreated();
    await ApplySchemaUpdatesAsync(dbContext);
}

// ── Middleware Pipeline ────────────────────────────────────────────────────
app.UseCors("AllowLocalDev");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "LinkSwift API v1");
    c.RoutePrefix = "swagger";
});

app.UseMiddleware<UrlShortener.Api.Middleware.ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static async Task ApplySchemaUpdatesAsync(UrlShortenerDbContext dbContext)
{
    const string sql = """
        ALTER TABLE IF EXISTS "UrlMappings" ADD COLUMN IF NOT EXISTS "IsPrivate" boolean NOT NULL DEFAULT false;
        ALTER TABLE IF EXISTS "UrlMappings" ADD COLUMN IF NOT EXISTS "QrScanCount" integer NOT NULL DEFAULT 0;
        CREATE TABLE IF NOT EXISTS "ClickEvents" (
            "Id" uuid NOT NULL PRIMARY KEY,
            "ShortCode" character varying(100) NOT NULL,
            "ClickedAt" timestamp with time zone NOT NULL,
            "Referrer" text NULL,
            "Country" text NULL
        );
        CREATE INDEX IF NOT EXISTS "IX_ClickEvents_ShortCode" ON "ClickEvents" ("ShortCode");
        CREATE INDEX IF NOT EXISTS "IX_ClickEvents_ClickedAt" ON "ClickEvents" ("ClickedAt");
        CREATE TABLE IF NOT EXISTS "UserProfiles" (
            "UserId" character varying(100) NOT NULL PRIMARY KEY,
            "DisplayName" character varying(200) NOT NULL,
            "Email" character varying(200) NOT NULL,
            "DefaultDomain" character varying(100) NOT NULL,
            "ApiKey" character varying(100) NOT NULL,
            "Theme" character varying(20) NOT NULL,
            "WeeklyAnalyticsReport" boolean NOT NULL DEFAULT true,
            "LinkThresholdAlerts" boolean NOT NULL DEFAULT true,
            "NewDeviceLogin" boolean NOT NULL DEFAULT false,
            "CompactView" boolean NOT NULL DEFAULT false,
            "CreatedAt" timestamp with time zone NOT NULL
        );
        CREATE TABLE IF NOT EXISTS "AppUsers" (
            "Id" uuid NOT NULL PRIMARY KEY,
            "Name" character varying(200) NOT NULL,
            "Email" character varying(200) NOT NULL,
            "PasswordHash" text NOT NULL,
            "CreatedAt" timestamp with time zone NOT NULL,
            "LastLoginAt" timestamp with time zone NULL,
            "Plan" character varying(50) NOT NULL DEFAULT 'free'
        );
        CREATE UNIQUE INDEX IF NOT EXISTS "IX_AppUsers_Email" ON "AppUsers" ("Email");
        """;

    await dbContext.Database.ExecuteSqlRawAsync(sql);
}

static string ConvertPostgresUriToConnectionString(string connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return connectionString;
    }

    if (connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) ||
        connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            var uri = new Uri(connectionString);
            var userInfo = uri.UserInfo.Split(':');
            var username = userInfo[0];
            var password = userInfo.Length > 1 ? userInfo[1] : "";
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.AbsolutePath.TrimStart('/');

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = host,
                Port = port,
                Database = database,
                Username = username,
                Password = password,
                SslMode = SslMode.Require
            };

            return builder.ConnectionString;
        }
        catch
        {
            return connectionString;
        }
    }

    return connectionString;
}

static string ConvertRedisUriToStackExchangeFormat(string connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return connectionString;
    }

    if (connectionString.StartsWith("rediss://", StringComparison.OrdinalIgnoreCase) ||
        connectionString.StartsWith("redis://", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            var uri = new Uri(connectionString);
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 6379;
            var ssl = uri.Scheme.Equals("rediss", StringComparison.OrdinalIgnoreCase);
            
            var password = uri.UserInfo;
            if (!string.IsNullOrEmpty(password) && password.Contains(':'))
            {
                password = password.Split(':')[1];
            }

            var parts = new List<string> { $"{host}:{port}" };
            if (!string.IsNullOrEmpty(password))
            {
                parts.Add($"password={password}");
            }
            if (ssl)
            {
                parts.Add("ssl=true");
            }
            parts.Add("abortConnect=false");

            return string.Join(",", parts);
        }
        catch
        {
            return connectionString;
        }
    }

    return connectionString;
}
