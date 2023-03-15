using System.Text.Json.Serialization;

namespace TheOmenDen.Crowbot36.UI.Shared.Models.DivineApi.Coffee;

public sealed class Prediction
{
    [JsonPropertyName("present_title")]
    public string PresentTitle { get; set; }

    [JsonPropertyName("present_image")]
    public string PresentImage { get; set; }

    [JsonPropertyName("present_content")]
    public string PresentContent { get; set; }

    [JsonPropertyName("near_future_title")]
    public string NearFutureTitle { get; set; }

    [JsonPropertyName("near_future_image")]
    public string NearFutureImage { get; set; }

    [JsonPropertyName("near_future_content")]
    public string NearFutureContent { get; set; }

    [JsonPropertyName("distant_future_title")]
    public string DistantFutureTitle { get; set; }

    [JsonPropertyName("distant_future_image")]
    public string DistantFutureImage { get; set; }

    [JsonPropertyName("distant_future_content")]
    public string DistantFutureContent { get; set; }
}