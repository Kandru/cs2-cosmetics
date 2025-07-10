using Cosmetics.Classes;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Utils;
using System.Reflection;

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
                // register listeners
                RegisterListeners();
                RegisterEventHandlers();
            }
        }

        public override void Unload(bool hotReload)
        {
            // deregister global listeners
            RemoveListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
            RemoveListener<Listeners.OnMapStart>(OnMapStart);
            RemoveListener<Listeners.OnMapEnd>(OnMapEnd);
            // deregister listeners
            DeregisterListeners();
            DeregisterEventHandlers();
            // destroy modules
            DestroyModules();
            Console.WriteLine(Localizer["core.unload"]);
        }

        private void OnServerPrecacheResources(ResourceManifest manifest)
        {
            // initialize modules (otherwise no models are precached)
            InitializeModules();
            // precache resources for all cosmetics modules
            DebugPrint("Pre-caching resources for cosmetics modules...");
            foreach (ParentModule module in _cosmetics)
            {
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
            // register listeners
            RegisterListeners();
            RegisterEventHandlers();
        }

        private void OnMapEnd()
        {
            // deregister listeners
            DeregisterListeners();
            DeregisterEventHandlers();
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
            // initialize DeathBeam module
            if (config.Modules.DeathBeam.Enabled)
            {
                _cosmetics.Add(new DeathBeam(config));
            }
            // initialize ColoredSmokeGrenades module
            if (config.Modules.ColoredSmokeGrenades.Enabled)
            {
                _cosmetics.Add(new ColoredSmokeGrenades(config));
            }
            // initialize DeathTombstone module
            if (config.Modules.DeathTombstone.Enabled)
            {
                _cosmetics.Add(new DeathTombstone(config));
            }
            // initialize SpectatorModel module
            if (config.Modules.SpectatorModel.Enabled)
            {
                _cosmetics.Add(new SpectatorModel(config));
            }
        }

        private void RegisterListeners()
        {
            foreach (ParentModule module in _cosmetics)
            {
                DebugPrint($"Initializing listener for module {module.GetType().Name}");
                foreach (string listenerName in module.Listeners)
                {
                    DebugPrint($"- {listenerName}");
                    RegisterModuleListener(listenerName, module);
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
                    DeregisterModuleListener(listenerName, module);
                }
            }
        }

        private void RegisterModuleListener(string listenerName, ParentModule module)
        {
            // get the listener type from CounterStrikeSharp.API.Core.Listeners
            Type? listenerType = typeof(Listeners).GetNestedType(listenerName);
            if (listenerType == null)
            {
                DebugPrint($"Listener type {listenerName} not found");
                return;
            }
            // get the method from the module
            MethodInfo? method = module.GetType().GetMethod(listenerName);
            if (method == null)
            {
                DebugPrint($"Method {listenerName} not found in module {module.GetType().Name}");
                return;
            }
            // create delegate
            Delegate handler = Delegate.CreateDelegate(listenerType, module, method);
            // use reflection to call RegisterListener<T>
            MethodInfo? registerMethod = typeof(BasePlugin).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "RegisterListener" && m.IsGenericMethodDefinition && m.GetParameters().Length == 1);
            if (registerMethod == null)
            {
                DebugPrint("RegisterListener method not found.");
                return;
            }
            MethodInfo genericRegisterMethod = registerMethod.MakeGenericMethod(listenerType);
            _ = genericRegisterMethod.Invoke(this, [handler]);
        }

        private void DeregisterModuleListener(string listenerName, ParentModule module)
        {
            // get the listener type from CounterStrikeSharp.API.Core.Listeners
            Type? listenerType = typeof(Listeners).GetNestedType(listenerName);
            if (listenerType == null)
            {
                DebugPrint($"Listener type {listenerName} not found");
                return;
            }
            // get the method from the module
            MethodInfo? method = module.GetType().GetMethod(listenerName);
            if (method == null)
            {
                DebugPrint($"Method {listenerName} not found in module {module.GetType().Name}");
                return;
            }
            // create delegate
            Delegate handler = Delegate.CreateDelegate(listenerType, module, method);
            // use reflection to call RemoveListener<T>
            MethodInfo? removeMethod = typeof(BasePlugin).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "RemoveListener" && m.IsGenericMethodDefinition && m.GetParameters().Length == 1);
            if (removeMethod == null)
            {
                DebugPrint("RemoveListener method not found.");
                return;
            }
            MethodInfo genericRemoveMethod = removeMethod.MakeGenericMethod(listenerType);
            _ = genericRemoveMethod.Invoke(this, [handler]);
        }

        private void RegisterEventHandlers()
        {
            foreach (ParentModule module in _cosmetics)
            {
                DebugPrint($"Initializing event handlers for module {module.GetType().Name}");
                foreach (string eventName in module.Events)
                {
                    DebugPrint($"- {eventName}");
                    RegisterModuleEventHandler(eventName, module);
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
                    DeregisterModuleEventHandler(eventName, module);
                }
            }
        }

        private void RegisterModuleEventHandler(string eventName, ParentModule module)
        {
            // get the event type from CounterStrikeSharp.API.Core
            Type? eventType = typeof(BasePlugin).Assembly.GetType($"CounterStrikeSharp.API.Core.{eventName}");
            if (eventType == null)
            {
                DebugPrint($"Event type {eventName} not found");
                return;
            }

            // get the method from the module
            MethodInfo? method = module.GetType().GetMethod(eventName);
            if (method == null)
            {
                DebugPrint($"Method {eventName} not found in module {module.GetType().Name}");
                return;
            }

            // create delegate using Func<T, GameEventInfo, HookResult> for event handlers
            Type gameEventHandlerType = typeof(BasePlugin).GetNestedType("GameEventHandler`1")!.MakeGenericType(eventType);
            Delegate handler = Delegate.CreateDelegate(gameEventHandlerType, module, method);

            // use reflection to call RegisterEventHandler<T>
            MethodInfo? registerMethod = typeof(BasePlugin).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "RegisterEventHandler" && m.IsGenericMethodDefinition && m.GetParameters().Length == 2);
            if (registerMethod == null)
            {
                DebugPrint("RegisterEventHandler method not found.");
                return;
            }
            MethodInfo genericRegisterMethod = registerMethod.MakeGenericMethod(eventType);
            _ = genericRegisterMethod.Invoke(this, [handler, HookMode.Pre]);
        }

        private void DeregisterModuleEventHandler(string eventName, ParentModule module)
        {
            // get the event type from CounterStrikeSharp.API.Core
            Type? eventType = typeof(BasePlugin).Assembly.GetType($"CounterStrikeSharp.API.Core.{eventName}");
            if (eventType == null)
            {
                DebugPrint($"Event type {eventName} not found");
                return;
            }

            // get the method from the module
            MethodInfo? method = module.GetType().GetMethod(eventName);
            if (method == null)
            {
                DebugPrint($"Method {eventName} not found in module {module.GetType().Name}");
                return;
            }

            // create delegate using BasePlugin.GameEventHandler<T>
            Type gameEventHandlerType = typeof(BasePlugin).GetNestedType("GameEventHandler`1")!.MakeGenericType(eventType);
            Delegate handler = Delegate.CreateDelegate(gameEventHandlerType, module, method);

            // use reflection to call DeregisterEventHandler<T>
            MethodInfo? deregisterMethod = typeof(BasePlugin).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "DeregisterEventHandler" && m.IsGenericMethodDefinition && m.GetParameters().Length == 2);
            if (deregisterMethod == null)
            {
                DebugPrint("DeregisterEventHandler method not found.");
                return;
            }
            MethodInfo genericDeregisterMethod = deregisterMethod.MakeGenericMethod(eventType);
            _ = genericDeregisterMethod.Invoke(this, [handler, HookMode.Pre]);
        }

        private void DestroyModules()
        {
            // destroy all cosmetics modules
            foreach (ParentModule module in _cosmetics)
            {
                module.Destroy();
            }
            _cosmetics.Clear();
        }
    }
}