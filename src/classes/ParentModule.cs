using Microsoft.Extensions.Localization;

namespace Cosmetics.Classes
{
    public class ParentModule(PluginConfig Config, IStringLocalizer Localizer)
    {
        public readonly PluginConfig _config = Config;
        public readonly IStringLocalizer _localizer = Localizer;
        public virtual List<string> Events => [];
        public virtual List<string> Listeners => [];
        public virtual List<string> Precache => [];
        public readonly Random _random = new();

        public virtual void Destroy()
        {
            // This method can be overridden in derived classes to handle destruction logic
        }
    }
}
