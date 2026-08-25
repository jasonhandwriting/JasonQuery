using System;
using System.Net;

namespace JasonLibrary.Core.Update
{
    public enum UpdateMetadataFailureKind
    {
        NotFound,
        AccessDenied,
        Timeout,
        Network,
        ServerError,
        HttpError,
        ReadError
    }

    public sealed class UpdateMetadataLoadException : Exception
    {
        public UpdateMetadataLoadException(UpdateMetadataFailureKind failureKind, string location, string message,
                                           HttpStatusCode? statusCode = null, Exception innerException = null) : base(message, innerException)
        {
            FailureKind = failureKind;
            Location = location ?? string.Empty;
            StatusCode = statusCode;
        }

        public UpdateMetadataFailureKind FailureKind { get; }

        public string Location { get; }

        public HttpStatusCode? StatusCode { get; }
    }
}
