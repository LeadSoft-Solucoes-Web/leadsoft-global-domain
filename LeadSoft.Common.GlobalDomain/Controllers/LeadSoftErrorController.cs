using LeadSoft.Common.GlobalDomain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace LeadSoft.Common.GlobalDomain.Controllers
{
    /// <summary>
    /// Provides a base controller for handling unhandled exceptions and returning standardized error responses in
    /// LeadSoft applications.
    /// </summary>
    /// <remarks>This abstract controller is intended to be used as a global error handler in ASP.NET Core
    /// applications. It exposes an unauthenticated endpoint for error handling, returning error details in a consistent
    /// format and appending an 'ErrorCount' header to the HTTP response. Sensitive exception details are not included
    /// in the response body. Inherit from this controller to implement custom error handling logic as needed.</remarks>
    /// <param name="logger">The logger used to record error and diagnostic information for the controller. Cannot be null.</param>
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract class LeadSoftErrorController(ILogger<LeadSoftErrorController> logger) : ControllerBase
    {
        /// <summary>
        /// Handles unhandled exceptions by returning a standardized error response to the client.
        /// </summary>
        /// <remarks>This action is accessible without authentication and is typically used as a global
        /// error handler for the application. The response includes error information in a consistent format and
        /// appends an 'ErrorCount' header to the HTTP response. Sensitive exception details are not exposed to the
        /// client beyond the exception message.</remarks>
        /// <returns>An <see cref="IActionResult"/> containing the error details and an HTTP 500 Internal Server Error status
        /// code.</returns>
        [Route("Error")]
        [AllowAnonymous]
        public IActionResult Error()
        {
            IExceptionHandlerFeature context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            Exception exception = context?.Error;

            logger.LogError(exception, "LeadSoftErrorController: Ocorreu uma exceção não tratada.");

            DTOErrorResponse dtoResponse = new(HttpStatusCode.InternalServerError, "Internal Server Error.", $"Original message: '{exception?.Message}'");

            dtoResponse.HandleException(exception);

            var statusCode = (int)dtoResponse.Status;
            Response.Headers.Append("ErrorCount", dtoResponse.ErrorCount.ToString());

            return StatusCode(statusCode, dtoResponse);
        }
    }
}
