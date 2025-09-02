using BuildingBlocks.Core.Web;
using PaymentService.API;
using PaymentService.Application;

var builder = WebApplication.CreateBuilder(args);

builder.AddMinimalEndpoints(assemblies: typeof(PaymentServiceApplicationRoot).Assembly);
builder.AddInfrastructure();

var app = builder.Build();

app.MapMinimalEndpoints();
app.UseInfrastructure();

app.Run();