using pixAPI.Exceptions;
using pixAPI.Models;
using pixAPI.Repositories;

namespace pixAPI.Middlewares;

public class AuthorizationHandlerMiddleware(RequestDelegate next)
{
  private readonly RequestDelegate _next = next;
  private readonly Dictionary<string, bool> authRoutes = new()
  {
    { "keys", true },
    { "payments", true },
    { "concilliation", true }
  };

  public async Task InvokeAsync(HttpContext context, PaymentProviderRepository paymentProviderRepository)
  {
    var path = context.Request.Path;
    if (!path.HasValue) 
    {
      await _next(context);
    }

    string strPath = path.ToString();
    string firstPathValue = strPath.Split('/')[1];
    if (authRoutes.ContainsKey(firstPathValue))
    {
      if (!context.Request.Headers.ContainsKey("Authorization"))
        throw new UnauthorizedException("Formato inválido para autentificação.");

      var authorization = context.Request.Headers.Authorization;
      var bearer = authorization.FirstOrDefault()?.Split(" ")[0];

      if (bearer is null || !bearer.Equals("Bearer"))
        throw new UnauthorizedException("Formato inválido para autentificação.");

      var token = authorization.FirstOrDefault()?.Split(" ")[1];
      if (token is null)
        throw new UnauthorizedException("Token inválido ou inexistente.");

      PaymentProvider? paymentProvider = await paymentProviderRepository.GetBankByToken(token);
      if (paymentProvider is null)
        throw new UnauthorizedException("Token inválido ou inexistente.");

      context.Items["bankData"] = paymentProvider;
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