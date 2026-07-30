using System.Security.Cryptography;

namespace FWU.Exam.Management.Web.Middleware;

public class SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Response.HasStarted)
        {
            var headers = context.Response.Headers;

            headers.Append("X-Content-Type-Options", "nosniff");
            headers.Append("Cache-Control", "no-store, no-cache, must-revalidate, proxy-revalidate");
            headers.Append("Pragma", "no-cache");
            headers.Append("Expires", "0");
            headers.Append("X-Frame-Options", "DENY");
            headers.Append("X-XSS-Protection", "1; mode=block");
            headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=(), payment=()");

            if (!env.IsDevelopment())
            {
                var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
                context.Items["ScriptNonce"] = nonce;

                headers.Append("Content-Security-Policy",
                    "default-src 'self'; " +
                    $"script-src 'nonce-{nonce}' 'strict-dynamic' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://code.jquery.com; " +
                    "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdnjs.cloudflare.com https://cdn.jsdelivr.net; " +
                    "font-src 'self' https://fonts.gstatic.com https://cdnjs.cloudflare.com https://cdn.jsdelivr.net; " +
                    "img-src 'self' data: https:; " +
                    "connect-src 'self'; " +
                    "frame-ancestors 'none'");
            }
        }

        await next(context);
    }
}
