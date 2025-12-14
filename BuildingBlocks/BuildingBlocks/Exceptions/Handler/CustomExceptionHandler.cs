using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Exceptions.Handler
{
    public class CustomExceptionHandler(ILogger<CustomExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext Context, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError("Error message: {exceptionMessage}, Time of occurence {time}", exception.Message, DateTime.UtcNow);


            (string Details, string Title, int StatusCode) details = exception switch
            {
                InternalServerException => (exception.Message, exception.GetType().Name, StatusCodes.Status500InternalServerError),
                BadRequestException => (exception.Message, exception.GetType().Name, StatusCodes.Status400BadRequest),
                ValidationException => (exception.Message, exception.GetType().Name, StatusCodes.Status400BadRequest),
                NotFoundException => (exception.Message, exception.GetType().Name, StatusCodes.Status404NotFound),
                _ => (exception.Message, "Internal Server Error", StatusCodes.Status500InternalServerError)
            };

            var problemDetails = new ProblemDetails
            {
                Title = details.Title,
                Status = details.StatusCode,
                Detail = details.Details,
                Instance = Context.Request.Path
            };

            problemDetails.Extensions.Add("traceId", Context.TraceIdentifier);

            if (exception is ValidationException validationException)
            {
                problemDetails.Extensions.Add("errors", validationException.Errors);
            }

            await Context.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken);

            return true;
        }
    }
}
