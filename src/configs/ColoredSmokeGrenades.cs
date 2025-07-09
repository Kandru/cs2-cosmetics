using System.Text.Json.Serialization;

namespace Cosmetics.Configs
{
    public class ColoredSmokeGrenadesConfig
    {
        [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
    }
}
