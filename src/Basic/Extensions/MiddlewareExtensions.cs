using Basic.Middlewares;

namespace Basic.Extensions;

public static class MiddlewareExtensions
{
   public static void UseRateLimiting(this IApplicationBuilder builder)
   {
      builder.UseMiddleware<RateLimitingMiddleware>();
   }
}
