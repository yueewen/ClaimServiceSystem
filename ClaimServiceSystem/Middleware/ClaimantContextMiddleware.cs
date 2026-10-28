using System.Security.Claims;

namespace ClaimServiceSystem.Middleware;

// Post-auth helper middleware:
// - runs after token is already authenticated
// - checks claimant identity claim exists for claimant routes
// - stores claimant id in request context for controllers/services
public sealed class ClaimantContextMiddleware
{
    public const string ClaimantIdItemKey = "ClaimantId";

    private readonly RequestDelegate _next;

    public ClaimantContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api/v1/claims", StringComparison.OrdinalIgnoreCase)
            && context.User.Identity?.IsAuthenticated == true
            && context.User.IsInRole("Claimant"))
        {
            var claimantId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.User.FindFirstValue(ClaimTypes.Name)
                ?? context.User.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(claimantId))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Missing claimant identity claim.");
                return;
            }

            context.Items[ClaimantIdItemKey] = claimantId;
        }

        await _next(context);
    }
}
