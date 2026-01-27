namespace LeadSoft.Common.GlobalDomain.Handlers
{
    public partial class LeadSoftExceptionHandler
    {
        /// <summary>
        /// Represents a standardized error response containing status information, a title, error messages, and an
        /// optional trace identifier.
        /// </summary>
        /// <param name="Status">The HTTP status code associated with the error response.</param>
        /// <param name="Title">A short, human-readable summary of the error.</param>
        /// <param name="Messages">A collection of detailed error messages describing the error conditions.</param>
        /// <param name="TraceId">An optional identifier used to correlate this error response with a specific request or trace. Can be null.</param>
        public record ErrorResponse(int Status, string Title, IEnumerable<string> Messages, string? TraceId = null)
        {
            /// <summary>
            /// Gets the date and time when the object was created or last updated, in Coordinated Universal Time (UTC).
            /// </summary>
            public DateTime Timestamp { get; init; } = DateTime.UtcNow;

            /// <summary>
            /// Gets the number of error messages currently stored.
            /// </summary>
            public int ErrorCount => Messages.Count();
        }
    }
}
