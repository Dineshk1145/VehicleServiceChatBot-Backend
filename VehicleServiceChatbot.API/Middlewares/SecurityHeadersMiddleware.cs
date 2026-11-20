using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace VehicleServiceChatbot.API.Middlewares;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Prevent clickjacking by disallowing framing
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // Prevent MIME type sniffing
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // Enable cross-site scripting filter
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        // Restrict referrer information sent with requests
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Modern browsers: prevent embedding in iframes
        context.Response.Headers.Append("Content-Security-Policy", "frame-ancestors 'none';");

        // Disable unnecessary device capabilities
        context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");

        await _next(context);
    }
}
