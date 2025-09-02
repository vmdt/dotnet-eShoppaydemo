using BuildingBlocks.Core.EFCore;
using BuildingBlocks.Core.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentService.Domain;

namespace PaymentService.Infrastucture;

public sealed class PaymentDbContext : AppDbContextBase
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options, ICurrentUserProvider? currentUserProvider = null, ILogger<PaymentDbContext>? logger = null)
        : base(options, currentUserProvider, logger)
    {
    }

    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
        builder.FilterSoftDeletedProperties();
        builder.ToSnakeCaseTables();
    }
}