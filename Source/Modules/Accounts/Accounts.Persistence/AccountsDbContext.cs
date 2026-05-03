using Accounts.Domain.Lenders;
using Microsoft.EntityFrameworkCore;

namespace Accounts.Persistence;

public class AccountsDbContext(DbContextOptions<AccountsDbContext> options) : DbContext(options)
{
    #region Entities
    public DbSet<Lender> Lenders { get; set; }
    #endregion
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.HasDefaultSchema("accounts");
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}