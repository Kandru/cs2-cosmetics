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
            if (!Config.EnableColoredSmokeGrenades) return;
            if (_currentMap == "") return;
            var mapConfig = Config.MapConfigs[_currentMap];
            if (!mapConfig.EnableColoredSmokeGrenades) return;
            RegisterListener<Listeners.OnEntitySpawned>(ColoredSmokeGrenadesOnEntitySpawned);
        }

        private void ResetColoredSmokeGrenades()
        {
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
