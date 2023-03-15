using System.Text.Json.Serialization;

namespace TheOmenDen.Crowbot36.UI.Shared.Models.DivineApi.Coffee;

public sealed class Data
{
    [JsonPropertyName("prediction")]
    public Prediction Prediction { get; set; }
}
