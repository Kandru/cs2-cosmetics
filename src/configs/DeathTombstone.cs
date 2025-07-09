using System.Text.Json.Serialization;

namespace Cosmetics.Configs
{
    public class DeathTombstoneConfig
    {
        [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
        [JsonPropertyName("model")] public string Model { get; set; } = "models/cs2/kandru/tombstone.vmdl";
        [JsonPropertyName("size")] public float Size { get; set; } = 1.0f;
        [JsonPropertyName("health")] public int Health { get; set; } = 100;
    }
}
