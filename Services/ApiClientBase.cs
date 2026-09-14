using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

namespace AccountingEngineAdmin.Services;

/// <summary>
/// Shared plumbing for the typed API clients: success/error handling and the
/// UTC date formatting required by the backend (PostgreSQL only accepts
/// timestamps with offset 0, so query-string dates are always sent as UTC).
/// </summary>
public abstract class ApiClientBase
{
    protected readonly HttpClient Http;

    protected ApiClientBase(HttpClient http) => Http = http;

    /// <summary>Renders a query-string date as UTC ISO 8601 ("...Z").</summary>
    protected static string? DateParam(DateTimeOffset? value) =>
        value is null ? null : Uri.EscapeDataString(value.Value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture));

    protected static string Escape(string segment) => Uri.EscapeDataString(segment);

    /// <summary>
    /// Appends a date range to the query list, skipping null bounds.
    /// Parameter names must match the API (camelCase, e.g. "from"/"to").
    /// </summary>
    protected static void AddDateRange(List<string> query, string fromName, DateTimeOffset? from, string toName, DateTimeOffset? to)
    {
        var fromValue = DateParam(from);
        if (fromValue is not null)
        {
            query.Add($"{fromName}={fromValue}");
        }

        var toValue = DateParam(to);
        if (toValue is not null)
        {
            query.Add($"{toName}={toValue}");
        }
    }

    protected async Task<T> SendForValueAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await EnsureSuccessAsync(response, cancellationToken);

        var payload = await response.Content.ReadFromJsonAsync<T>(ApiJson.Options, cancellationToken);
        return payload ?? throw new ApiException(
            (int)response.StatusCode, "The API returned an empty response.");
    }

    protected async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var message = await ReadErrorMessageAsync(response, cancellationToken);
        throw new ApiException(
            (int)response.StatusCode,
            string.IsNullOrWhiteSpace(message)
                ? $"The API returned {(int)response.StatusCode} {response.StatusCode}."
                : message!);
    }

    /// <summary>
    /// Reads a JSON payload tolerantly: returns null when the body is absent or
    /// not the expected type (some endpoints answer 200 with an empty body).
    /// </summary>
    protected async Task<T?> TryReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<T>(ApiJson.Options, cancellationToken);
        }
        catch (JsonException)
        {
            return default;
        }
        catch (NotSupportedException)
        {
            // Empty body (Content-Length 0) - ReadFromJsonAsync throws NotSupported.
            return default;
        }
    }

    /// <summary>
    /// The API uses two error shapes:
    /// - plain endpoints: { "error": "..." }
    /// - validation endpoints: ProblemDetails with { "errors": { "Field": ["msg", ...] } }
    /// Both are flattened into a single readable message.
    /// </summary>
    private static async Task<string?> ReadErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        string? body = null;
        try
        {
            body = await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (InvalidOperationException)
        {
            // Body already consumed by a previous read attempt.
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.String)
                {
                    return error.GetString();
                }

                if (root.TryGetProperty("title", out var title) && root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                {
                    var details = new List<string>();
                    foreach (var field in errors.EnumerateObject())
                    {
                        if (field.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in field.Value.EnumerateArray())
                            {
                                if (item.ValueKind == JsonValueKind.String)
                                {
                                    details.Add(item.GetString() ?? string.Empty);
                                }
                            }
                        }
                        else if (field.Value.ValueKind == JsonValueKind.String)
                        {
                            details.Add(field.Value.GetString() ?? string.Empty);
                        }
                    }

                    details.RemoveAll(string.IsNullOrWhiteSpace);
                    if (details.Count > 0)
                    {
                        var heading = title.ValueKind == JsonValueKind.String ? title.GetString() : null;
                        return heading is null
                            ? string.Join(' ', details)
                            : $"{heading} {string.Join(' ', details)}";
                    }

                    return title.ValueKind == JsonValueKind.String ? title.GetString() : null;
                }
            }
        }
        catch (JsonException)
        {
            // Body wasn't JSON (proxy page, HTML, plain-text exception dump, etc.).
        }

        return null;
    }
}
