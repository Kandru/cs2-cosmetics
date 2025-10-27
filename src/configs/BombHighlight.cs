using System.Text.Json.Serialization;

namespace Cosmetics.Configs
{
    public class BombHighlightConfig
    {
        [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
        [JsonPropertyName("highlight_planted_c4")] public bool HighlightPlantedC4 { get; set; } = true;
        [JsonPropertyName("highlight_lost_c4")] public bool HighlightLostC4 { get; set; } = true;
        [JsonPropertyName("highlight_color_c4")] public string ColorC4 { get; set; } = "237 163 56"; // rgb
    }
}
