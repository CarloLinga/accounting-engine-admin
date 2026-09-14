using System.Text.Json;
using System.Text.Json.Serialization;

namespace AccountingEngineAdmin.Services;

/// <summary>
/// JSON settings mirroring the Accounting Engine API contract
/// (camelCase properties, enums serialized as strings - see the API's
/// AddJsonOptions configuration).
/// </summary>
public static class ApiJson
{
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };
}