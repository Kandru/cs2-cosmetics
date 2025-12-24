using System.Text.Json.Serialization;

namespace Cosmetics.Configs
{
    public class BombModelConfig
    {
        [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
        [JsonPropertyName("delay_size_change")] public float DelaySizeChange { get; set; } = 2f;
        [JsonPropertyName("change_size_on_planted_c4")] public bool ChangeSizeOnPlant { get; set; } = true;
        [JsonPropertyName("change_size_on_equip_c4")] public bool ChangeSizeOnEquip { get; set; } = true;
        [JsonPropertyName("min_size")] public float MinSize { get; set; } = 3f;
        [JsonPropertyName("max_size")] public float MaxSize { get; set; } = 7f;
    }
}
