using System.Text.Json.Serialization;

namespace TheOmenDen.Crowbot36.UI.Shared.Models.DivineApi.Fortunes;
public class Rootobject
{
    [JsonPropertyName("success")]
    public int Success { get; set; }
    [JsonPropertyName("message")]
    public string Message { get; set; }
    [JsonPropertyName("data")]
    public Data Data { get; set; }
}