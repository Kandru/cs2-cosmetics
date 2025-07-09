using System.Text.Json.Serialization;

namespace Cosmetics.Configs
{
    public class DeathbeamConfig
    {
        [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
    }
}
