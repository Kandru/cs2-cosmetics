using System.IO.Enumeration;
using System.Text.Json;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Config;

namespace Cosmetics
{
    public class MapConfig
    {
        [JsonPropertyName("enable_coloredsmokegrenades")] public bool EnableColoredSmokeGrenades { get; set; } = true;
        [JsonPropertyName("enable_deathbeam")] public bool EnableDeathBeam { get; set; } = true;
        [JsonPropertyName("enable_lighting")] public bool EnableLighting { get; set; } = true;
        [JsonPropertyName("enable_specatormodel")] public bool EnableSpectatorModel { get; set; } = true;
    }

    public class PluginConfig : BasePluginConfig
    {
        // global settings
        [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
        [JsonPropertyName("enable_coloredsmokegrenades")] public bool EnableColoredSmokeGrenades { get; set; } = true;
        [JsonPropertyName("enable_deathbeam")] public bool EnableDeathBeam { get; set; } = true;
        [JsonPropertyName("enable_lighting")] public bool EnableLighting { get; set; } = true;
        [JsonPropertyName("enable_specatormodel")] public bool EnableSpectatorModel { get; set; } = true;
        // map configurations
        [JsonPropertyName("maps")] public Dictionary<string, MapConfig> MapConfigs { get; set; } = new Dictionary<string, MapConfig>();
    }

    public partial class Cosmetics : BasePlugin, IPluginConfig<PluginConfig>
    {
        public PluginConfig Config { get; set; } = null!;
        private MapConfig[] _currentMapConfigs = Array.Empty<MapConfig>();
        private string _configPath = "";

        private void LoadConfig()
        {
            Config = ConfigManager.Load<PluginConfig>("Cosmetics");
            _configPath = Path.Combine(ModuleDirectory, $"../../configs/plugins/Cosmetics/Cosmetics.json");
        }

        private void InitializeConfig(string mapName)
        {
            // select map configs whose regexes (keys) match against the map name
            _currentMapConfigs = (from mapConfig in Config.MapConfigs
                                  where FileSystemName.MatchesSimpleExpression(mapConfig.Key, mapName)
                                  select mapConfig.Value).ToArray();

            if (_currentMapConfigs.Length > 0)
            {
                if (Config.MapConfigs.TryGetValue("default", out var config))
                {
                    // add default configuration
                    _currentMapConfigs = new[] { config };
                    Console.WriteLine(Localizer["core.defaultconfig"].Value.Replace("{mapName}", mapName));
                }
                else
                {
                    // there is no config to apply
                    Console.WriteLine(Localizer["core.noconfig"].Value.Replace("{mapName}", mapName));
                }
            }
            else
            {
                Console.WriteLine(Localizer["core.defaultconfig"].Value.Replace("{mapName}", mapName));
                // create default configuration
                Config.MapConfigs.Add(mapName, new MapConfig());
            }
            Console.WriteLine(Localizer["core.foundconfig"].Value.Replace("{count}", _currentMapConfigs.Length.ToString()).Replace("{mapName}", mapName));
        }

        public void OnConfigParsed(PluginConfig config)
        {
            Config = config;
            Console.WriteLine("[Cosmetics] Initialized map configuration!");
        }

        private void SaveConfig()
        {
            var jsonString = JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, jsonString);
        }
    }
}
