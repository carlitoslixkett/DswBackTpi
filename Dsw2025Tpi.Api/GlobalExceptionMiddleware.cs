using Dsw2025Tpi.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Api.Middleware
{
    public class GlobalExceptionMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception e)
            {
                context.Response.ContentType = "application/json";

                var statusCode = e switch
                {
                    EntityNotFoundException => HttpStatusCode.NotFound,
                    DuplicatedEntityException => HttpStatusCode.BadRequest,
                    BadRequestException => HttpStatusCode.BadRequest,
                    ValidationException => HttpStatusCode.BadRequest,
                    NoContentException => HttpStatusCode.NoContent,
                    UnauthorizedException => HttpStatusCode.Unauthorized,
                    ForbiddenException => HttpStatusCode.Forbidden,
                    _ => HttpStatusCode.InternalServerError
                };

                context.Response.StatusCode = (int)statusCode;

                if (statusCode == HttpStatusCode.NoContent)
                {
                    return;
                }

                var errorResponse = new
                {
                    status = (int)statusCode,
                    title = e.GetType().Name,
                    detail = e.Message
                };

                var json = JsonSerializer.Serialize(errorResponse);
                await context.Response.WriteAsync(json);
            }
        }
    }
}
