using System.Text;
using FirstGIG.Identity.Application.Interfaces;
using FirstGIG.Identity.Domain.Repositories;
using FirstGIG.Identity.Infrastructure.Authentication;
using FirstGIG.Identity.Infrastructure.Email;
using FirstGIG.Identity.Infrastructure.Persistence;
using FirstGIG.Identity.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace FirstGIG.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Settings
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));

        // Database
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "identity")));

        // Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // JWT Authentication
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }
}
