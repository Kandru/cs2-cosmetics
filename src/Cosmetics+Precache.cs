using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Cosmetics
{
    public partial class Cosmetics : BasePlugin
    {
        private readonly List<string> _precacheModels = new List<string>
        {
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
