using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace Cosmetics
{
    public partial class Cosmetics : BasePlugin, IPluginConfig<PluginConfig>
    {
        public override string ModuleName => "Cosmetics";
        public override string ModuleAuthor => "Jon-Mailes Graeffe <mail@jonni.it> / Kalle <kalle@kandru.de>";
        public override string ModuleVersion => "1.0.0";

        private string _currentMap = "";
        private Random _random = new Random();

        public override void Load(bool hotReload)
        {
            // initialize configuration
            LoadConfig();
            SaveConfig();
            RegisterListeners();
            // print message if hot reload
            if (hotReload)
            {
                // set current map
                _currentMap = Server.MapName;
                // initialize configuration
                InitializeConfig(_currentMap);
            }
        }

        public override void Unload(bool hotReload)
        {
            RemoveListeners();
            Console.WriteLine(Localizer["core.unload"]);
        }

        public void RegisterListeners()
        {
            RegisterListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
            RegisterListener<Listeners.OnMapStart>(OnMapStart);
            RegisterListener<Listeners.OnMapEnd>(OnMapEnd);
            InitializeColoredSmokeGrenades();
        }

        public void RemoveListeners()
        {
            RemoveListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
            RemoveListener<Listeners.OnMapStart>(OnMapStart);
            RemoveListener<Listeners.OnMapEnd>(OnMapEnd);
            ResetColoredSmokeGrenades();
        }

        private void OnMapStart(string mapName)
        {
            // set current map
            _currentMap = mapName;
        }

        private void OnMapEnd()
        {

        }
    }
}
