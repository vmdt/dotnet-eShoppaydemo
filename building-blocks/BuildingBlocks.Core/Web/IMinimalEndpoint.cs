using Microsoft.AspNetCore.Routing;

namespace BuildingBlocks.Core.Web;

public interface IMinimalEndpoint
{
    IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder);
}