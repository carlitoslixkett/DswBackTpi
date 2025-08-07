// Importa tus excepciones personalizadas definidas en la capa Application
using Dsw2025Tpi.Application.Exceptions;

// Importa clases necesarias para manejar solicitudes HTTP en ASP.NET Core
using Microsoft.AspNetCore.Http;

// Importa la excepción ValidationException para validaciones de modelo
using System.ComponentModel.DataAnnotations;

// Permite usar los códigos de estado HTTP como NotFound, BadRequest, etc.
using System.Net;

// Permite convertir objetos a JSON
using System.Text.Json;

// Permite el uso de async/await
using System.Threading.Tasks;

namespace Dsw2025Tpi.Api.Middleware
{
    // Define un middleware personalizado que implementa IMiddleware
    public class GlobalExceptionMiddleware : IMiddleware
    {
        // Método que se ejecuta cada vez que una solicitud entra a la aplicación
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                // Intenta ejecutar el siguiente componente del pipeline (puede ser otro middleware o el controlador)
                await next(context);
            }
            catch (Exception e)
            {
                // Si ocurre una excepción, se configura la respuesta para que sea de tipo JSON
                context.Response.ContentType = "application/json";

                // Se determina el código de estado HTTP según el tipo de excepción capturada
                var statusCode = e switch
                {
                    EntityNotFoundException => HttpStatusCode.NotFound,       // 404
                    DuplicatedEntityException => HttpStatusCode.BadRequest,   // 400
                    BadRequestException => HttpStatusCode.BadRequest,         // 400
                    ValidationException => HttpStatusCode.BadRequest,         // 400
                    NoContentException => HttpStatusCode.NoContent,           // 204
                    UnauthorizedException => HttpStatusCode.Unauthorized,     // 401
                    ForbiddenException => HttpStatusCode.Forbidden,           // 403
                    _ => HttpStatusCode.InternalServerError                  // 500 para cualquier otro error
                };

                // Se asigna el código de estado HTTP a la respuesta
                context.Response.StatusCode = (int)statusCode;

                // Si el código es 204 (No Content), no se devuelve contenido alguno
                if (statusCode == HttpStatusCode.NoContent)
                {
                    return;
                }

                // Se construye el objeto de respuesta con detalles del error
                var errorResponse = new
                {
                    status = (int)statusCode,          // Código numérico del estado (ej. 400)
                    title = e.GetType().Name,          // Nombre del tipo de excepción (ej. BadRequestException)
                    detail = e.Message                 // Mensaje específico del error
                };

                // Se convierte el objeto a JSON
                var json = JsonSerializer.Serialize(errorResponse);

                // Se escribe el JSON en la respuesta HTTP
                await context.Response.WriteAsync(json);
            }
        }
    }
}
