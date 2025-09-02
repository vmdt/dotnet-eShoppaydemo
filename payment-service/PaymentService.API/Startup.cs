using BuildingBlocks.Core.EFCore;
using BuildingBlocks.Core.Web;
using Microsoft.Extensions.Options;
using PaymentService.Domain;
using PaymentService.Infrastucture;

namespace PaymentService.API;

public static class Startup
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var env = builder.Environment;
        builder.Services.AddHttpContextAccessor();

        builder.Services.Configure<AppOptions>(configuration.GetSection(nameof(AppOptions)));
        builder.Services.Configure<PostgresOptions>(configuration.GetSection(nameof(PostgresOptions)));
        builder.Services.AddSingleton(sp =>
            sp.GetRequiredService<IOptions<PostgresOptions>>().Value);

        builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();
        builder.AddCustomDbContext<PaymentDbContext>();


        return builder;
    }

    public static WebApplication UseInfrastructure(this WebApplication app)
    {
        var env = app.Environment;

        // app.UseAuthentication();
        // app.UseAuthorization();

        app.UseMigration<PaymentDbContext>();

        app.MapGet("/", x => x.Response.WriteAsync("Hello from PaymentService.API!"));

        if (env.IsDevelopment())
        {
            // app.UseAspnetOpenApi();
        }

        return app;
    }
}
