using LeadSoft.Common.Library.EnvUtils;
using LeadSoft.Common.Library.Exceptions;
using System.Net;
using System.Runtime.Serialization;

namespace LeadSoft.Common.GlobalDomain.DTOs
{
    /// <summary>
    /// Represents a standardized error response containing HTTP status information and one or more error messages.
    /// </summary>
    /// <remarks>This class is typically used to return error details from an API or service endpoint. It
    /// includes the HTTP status code, a list of error messages, the number of errors, and the UTC timestamp when the
    /// error response was created.</remarks>
    /// <param name="status">The HTTP status code associated with the error response.</param>
    /// <param name="messages">A collection of error messages that describe the error condition. Cannot be null; an empty collection indicates
    /// no specific error messages.</param>
    [Serializable]
    [DataContract]
    public partial class DTOErrorResponse(HttpStatusCode status, params IEnumerable<string> messages)
    {
        /// <summary>
        /// Gets or sets the HTTP status code associated with the response.
        /// </summary>
        [DataMember]
        public HttpStatusCode Status { get; set; } = status;

        /// <summary>
        /// Gets or sets the collection of messages associated with the current context.
        /// </summary>
        [DataMember]
        public IEnumerable<string> Messages { get; set; } = messages ?? [];

        /// <summary>
        /// Gets the total number of error messages currently stored.
        /// </summary>
        [DataMember]
        public int ErrorCount => Messages.Count();

        /// <summary>
        /// Gets the UTC date and time at which the event occurred or the object was created.
        /// </summary>
        [DataMember]
        public DateTime At { get; } = DateTime.UtcNow;

        /// <summary>
        /// Initializes a new instance of the DTOErrorResponse class with the specified HTTP status code and one or more
        /// error messages.
        /// </summary>
        /// <param name="aStatus">The HTTP status code that represents the error condition.</param>
        /// <param name="aMessages">An array of error message strings to include in the response. Cannot be null.</param>
        public DTOErrorResponse(HttpStatusCode aStatus, params string[] aMessages) : this(aStatus, aMessages.AsEnumerable()) { }

        /// <summary>
        /// Handles the provided exception and updates the error response accordingly.
        /// </summary>
        /// <param name="exception">The exception to handle.</param>
        /// <returns>The updated error response.</returns>
        public DTOErrorResponse HandleException(Exception? exception)
        {
            if (exception is null)
                return this;

            if (exception is AppException appException)
            {
                Status = appException.Status;
                Messages = appException.Messages;
            }
            else if (EnvUtil.IsDevelopment())
                Messages = [exception.Message, exception.StackTrace ?? string.Empty];

            return this;
        }
    }
}
