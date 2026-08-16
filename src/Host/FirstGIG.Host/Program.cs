using FirstGIG.Host.Middleware;
using FirstGIG.Identity.Application;
using FirstGIG.Identity.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/firstgig-.log", rollingInterval: RollingInterval.Day));

// Controllers
builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy
            .WithOrigins(
                builder.Configuration["Frontend:BaseUrl"] ?? "http://localhost:3000",
                "http://localhost:5173") // Vite default
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FirstGIG API",
        Version = "v1",
        Description = "The backend API for the FirstGIG Egyptian freelancing platform.",
        Contact = new OpenApiContact { Name = "FirstGIG Team" }
    });

    // Add JWT Bearer support to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. Example: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── Modules ────────────────────────────────────────────────────────────────
builder.Services.AddIdentityApplication();
builder.Services.AddIdentityInfrastructure(builder.Configuration);
// Future modules go here: AddJobsApplication(), AddProfilesApplication(), etc.
// ──────────────────────────────────────────────────────────────────────────

var app = builder.Build();

// Global Exception Handler
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FirstGIG API v1");
        c.RoutePrefix = string.Empty; // Swagger at root
    });
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseCors("ReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply migrations on startup in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var identityDb = scope.ServiceProvider
            .GetRequiredService<FirstGIG.Identity.Infrastructure.Persistence.IdentityDbContext>();
        await identityDb.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying migrations.");
    }
}

app.Run();
