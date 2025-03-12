using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace Cosmetics
{
    public class BombModelConfig
    {
        [JsonPropertyName("enable")] public bool Enable { get; set; } = true;
        [JsonPropertyName("modify_planted_c4")] public bool ModifyPlantedC4 { get; set; } = true;
        [JsonPropertyName("modify_weapon_c4")] public bool ModifyWeaponC4 { get; set; } = true;
        [JsonPropertyName("min_size")] public float MinSize { get; set; } = 3f;
        [JsonPropertyName("max_size")] public float MaxSize { get; set; } = 7f;
    }

    public class ColoredSmokeGrenadesConfig
    {
        [JsonPropertyName("enable")] public bool Enable { get; set; } = true;
    }

    public class DeathbeamConfig
    {
        [JsonPropertyName("enable")] public bool Enable { get; set; } = true;
    }


    public class SpectatorModelConfig
    {
        [JsonPropertyName("enable")] public bool Enable { get; set; } = true;
    }

    public class MapConfig
    {
        [JsonPropertyName("bombmodel")] public BombModelConfig BombModel { get; set; } = new BombModelConfig();
        [JsonPropertyName("coloredsmokegrenades")] public ColoredSmokeGrenadesConfig ColoredSmokeGrenades { get; set; } = new ColoredSmokeGrenadesConfig();
        [JsonPropertyName("deathbeam")] public DeathbeamConfig DeathBeam { get; set; } = new DeathbeamConfig();
        [JsonPropertyName("spectatormodel")] public SpectatorModelConfig SpectatorModel { get; set; } = new SpectatorModelConfig();
    }

    public class PluginConfig : BasePluginConfig
    {
        // global kill switch
        [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
        // global map settings
        [JsonPropertyName("global")] public MapConfig Global { get; set; } = new MapConfig();
        // map configurations
        [JsonPropertyName("maps")] public Dictionary<string, MapConfig> MapConfigs { get; set; } = [];
    }

    public partial class Cosmetics : BasePlugin, IPluginConfig<PluginConfig>
    {
        public PluginConfig Config { get; set; } = null!;
        private Dictionary<string, MapConfig> _mapConfigs = [];

        public void OnConfigParsed(PluginConfig config)
        {
            Config = config;
            Console.WriteLine("[Cosmetics] Initialized configuration!");
        }
    }
}
