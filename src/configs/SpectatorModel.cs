using System.Text.Json.Serialization;

namespace Cosmetics.Configs
{
    public class SpectatorModelConfig
    {
        [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
    }
}
