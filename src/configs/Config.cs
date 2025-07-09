using Cosmetics.Configs;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cosmetics
{
    public class ModuleConfig
    {
        [JsonPropertyName("bombmodel")] public BombModelConfig BombModel { get; set; } = new BombModelConfig();
        [JsonPropertyName("coloredsmokegrenades")] public ColoredSmokeGrenadesConfig ColoredSmokeGrenades { get; set; } = new ColoredSmokeGrenadesConfig();
        [JsonPropertyName("deathbeam")] public DeathbeamConfig DeathBeam { get; set; } = new DeathbeamConfig();
        [JsonPropertyName("deathtombstone")] public DeathTombstoneConfig DeathTombstone { get; set; } = new DeathTombstoneConfig();
        [JsonPropertyName("spectatormodel")] public SpectatorModelConfig SpectatorModel { get; set; } = new SpectatorModelConfig();
    }

    public class PluginConfig : BasePluginConfig
    {
        // kill switch
        [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
        // module settings
        [JsonPropertyName("modules")] public ModuleConfig Modules { get; set; } = new ModuleConfig();
    }

    public partial class Cosmetics : BasePlugin, IPluginConfig<PluginConfig>
    {
        private static readonly JsonSerializerOptions CachedJsonOptions = new() { WriteIndented = true };
        public PluginConfig Config { get; set; } = null!;
        private PluginConfig? _mapConfig;

        public void OnConfigParsed(PluginConfig config)
        {
            Config = config;
            // update config and write new values from plugin to config file if changed after update
            Config.Update();
            Console.WriteLine("[Cosmetics] Initialized configuration!");
        }

        public void LoadConfig(string mapName)
        {
            string mapConfigPath = Path.Combine(
                $"{Path.GetDirectoryName(Config.GetConfigPath())}/maps/" ?? "./maps/",
                $"{mapName.ToLower(System.Globalization.CultureInfo.CurrentCulture)}.json"
            );
            // skip if map config file does not exist
            if (!File.Exists(mapConfigPath))
            {
                Console.WriteLine(Localizer["core.defaultconfig"].Value.Replace("{mapName}", mapName));
                return;
            }
            // try to load map config
            Console.WriteLine(Localizer["core.foundconfig"].Value.Replace("{mapName}", mapName));
            try
            {
                string jsonString = File.ReadAllText(mapConfigPath);
                PluginConfig? _mapConfig = JsonSerializer.Deserialize<PluginConfig>(jsonString, CachedJsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine(Localizer["core.debugprint"].Value.Replace("{message}", ex.Message));
            }
        }
    }
}
