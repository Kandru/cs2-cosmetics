using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Cosmetics
{
    public partial class Cosmetics : BasePlugin
    {
        private readonly List<string> _precacheModels = new List<string>
        {
            "models/vehicles/airplane_medium_01/airplane_medium_01_landed.vmdl"
        };

        private void OnServerPrecacheResources(ResourceManifest manifest)
        {
            foreach (var model in _precacheModels)
            {
                manifest.AddResource(model);
            }
        }
    }
}
