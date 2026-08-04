namespace Dsw2026Tpi.Api.Configurations;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.RateLimiting;
using Dsw2026Tpi.CrossCutting.Models;
using Dsw2026Tpi.CrossCutting.Resources;
using Microsoft.AspNetCore.RateLimiting;
public static class RateLimitingConfigurationExtensions
{
    public const string AdminLoginPolicy = "AdminLoginPolicy";
    public const string PatientLoginPolicy = "PatientLoginPolicy";
    public const string AppointmentBookingPolicy = "AppointmentBookingPolicy";

    private record RateLimitPolicyOptions(int PermitLimit, int WindowSeconds);

    public static IServiceCollection AddAppRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var adminLogin = configuration.GetSection("RateLimiting:AdminLogin").Get<RateLimitPolicyOptions>()
                          ?? new RateLimitPolicyOptions(5, 60);
        var patientLogin = configuration.GetSection("RateLimiting:PatientLogin").Get<RateLimitPolicyOptions>()
                          ?? new RateLimitPolicyOptions(10, 60);
        var appointmentBooking = configuration.GetSection("RateLimiting:AppointmentBooking").Get<RateLimitPolicyOptions>()
                          ?? new RateLimitPolicyOptions(5, 60);
        var general = configuration.GetSection("RateLimiting:General").Get<RateLimitPolicyOptions>()
                          ?? new RateLimitPolicyOptions(100, 60);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetUserOrIp(httpContext),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = general.PermitLimit,
                        Window = TimeSpan.FromSeconds(general.WindowSeconds),
                        QueueLimit = 0
                    }));

            options.AddPolicy(AdminLoginPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetIp(httpContext),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = adminLogin.PermitLimit,
                        Window = TimeSpan.FromSeconds(adminLogin.WindowSeconds),
                        QueueLimit = 0
                    }));

            options.AddPolicy(PatientLoginPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetIp(httpContext),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = patientLogin.PermitLimit,
                        Window = TimeSpan.FromSeconds(patientLogin.WindowSeconds),
                        QueueLimit = 0
                    }));

            options.AddPolicy(AppointmentBookingPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetUserOrIp(httpContext),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = appointmentBooking.PermitLimit,
                        Window = TimeSpan.FromSeconds(appointmentBooking.WindowSeconds),
                        QueueLimit = 0
                    }));

            options.OnRejected = async (context, cancellationToken) =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning(
                    "Rate limit excedido: {Method} {Path} desde {Key}",
                    context.HttpContext.Request.Method,
                    context.HttpContext.Request.Path,
                    GetUserOrIp(context.HttpContext));

                context.HttpContext.Response.ContentType = "application/json";

                var error = new ErrorResponse(nameof(ErrorCodes.RATE_LIMIT_EXCEEDED), ErrorCodes.RATE_LIMIT_EXCEEDED);
                var json = JsonSerializer.Serialize(error); 

                await context.HttpContext.Response.WriteAsync(json, cancellationToken);
            };
        });

        return services;
    }

    private static string GetIp(HttpContext httpContext) =>
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private static string GetUserOrIp(HttpContext httpContext) =>
        httpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? GetIp(httpContext);
}

