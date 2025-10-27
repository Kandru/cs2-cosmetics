using Cosmetics.Classes;
using Cosmetics.Utils;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Utils;

namespace Cosmetics
{
    public partial class Cosmetics : BasePlugin, IPluginConfig<PluginConfig>
    {
        public override string ModuleName => "Cosmetics";
        public override string ModuleAuthor => "Kalle <kalle@kandru.de>";
        private readonly List<ParentModule> _cosmetics = [];

        public override void Load(bool hotReload)
        {
            if (!Config.Enabled)
            {
                return;
            }
            // register global listeners
            RegisterListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
            RegisterListener<Listeners.OnMapStart>(OnMapStart);
            RegisterListener<Listeners.OnMapEnd>(OnMapEnd);
            // initialize configuration
            if (hotReload)
            {
                // reload configuration
                Config.Reload();
                // save configuration
                Config.Update();
                // initialize modules
                InitializeModules();
            }
        }

        public override void Unload(bool hotReload)
        {
            // deregister global listeners
            RemoveListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
            RemoveListener<Listeners.OnMapStart>(OnMapStart);
            RemoveListener<Listeners.OnMapEnd>(OnMapEnd);
            // destroy modules
            DestroyModules();
            Console.WriteLine(Localizer["core.unload"]);
        }

        private void OnServerPrecacheResources(ResourceManifest manifest)
        {
            // precache resources for all cosmetics modules
            DebugPrint("Pre-caching resources for cosmetics modules...");
            foreach (ParentModule module in _cosmetics)
            {
                DebugPrint($"Module {module.GetType().Name} has {module.Precache.Count} resources to precache.");
                foreach (string model in module.Precache)
                {
                    DebugPrint($"Pre-caching model: {model}");
                    manifest.AddResource(model);
                }
            }
        }

        private void OnMapStart(string mapName)
        {
            // reload configuration
            Config.Reload();
            // load map-specific configuration
            LoadConfig(mapName);
            // initialize modules
            InitializeModules();
        }

        private void OnMapEnd()
        {
            // destroy modules
            DestroyModules();
            // reset map-specific configuration
            _mapConfig = null;
        }

        private void InitializeModules()
        {
            if (_cosmetics.Count > 0)
            {
                return;
            }
            // get current configuration
            PluginConfig config = _mapConfig ?? Config;
            // skip if globally disabled
            if (!config.Enabled)
            {
                return;
            }
            // initialize BombModel module
            if (config.Modules.BombModel.Enabled)
            {
                _cosmetics.Add(new BombModel(config, Localizer));
            }
            // initialize DeathBeam module
            if (config.Modules.DeathBeam.Enabled)
            {
                _cosmetics.Add(new DeathBeam(config, Localizer));
            }
            // initialize ColoredSmokeGrenades module
            if (config.Modules.ColoredSmokeGrenades.Enabled)
            {
                _cosmetics.Add(new ColoredSmokeGrenades(config, Localizer));
            }
            // initialize DeathTombstone module
            if (config.Modules.DeathTombstone.Enabled)
            {
                _cosmetics.Add(new DeathTombstone(config, Localizer));
            }
            // initialize SpectatorModel module
            if (config.Modules.SpectatorModel.Enabled)
            {
                _cosmetics.Add(new SpectatorModel(config, Localizer));
            }
            // register listeners
            RegisterListeners();
            RegisterEventHandlers();
            RegisterUserMessageHooks();
        }

        private void RegisterListeners()
        {
            foreach (ParentModule module in _cosmetics)
            {
                DebugPrint($"Initializing listener for module {module.GetType().Name}");
                foreach (string listenerName in module.Listeners)
                {
                    DebugPrint($"- {listenerName}");
                    DynamicHandlers.RegisterModuleListener(this, listenerName, module);
                }
            }
        }

        private void DeregisterListeners()
        {
            foreach (ParentModule module in _cosmetics)
            {
                DebugPrint($"Destroying listener for module {module.GetType().Name}");
                foreach (string listenerName in module.Listeners)
                {
                    DebugPrint($"- {listenerName}");
                    DynamicHandlers.DeregisterModuleListener(this, listenerName, module);
                }
            }
        }

        private void RegisterEventHandlers()
        {
            foreach (ParentModule module in _cosmetics)
            {
                DebugPrint($"Initializing event handlers for module {module.GetType().Name}");
                foreach (string eventName in module.Events)
                {
                    DebugPrint($"- {eventName}");
                    DynamicHandlers.RegisterModuleEventHandler(this, eventName, module);
                }
            }
        }

        private void DeregisterEventHandlers()
        {
            foreach (ParentModule module in _cosmetics)
            {
                DebugPrint($"Destroying event handlers for module {module.GetType().Name}");
                foreach (string eventName in module.Events)
                {
                    DebugPrint($"- {eventName}");
                    DynamicHandlers.DeregisterModuleEventHandler(this, eventName, module);
                }
            }
        }

        private void RegisterUserMessageHooks()
        {
            foreach (ParentModule module in _cosmetics)
            {
                DebugPrint($"Registering user messages for module {module.GetType().Name}");
                foreach ((int userMessageId, HookMode hookMode) in module.UserMessages)
                {
                    DebugPrint($"- UserMessage ID: {userMessageId}, HookMode: {hookMode}");
                    DynamicHandlers.RegisterUserMessageHook(this, userMessageId, module, hookMode);
                }
            }
        }

        private void DeregisterUserMessageHooks()
        {
            foreach (ParentModule module in _cosmetics)
            {
                DebugPrint($"Deregistering user messages for module {module.GetType().Name}");
                foreach ((int userMessageId, HookMode hookMode) in module.UserMessages)
                {
                    DebugPrint($"- UserMessage ID: {userMessageId}, HookMode: {hookMode}");
                    DynamicHandlers.DeregisterUserMessageHook(this, userMessageId, module, hookMode);
                }
            }
        }

        private void DestroyModules()
        {
            DebugPrint("Destroying all modules...");
            // deregister listeners
            DeregisterListeners();
            DeregisterEventHandlers();
            DeregisterUserMessageHooks();
            // destroy all cosmetics modules
            foreach (ParentModule module in _cosmetics)
            {
                module.Destroy();
            }
            _cosmetics.Clear();
        }
    }
}