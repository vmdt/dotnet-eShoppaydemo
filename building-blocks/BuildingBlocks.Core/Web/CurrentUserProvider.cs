using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Core.Web;

public interface ICurrentUserProvider
{
    public long? GetCurrentUserId();
}

public class CurrentUserProvider : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long? GetCurrentUserId()
    {
        var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(userId, out var id))
        {
            return id;
        }
        return null;
    }
}
