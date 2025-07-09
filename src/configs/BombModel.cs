using System.Text.Json.Serialization;

namespace Cosmetics.Configs
{
    public class BombModelConfig
    {
        [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
        [JsonPropertyName("modify_planted_c4")] public bool ModifyPlantedC4 { get; set; } = true;
        [JsonPropertyName("modify_weapon_c4")] public bool ModifyWeaponC4 { get; set; } = true;
        [JsonPropertyName("min_size")] public float MinSize { get; set; } = 3f;
        [JsonPropertyName("max_size")] public float MaxSize { get; set; } = 7f;
    }
}
