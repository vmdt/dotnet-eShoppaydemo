using System.Linq.Expressions;
using BuildingBlocks.Core.Core.Model;
using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Core.EFCore;

public static class Extensions
{
    public static IServiceCollection AddCustomDbContext<TContext>(this WebApplicationBuilder builder, string? dbOptionName = "Database")
        where TContext : DbContext, IDbContext
    {
        builder.Services.AddOptions<DbOptions>().BindConfiguration(dbOptionName).ValidateDataAnnotations().ValidateOnStart();
        builder.Services.AddSingleton(sp =>
            sp.GetRequiredService<IOptions<DbOptions>>().Value);

        builder.Services.AddDbContext<TContext>(
            (sp, options) =>
            {
                var dbOptions = sp.GetRequiredService<DbOptions>();
                var connectionString = dbOptions.ConnectionString;

                switch (dbOptions.Provider.ToLowerInvariant())
                {
                    case "postgres":
                        options.UseNpgsql(connectionString);
                        break;
                    case "sqlserver":
                        options.UseSqlServer(connectionString);
                        break;
                }

                options.UseSnakeCaseNamingConvention();
                options.ConfigureWarnings(
                    w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            }
        );
        builder.Services.AddScoped<IDbContext>(provider => provider.GetService<TContext>()!);

        return builder.Services;
    }

    public static void FilterSoftDeletedProperties(this ModelBuilder modelBuilder)
    {
        Expression<Func<IAggregate, bool>> filterExpr = e => !e.IsDeleted;

        foreach (var mutableEntityType in modelBuilder.Model.GetEntityTypes()
                     .Where(m => m.ClrType.IsAssignableTo(typeof(IEntity))))
        {
            // modify expression to handle correct child type
            var parameter = Expression.Parameter(mutableEntityType.ClrType);

            var body = ReplacingExpressionVisitor
                .Replace(filterExpr.Parameters.First(), parameter, filterExpr.Body);

            var lambdaExpression = Expression.Lambda(body, parameter);

            // set filter
            mutableEntityType.SetQueryFilter(lambdaExpression);
        }
    }


    // ref: https://andrewlock.net/customising-asp-net-core-identity-ef-core-naming-conventions-for-postgresql/
    public static void ToSnakeCaseTables(this ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Replace table names
            entity.SetTableName(entity.GetTableName()?.Underscore());

            var tableObjectIdentifier =
                StoreObjectIdentifier.Table(
                    entity.GetTableName()?.Underscore()!,
                    entity.GetSchema());

            // Replace column names
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(property.GetColumnName(tableObjectIdentifier)?.Underscore());
            }

            foreach (var key in entity.GetKeys())
            {
                key.SetName(key.GetName()?.Underscore());
            }

            foreach (var key in entity.GetForeignKeys())
            {
                key.SetConstraintName(key.GetConstraintName()?.Underscore());
            }
        }
    }


    private static async Task MigrateAsync<TContext>(IServiceProvider serviceProvider)
        where TContext : DbContext, IDbContext
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TContext>>();

        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        

        if (pendingMigrations.Any())
        {
        logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());

            await context.Database.MigrateAsync();
            logger.LogInformation("Migrations applied successfully.");
        }
    }

    public static IApplicationBuilder UseMigration<TContext>(this IApplicationBuilder app)
    where TContext : DbContext, IDbContext
    {
        MigrateAsync<TContext>(app.ApplicationServices).GetAwaiter().GetResult();

        // SeedAsync(app.ApplicationServices).GetAwaiter().GetResult();

        return app;
    }

    private static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var seedersManager = scope.ServiceProvider.GetRequiredService<ISeedManager>();

        await seedersManager.ExecuteSeedAsync();
    }
}
