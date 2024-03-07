using System.Net;

namespace pixAPI.Middlewares;

public class ExceptionHandlerMiddleware(RequestDelegate next)
{
  private readonly RequestDelegate _next = next;

  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (Exception e)
    {
      HandleException(context, e);
    }
  }

  private static void HandleException(HttpContext context, Exception e) {
    ExceptionResponse response = e switch 
    {
      _ => new ExceptionResponse(HttpStatusCode.InternalServerError, "Erro Interno do Servidor")
    };

    context.Response.ContentType = "application/json";
    context.Response.StatusCode = (int)response.StatusCode;
    context.Response.WriteAsJsonAsync(response);
  }
}

public record ExceptionResponse(HttpStatusCode StatusCode, string Description);