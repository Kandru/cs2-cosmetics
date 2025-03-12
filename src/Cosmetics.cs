using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Extensions;

namespace Cosmetics
{
    public partial class Cosmetics : BasePlugin, IPluginConfig<PluginConfig>
    {
        public override string ModuleName => "Cosmetics";
        public override string ModuleAuthor => "Jon-Mailes Graeffe <mail@jonni.it> / Kalle <kalle@kandru.de>";

        private Random _random = new Random();

        public override void Load(bool hotReload)
        {
            if (!Config.Enabled) return;
            // register global listeners
            RegisterListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
            RegisterListener<Listeners.OnMapStart>(OnMapStart);
            RegisterListener<Listeners.OnMapEnd>(OnMapEnd);
            // register module listeners
            InitializeColoredSmokeGrenades();
            InitializeDeathBeams();
            InitializeSpectatorModel();
            InitializeBombModel();
            // initialize configuration
            if (hotReload)
            {
                // reload configuration
                Config.Reload();
                // save configuration
                Config.Update();
            }
        }

        public override void Unload(bool hotReload)
        {
            // unregister global listeners
            RemoveListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
            RemoveListener<Listeners.OnMapStart>(OnMapStart);
            RemoveListener<Listeners.OnMapEnd>(OnMapEnd);
            // unregister module listeners
            ResetColoredSmokeGrenades();
            ResetDeathBeams();
            ResetSpectatorModel();
            ResetBombModel();
            Console.WriteLine(Localizer["core.unload"]);
        }

        private void OnMapStart(string mapName)
        {
            // reload configuration
            Config.Reload();
            // save configuration
            Config.Update();
            // register listeners
            InitializeColoredSmokeGrenades();
            InitializeDeathBeams();
            InitializeSpectatorModel();
            InitializeBombModel();
        }

        private void OnMapEnd()
        {
            ResetColoredSmokeGrenades();
            ResetDeathBeams();
            ResetSpectatorModel();
            ResetBombModel();
        }
    }
}
