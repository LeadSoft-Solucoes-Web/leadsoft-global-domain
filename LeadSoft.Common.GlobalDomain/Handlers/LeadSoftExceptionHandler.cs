using LeadSoft.Common.Library.EnvUtils;
using LeadSoft.Common.Library.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LeadSoft.Common.GlobalDomain.Handlers
{
    /// <summary>
    /// Intercepta exceções não tratadas globalmente para fornecer respostas HTTP padronizadas e seguras.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Este handler centraliza a lógica de erro, convertendo exceções em um formato de resposta consistente. 
    /// Ele diferencia o nível de detalhe com base no ambiente de execução:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <term>Desenvolvimento:</term> 
    /// <description>Inclui mensagens detalhadas e StackTrace para facilitar o debugging.</description>
    /// </item>
    /// <item>
    /// <term>Produção:</term> 
    /// <description>Omite detalhes sensíveis, retornando mensagens genéricas para garantir a segurança.</description>
    /// </item>
    /// </list>
    /// <para>
    /// A resposta inclui o cabeçalho <c>Error-Count</c> e segue, preferencialmente, o padrão RFC 7807 (Problem Details).
    /// </para>
    /// <example>
    /// Exemplo de configuração no <c>Program.cs</c>:
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// 
    /// builder.Services.AddExceptionHandler&lt;GlobalExceptionHandler&gt;();
    /// builder.Services.AddProblemDetails();
    /// 
    /// var app = builder.Build();
    /// 
    /// app.UseExceptionHandler(); 
    /// app.MapControllers();
    /// app.Run();
    /// </code>
    /// </example>
    /// </remarks>
    /// <param name="logger">O logger para registro estruturado de erros e rastreamento de diagnósticos.</param>
    public partial class LeadSoftExceptionHandler(ILogger<LeadSoftExceptionHandler> logger) : IExceptionHandler
    {
        /// <summary>
        /// Attempts to handle the specified exception by generating an appropriate HTTP error response asynchronously.
        /// </summary>
        /// <remarks>The response includes an error status code, a title, and error messages based on the
        /// exception type. An "Error-Count" header is added to the response indicating the number of error messages.
        /// The method always returns <see langword="true"/> after writing the response.</remarks>
        /// <param name="httpContext">The HTTP context for the current request. Used to write the error response. Cannot be null.</param>
        /// <param name="exception">The exception to handle. Determines the error response content. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the
        /// exception was handled and a response was written.</returns>
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError(exception, "Um erro não tratado ocorreu: {Message}", exception.Message);

            var (statusCode, title, messages) = exception switch
            {
                AppException appEx => ((int)appEx.Status, "Application Error", appEx.Messages),
                _ => (StatusCodes.Status500InternalServerError, "Server Error", GetDefaultMessages(exception))
            };

            httpContext.Response.StatusCode = statusCode;

            ErrorResponse response = new(statusCode, title, messages, httpContext.TraceIdentifier);

            httpContext.Response.Headers.Append("Error-Count", response.ErrorCount.ToString());

            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true;
        }

        /// <summary>
        /// Returns a collection of default error messages based on the current environment and the specified exception.
        /// </summary>
        /// <remarks>This method provides more detailed error information when running in a development
        /// environment to aid debugging. In production or non-development environments, it returns a generic message to
        /// avoid exposing sensitive information.</remarks>
        /// <param name="ex">The exception for which to generate default error messages. Cannot be null.</param>
        /// <returns>An enumerable collection of strings containing error messages. In development environments, the collection
        /// includes the exception message and stack trace; in other environments, it contains a generic error message.</returns>
        private IEnumerable<string> GetDefaultMessages(Exception ex)
            => EnvUtil.IsDevelopment()
                ? [ex.Message, ex.StackTrace ?? string.Empty]
                : ["Ocorreu um erro interno no servidor."];
    }
}
