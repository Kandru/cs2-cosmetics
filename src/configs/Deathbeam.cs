using System.Text.Json.Serialization;

namespace Cosmetics.Configs
{
    public class DeathbeamConfig
    {
        [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
        [JsonPropertyName("timeout")] public float Timeout { get; set; } = 1.5f;
        [JsonPropertyName("width")] public float Width { get; set; } = 0.5f;
        [JsonPropertyName("color_t")] public string ColorTerrorists { get; set; } = "237 163 56"; // rgb
        [JsonPropertyName("color_ct")] public string ColorCounterTerrorists { get; set; } = "104 163 229"; // rgb
        [JsonPropertyName("color_other")] public string ColorOther { get; set; } = "0 255 0"; // rgb
    }
}
