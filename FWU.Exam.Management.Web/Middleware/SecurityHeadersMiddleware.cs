namespace FWU.Exam.Management.Web.Middleware;

public class SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Response.HasStarted)
        {
            var headers = context.Response.Headers;

            // Prevent MIME type sniffing
            headers.Append("X-Content-Type-Options", "nosniff");

            // Prevent clickjacking
            headers.Append("X-Frame-Options", "DENY");

            // Legacy XSS protection header (browsers ignore if CSP is present)
            headers.Append("X-XSS-Protection", "1; mode=block");

            // Referrer-Policy to prevent information leakage
            headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

            // Restrict device access
            headers.Append("Permissions-Policy", "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");

            if (!env.IsDevelopment())
            {
                // Strict Content Security Policy for production
                headers.Append("Content-Security-Policy",
                    "default-src 'self'; " +
                    "script-src 'self' https://cdn.tailwindcss.com https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://code.jquery.com https://code.jquery.com/ui; " +
                    "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdnjs.cloudflare.com https://cdn.jsdelivr.net https://code.jquery.com/ui; " +
                    "font-src 'self' https://fonts.gstatic.com https://cdnjs.cloudflare.com https://cdn.jsdelivr.net; " +
                    "img-src 'self' data: https:; " +
                    "media-src 'self'; " +
                    "connect-src 'self'; " +
                    "frame-ancestors 'none'; " +
                    "base-uri 'self'; " +
                    "form-action 'self'");

                // Enforce HTTPS
                headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            }
            else
            {
                // Relaxed CSP for development to avoid strict style restrictions with tools
                headers.Append("Content-Security-Policy",
                    "default-src 'self'; " +
                    "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.tailwindcss.com https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://code.jquery.com; " +
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
