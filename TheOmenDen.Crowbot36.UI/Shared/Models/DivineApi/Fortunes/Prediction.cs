using System.Text.Json.Serialization;

namespace TheOmenDen.Crowbot36.UI.Shared.Models.DivineApi.Fortunes;

public sealed class Prediction
{
    [JsonPropertyName("result")]
    public string Result { get; set; }
}
