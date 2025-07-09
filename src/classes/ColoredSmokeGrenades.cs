using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;

namespace Cosmetics.Classes
{
    public class ColoredSmokeGrenades : ParentModule
    {
        public override List<string> Listeners =>
        [
            "OnEntitySpawned"
        ];

        public ColoredSmokeGrenades(PluginConfig Config) : base(Config)
        {
            Console.WriteLine("[Cosmetics] Initializing ColoredSmokeGrenades module...");
        }

        public new void Destroy()
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

                if (owner.TeamNum == (byte)CsTeam.Terrorist)
                {
                    Color color = Color.FromArgb(255, _random.Next(30, 256), 0, 0);
                    grenade.SmokeColor.X = color.R;
                    grenade.SmokeColor.Y = color.G;
                    grenade.SmokeColor.Z = color.B;
                }
                else if (owner.TeamNum == (byte)CsTeam.CounterTerrorist)
                {
                    Color color = Color.FromArgb(0, 0, 0, _random.Next(30, 256));
                    grenade.SmokeColor.X = color.R;
                    grenade.SmokeColor.Y = color.G;
                    grenade.SmokeColor.Z = color.B;
                }
                else
                {
                    Color color = Color.FromArgb(0, 0, _random.Next(30, 256), 0);
                    grenade.SmokeColor.X = color.R;
                    grenade.SmokeColor.Y = color.G;
                    grenade.SmokeColor.Z = color.B;
                }
            });
        }
    }
}
