using FirstGIG.Identity.Application.Interfaces;
using FirstGIG.Identity.Domain.Entities;
using FirstGIG.Identity.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FirstGIG.Identity.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;

    public UserRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email.Value == email, cancellationToken);

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default) =>
        await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken), cancellationToken);

    public async Task<User?> GetByVerificationTokenAsync(string token, CancellationToken cancellationToken = default) =>
        await _context.Users
            .FirstOrDefaultAsync(u => u.EmailVerificationToken == token, cancellationToken);

    public async Task<User?> GetByResetTokenAsync(string token, CancellationToken cancellationToken = default) =>
        await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.PasswordResetToken == token, cancellationToken);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await _context.Users.AnyAsync(u => u.Email.Value == email, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        await _context.Users.AddAsync(user, cancellationToken);

    public void Update(User user) =>
        _context.Users.Update(user);
}

// Unit of Work implementation
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly IdentityDbContext _context;

    public UnitOfWork(IdentityDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
