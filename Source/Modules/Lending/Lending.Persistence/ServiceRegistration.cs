using Blocks.Domain.Abstractions;
using Lending.Application.Features.Loans.Queries;
using Lending.Domain.Loans;
using Lending.Persistence.Queries;
using Lending.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lending.Persistence;

public static class ServiceRegistration
{
    public static void AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<LendingDbContext>(options =>
            options.UseNpgsql(connectionString, b =>
                b.MigrationsAssembly(typeof(LendingDbContext).Assembly.FullName)
            )
        );

        services.AddScoped<IUnitOfWork, LendingUnitOfWork>();
        services.AddScoped<ILoanRepository, LoanRepository>();
        services.AddScoped<ILoanQueries, LoanQueries>();
    }
}