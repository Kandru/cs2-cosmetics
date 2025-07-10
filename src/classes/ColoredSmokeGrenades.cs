using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;

namespace Cosmetics.Classes
{
    public class ColoredSmokeGrenades : ParentModule
    {
        public override List<string> Listeners =>
        [
            "OnEntitySpawned"
        ];

        public ColoredSmokeGrenades(PluginConfig Config, IStringLocalizer Localizer) : base(Config, Localizer)
        {
            Console.WriteLine("[Cosmetics] Initializing ColoredSmokeGrenades module...");
        }

        public override void Destroy()
        {
            Console.WriteLine("[Cosmetics] Destroying ColoredSmokeGrenades module...");
        }

        public void OnEntitySpawned(CEntityInstance entity)
        {
            if (entity.DesignerName != "smokegrenade_projectile")
            {
                return;
            }

            nint handle = entity.Handle;
            Server.NextFrame(() =>
            {
                CSmokeGrenadeProjectile grenade = new(handle);
                if (grenade.Handle == IntPtr.Zero)
                {
                    return;
                }

                CBaseEntity? owner = grenade.OwnerEntity?.Value;
                if (owner == null)
                {
                    return;
                }
                int r = 0, g = 0, b = 0;
                if (owner.TeamNum == (byte)CsTeam.Terrorist)
                {
                    if (!string.IsNullOrEmpty(_config.Modules.ColoredSmokeGrenades.ColorTerrorists))
                    {
                        string[] parts = _config.Modules.ColoredSmokeGrenades.ColorTerrorists.Split(' ');
                        if (parts.Length == 3)
                        {
                            _ = int.TryParse(parts[0], out r);
                            _ = int.TryParse(parts[1], out g);
                            _ = int.TryParse(parts[2], out b);
                        }
                    }
                }
                else if (owner.TeamNum == (byte)CsTeam.CounterTerrorist)
                {
                    if (!string.IsNullOrEmpty(_config.Modules.ColoredSmokeGrenades.ColorCounterTerrorists))
                    {
                        string[] parts = _config.Modules.ColoredSmokeGrenades.ColorCounterTerrorists.Split(' ');
                        if (parts.Length == 3)
                        {
                            _ = int.TryParse(parts[0], out r);
                            _ = int.TryParse(parts[1], out g);
                            _ = int.TryParse(parts[2], out b);
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(_config.Modules.ColoredSmokeGrenades.ColorOther))
                    {
                        string[] parts = _config.Modules.ColoredSmokeGrenades.ColorOther.Split(' ');
                        if (parts.Length == 3)
                        {
                            _ = int.TryParse(parts[0], out r);
                            _ = int.TryParse(parts[1], out g);
                            _ = int.TryParse(parts[2], out b);
                        }
                    }
                }
                grenade.SmokeColor.X = r;
                grenade.SmokeColor.Y = g;
                grenade.SmokeColor.Z = b;
            });
        }
    }
}
