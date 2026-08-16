using FirstGIG.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FirstGIG.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-update UpdatedAt on modified entities
        var entries = ChangeTracker.Entries<FirstGIG.BuildingBlocks.Domain.Primitives.Entity>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
            entry.Entity.SetUpdatedAt();

        return base.SaveChangesAsync(cancellationToken);
    }
}
