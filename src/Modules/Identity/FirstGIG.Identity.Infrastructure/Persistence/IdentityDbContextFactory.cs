using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FirstGIG.Identity.Infrastructure.Persistence;

/// <summary>
/// Used by EF Core CLI tools (migrations) at design time.
/// </summary>
public sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        // Find solution root by walking up from this assembly's location
        // Assembly is in: src/Modules/Identity/FirstGIG.Identity.Infrastructure/bin/...
        var assemblyDir = Path.GetDirectoryName(typeof(IdentityDbContextFactory).Assembly.Location)!;

        // Try to find appsettings.json by walking up directories
        var dir = new DirectoryInfo(assemblyDir);
        string? hostAppsettingsPath = null;

        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "Host", "FirstGIG.Host", "appsettings.json");
            if (File.Exists(candidate))
            {
                hostAppsettingsPath = Path.GetDirectoryName(candidate)!;
                break;
            }
            dir = dir.Parent;
        }

        // Fallback: use current directory
        hostAppsettingsPath ??= Path.Combine(Directory.GetCurrentDirectory(), "src", "Host", "FirstGIG.Host");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(hostAppsettingsPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
        optionsBuilder.UseSqlServer(
            configuration.GetConnectionString("DefaultConnection"),
            sqlOptions => sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "identity"));

        return new IdentityDbContext(optionsBuilder.Options);
    }
}
