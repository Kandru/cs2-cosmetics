using System.Text.Json.Serialization;

namespace Cosmetics.Configs
{
    public class ColoredSmokeGrenadesConfig
    {
        [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
        [JsonPropertyName("color_t")] public string ColorTerrorists { get; set; } = "255 0 0"; // rgb
        [JsonPropertyName("color_ct")] public string ColorCounterTerrorists { get; set; } = "0 0 255"; // rgb
        [JsonPropertyName("color_other")] public string ColorOther { get; set; } = "0 255 0"; // rgb
    }
}
