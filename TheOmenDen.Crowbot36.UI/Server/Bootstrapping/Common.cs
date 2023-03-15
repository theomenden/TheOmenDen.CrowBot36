using System.Text.Json.Serialization;
using System.Text.Json;
using TheOmenDen.Shared.Enumerations.Serialization;
using TheOmenDen.Shared.Enumerations;

namespace TheOmenDen.Crowbot36.UI.Server.Bootstrapping;

public static class Common
{
    public static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            new EnumerationNameConverter<EventIdIdentifier, Int32>(),
        },
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}