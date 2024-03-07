using pixAPI.Exceptions;
using pixAPI.Models;
using pixAPI.Repositories;

namespace pixAPI.Middlewares;

public class AuthorizationHandlerMiddleware(RequestDelegate next)
{
  private readonly RequestDelegate _next = next;
  public async Task InvokeAsync(HttpContext context, PaymentProviderRepository paymentProviderRepository)
  {
    var path = context.Request.Path;
    if (path.HasValue && path.Value.StartsWith("/keys"))
    {
      if (!context.Request.Headers.ContainsKey("Authorization"))
      {
        throw new UnauthorizedException("Token inválido ou inexistente.");
      }

      var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();
      if (token is null)
      {
        throw new UnauthorizedException("Token inválido ou inexistente.");
      }

      PaymentProvider? paymentProvider = await paymentProviderRepository.GetBankByToken(token);
      if (paymentProvider is null)
      {
        throw new UnauthorizedException("Token inválido ou inexistente.");
      }
      context.Items["token"] = token;
    }
    await _next(context);
  }
}

public static class AuthenticationMiddlewareExtensions
{
  public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder)
  {
    return builder.UseMiddleware<AuthorizationHandlerMiddleware>();
  }
}