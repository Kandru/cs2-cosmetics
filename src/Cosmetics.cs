using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace Cosmetics
{
    public partial class Cosmetics : BasePlugin, IPluginConfig<PluginConfig>
    {
        public override string ModuleName => "Cosmetics";
        public override string ModuleAuthor => "Jon-Mailes Graeffe <mail@jonni.it> / Kalle <kalle@kandru.de>";

        private string _currentMap = "";
        private Random _random = new Random();

        public override void Load(bool hotReload)
        {
            // initialize configuration
            LoadConfig();
            if (hotReload)
            {
                // set current map
                _currentMap = Server.MapName.ToLower();
                // initialize configuration
                InitializeConfig(_currentMap);
                // register listeners
                RegisterListeners(true);
            }
            // update configuration
            UpdateConfig();
            // save configuration
            SaveConfig();
        }

        public override void Unload(bool hotReload)
        {
            RemoveListeners(true);
            Console.WriteLine(Localizer["core.unload"]);
        }

        public void RegisterListeners(bool complete = false)
        {
            if (!Config.Enabled) return;
            if (complete)
            {
                RegisterListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
                RegisterListener<Listeners.OnMapStart>(OnMapStart);
                RegisterListener<Listeners.OnMapEnd>(OnMapEnd);
            }
            InitializeColoredSmokeGrenades();
            InitializeDeathBeams();
            InitializeSpectatorModel();
        }

        public void RemoveListeners(bool complete = false)
        {
            if (complete)
            {
                RemoveListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
                RemoveListener<Listeners.OnMapStart>(OnMapStart);
                RemoveListener<Listeners.OnMapEnd>(OnMapEnd);
            }
            ResetColoredSmokeGrenades();
            ResetDeathBeams();
            ResetSpectatorModel();
        }

        private void OnMapStart(string mapName)
        {
            // set current map
            _currentMap = mapName.ToLower();
            // initialize configuration
            LoadConfig();
            InitializeConfig(_currentMap);
            // update configuration
            UpdateConfig();
            // save configuration
            SaveConfig();
            // register listeners
            RegisterListeners();
        }

        private void OnMapEnd()
        {
            RemoveListeners();
        }
    }
}
