using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;

namespace Cosmetics
{
    public partial class Cosmetics : BasePlugin
    {
        private void InitializeColoredSmokeGrenades()
        {
            // disable if globally disabled
            if (!Config.Global.ColoredSmokeGrenades.Enable) return;
            // disable if map specific disabled
            if (Config == null || Server.MapName == null ||
                (Config.MapConfigs.ContainsKey(Server.MapName.ToLower())
                && !Config.MapConfigs[Server.MapName.ToLower()].ColoredSmokeGrenades.Enable)) return;
            // register event handler
            RegisterListener<Listeners.OnEntitySpawned>(ColoredSmokeGrenadesOnEntitySpawned);
        }

        private void ResetColoredSmokeGrenades()
        {
            // unregister event handler
            RemoveListener<Listeners.OnEntitySpawned>(ColoredSmokeGrenadesOnEntitySpawned);
        }

        private void ColoredSmokeGrenadesOnEntitySpawned(CEntityInstance entity)
        {
            if (entity.DesignerName != "smokegrenade_projectile") return;
            nint handle = entity.Handle;
            Server.NextFrame(() =>
            {
                CSmokeGrenadeProjectile grenade = new(handle);
                if (grenade.Handle == IntPtr.Zero) return;
                CBaseEntity? owner = grenade.OwnerEntity?.Value;
                if (owner == null) return;
                if (owner.TeamNum == (byte)CsTeam.Terrorist)
                {
                    var color = Color.FromArgb(255, _random.Next(30, 256), 0, 0);
                    grenade.SmokeColor.X = color.R;
                    grenade.SmokeColor.Y = color.G;
                    grenade.SmokeColor.Z = color.B;
                }
                else if (owner.TeamNum == (byte)CsTeam.CounterTerrorist)
                {
                    var color = Color.FromArgb(0, 0, 0, _random.Next(30, 256));
                    grenade.SmokeColor.X = color.R;
                    grenade.SmokeColor.Y = color.G;
                    grenade.SmokeColor.Z = color.B;
                }
                else
                {
                    var color = Color.FromArgb(0, 0, _random.Next(30, 256), 0);
                    grenade.SmokeColor.X = color.R;
                    grenade.SmokeColor.Y = color.G;
                    grenade.SmokeColor.Z = color.B;
                }
            });
        }
    }
}
