using Lending.Domain.Loans;
using Microsoft.EntityFrameworkCore;

namespace Lending.Persistence;

public class LendingDbContext(DbContextOptions<LendingDbContext> options) : DbContext(options)
{
    #region Entities
    public DbSet<Loan> Loans { get; set; }
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.HasDefaultSchema("lending");
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}