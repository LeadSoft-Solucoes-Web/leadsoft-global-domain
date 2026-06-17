using LeadSoft.Common.GlobalDomain.DTOs;
using LeadSoft.Common.Library.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace LeadSoft.Common.GlobalDomain.Controllers;

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
[Route("api/[controller]")]
public abstract class LeadSoftErrorController(ILogger<LeadSoftErrorController> logger) : ControllerBase
{
    /// <summary>
    /// Handles errors routed from the exception handler middleware.
    /// </summary>
    [Route("")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Error()
    {
        IExceptionHandlerFeature context = HttpContext.Features.Get<IExceptionHandlerFeature>();
        DTOErrorResponse dtoResponse;

        if (context?.Error is AppException appEx)
        {
            logger.LogError(appEx, "{Status} ({Code}) — {Messages}",
                appEx.Status, (int)appEx.Status, string.Join(" | ", appEx.Messages));

            dtoResponse = new(appEx.Status, appEx.Messages);
        }
        else
        {
            Exception ex = context?.Error;

            logger.LogError(ex, "Exceção não tratada: {Message}", ex?.Message);

            dtoResponse = new(HttpStatusCode.InternalServerError, ["Ocorreu um erro interno. Tente novamente."]);
            dtoResponse.HandleException(context?.Error);
        }

        Response.Headers.Append("ErrorCount", dtoResponse.ErrorCount.ToString());

        return StatusCode((int)dtoResponse.Status, dtoResponse);
    }
}
