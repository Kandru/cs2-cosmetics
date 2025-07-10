using System.Text.Json.Serialization;

namespace Cosmetics.Configs
{
    public class SpectatorModelConfig
    {
        [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
        [JsonPropertyName("model")] public string Model { get; set; } = "models/vehicles/airplane_medium_01/airplane_medium_01_landed.vmdl";
        [JsonPropertyName("size")] public float Size { get; set; } = 0.01f;
        [JsonPropertyName("offset_z")] public float OffsetZ { get; set; } = -2f;
        [JsonPropertyName("offset_angle")] public float OffsetAngle { get; set; } = 0f;
    }
}
