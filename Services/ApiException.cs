namespace AccountingEngineAdmin.Services;

/// <summary>
/// Raised when the API answers with a non-success status code. Carries the HTTP
/// status and - when available - the API's `{"error": "..."}` message, so the
/// UI can surface exactly what the backend rejected (e.g. delete conflicts).
/// </summary>
public sealed class ApiException : Exception
{
    public int StatusCode { get; }

    public ApiException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}