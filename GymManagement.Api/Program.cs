using GymManagement.Api.Infrastructure.Persistence;
using GymManagement.Api.Infrastructure.Tenant;
using GymManagement.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;
using System.Text;
using AspNetCoreRateLimit;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddInMemoryRateLimiting();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Limit = 100,
            Period = "1m"
        },
        new RateLimitRule
        {
            Endpoint = "/api/Account/Login",
            Limit = 5,
            Period = "1m"
        },
        new RateLimitRule
        {
            Endpoint = "POST:/api/payments",
            Limit = 20,
            Period = "5m"
        }
    };
});

builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

builder.Services.AddControllers();

// ✅ OpenAPI nativo .NET 10 con soporte para Authorize
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        var schemeName = "Bearer";

        // 1. Definir el esquema
        var securityScheme = new Microsoft.OpenApi.OpenApiSecurityScheme
        {
            Type = Microsoft.OpenApi.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Introduce tu token JWT"
        };

        // 2. Registrar en Components
        document.Components ??= new Microsoft.OpenApi.OpenApiComponents();
        if (document.Components.SecuritySchemes == null)
        {
            document.Components.SecuritySchemes = new Dictionary<string, Microsoft.OpenApi.IOpenApiSecurityScheme>();
        }
        document.Components.SecuritySchemes.Add(schemeName, securityScheme);

        // 3. Aplicar GLOBALMENTE usando una Referencia
        document.Security ??= new List<Microsoft.OpenApi.OpenApiSecurityRequirement>();
        var requirement = new Microsoft.OpenApi.OpenApiSecurityRequirement
        {
            [new Microsoft.OpenApi.OpenApiSecuritySchemeReference(schemeName, document)] = new List<string>()
        };
        document.Security.Add(requirement);

        return Task.CompletedTask;
    });
});



// JWT
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection.GetValue<string>("Key")
    ?? throw new InvalidOperationException("Missing required configuration 'Jwt:Key'. Set it via environment variable, user secrets, or appsettings.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
})
.AddNegotiate();

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, HttpContextTenantProvider>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtService>(sp => new JwtService(jwtKey));

builder.Services.AddAutoMapper(typeof(GymManagement.Api.Infrastructure.Mapping.MappingProfile));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddDbContext<GymManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlOptions => sqlOptions.EnableRetryOnFailure()));


builder.Host.UseSerilog((context, configuration) =>
{
    configuration
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/app-.log",
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7);

});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // 
    app.MapScalarApiReference(options =>
    {
        options.WithHttpBearerAuthentication(bearer =>
        {
            bearer.Token = ""; // Scalar habilitará el campo de texto globalmente
        });
    });

    // Seeding
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<GymManagementDbContext>();
    await db.Database.MigrateAsync();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await DbInitializer.SeedAsync(db, hasher);
}

app.UseHttpsRedirection();
app.UseIpRateLimiting();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();