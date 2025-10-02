using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;
using Microsoft.Extensions.Localization;

namespace Cosmetics.Classes
{
    public class DeathBeam : ParentModule
    {
        public override List<string> Events =>
        [
            "EventBulletImpact",
            "EventPlayerDeath"
        ];
        public override List<string> Listeners => [];
        private readonly List<string> _ignoredWeapons =
        [
            "hegrenade",
            "molotov",
            "decoy",
            "flashbang",
            "incgrenade",
            "tagrenade",
            "taser",
            "smokegrenade",
            "breachcharge",
            "bumpmine",
            "shield",
            "melee",
            "knife",
            "knife_t",
            "knife_ct",
            "env_fire",
            "c4",
            "bomb",
            "tripwirefire",
        ];
        private readonly Dictionary<CCSPlayerController, Vector> _lastBulletImpact = [];

        public DeathBeam(PluginConfig Config, IStringLocalizer Localizer) : base(Config, Localizer)
        {
            Console.WriteLine("[Cosmetics] Initializing DeathBeam module...");
        }

        public override void Destroy()
        {
            Console.WriteLine("[Cosmetics] Destroying DeathBeam module...");
        }

        public HookResult EventBulletImpact(EventBulletImpact @event, GameEventInfo info)
        {
            CCSPlayerController attacker = @event.Userid!;
            if (attacker == null)
            {
                return HookResult.Continue;
            }

            _lastBulletImpact[attacker] = new Vector(@event.X, @event.Y, @event.Z);
            return HookResult.Continue;
        }

        public HookResult EventPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
        {
            CCSPlayerController attacker = @event.Attacker!;
            CCSPlayerController victim = @event.Userid!;
            if (attacker == null || victim == null
                || attacker.Pawn == null || !attacker.Pawn.IsValid || attacker.Pawn.Value == null || attacker.Pawn.Value.CameraServices == null
                || victim.Pawn == null || !victim.Pawn.IsValid || victim.Pawn.Value == null || victim.Pawn.Value.CameraServices == null
                || attacker.Index == victim.Index
                || _ignoredWeapons.Any(@event.Weapon.Contains))
            {
                return HookResult.Continue;
            }
            // create beam from attackers eye position
            float playerViewOffsetZ = attacker.Pawn.Value.CameraServices.OldPlayerViewOffsetZ == 0 ? (attacker.Buttons & PlayerButtons.Duck) != 0 ? 40 : 60 : attacker.Pawn.Value.CameraServices.OldPlayerViewOffsetZ - 5;
            Vector attackerEyePos = attacker.Pawn.Value.AbsOrigin! + new Vector(0, 0, playerViewOffsetZ);
            // set end of beam to last bullet impact position if available
            Vector victimHitVector = _lastBulletImpact.TryGetValue(attacker, out Vector? value) ? value : victim.Pawn.Value.AbsOrigin! + new Vector(0, 0, 40);
            CreateBeam(attackerEyePos, victimHitVector, attacker.Team, _config.Modules.DeathBeam.Width, _config.Modules.DeathBeam.Timeout);
            return HookResult.Continue;
        }

        private void CreateBeam(Vector startOrigin, Vector endOrigin, CsTeam team = CsTeam.None, float width = 1f, float timeout = 2f)
        {
            CEnvBeam beam = Utilities.CreateEntityByName<CEnvBeam>("env_beam")!;
            beam.Width = width;
            Color color = Color.White;
            if (team == CsTeam.CounterTerrorist && !string.IsNullOrEmpty(_config.Modules.DeathBeam.ColorCounterTerrorists))
            {
                int r = 0, g = 0, b = 0;
                string[] parts = _config.Modules.DeathBeam.ColorCounterTerrorists.Split(' ');
                if (parts.Length == 3)
                {
                    _ = int.TryParse(parts[0], out r);
                    _ = int.TryParse(parts[1], out g);
                    _ = int.TryParse(parts[2], out b);
                }
                color = Color.FromArgb(r, g, b);
            }
            else if (team == CsTeam.Terrorist && !string.IsNullOrEmpty(_config.Modules.DeathBeam.ColorTerrorists))
            {
                int r = 0, g = 0, b = 0;
                string[] parts = _config.Modules.DeathBeam.ColorTerrorists.Split(' ');
                if (parts.Length == 3)
                {
                    _ = int.TryParse(parts[0], out r);
                    _ = int.TryParse(parts[1], out g);
                    _ = int.TryParse(parts[2], out b);
                }
                color = Color.FromArgb(r, g, b);
            }
            else if (!string.IsNullOrEmpty(_config.Modules.DeathBeam.ColorOther))
            {
                int r = 0, g = 0, b = 0;
                string[] parts = _config.Modules.DeathBeam.ColorOther.Split(' ');
                if (parts.Length == 3)
                {
                    _ = int.TryParse(parts[0], out r);
                    _ = int.TryParse(parts[1], out g);
                    _ = int.TryParse(parts[2], out b);
                }
                color = Color.FromArgb(r, g, b);
            }
            beam.Render = color;
            beam.SetModel("materials/sprites/laserbeam.vtex");
            beam.Teleport(startOrigin);
            beam.EndPos.X = endOrigin.X;
            beam.EndPos.Y = endOrigin.Y;
            beam.EndPos.Z = endOrigin.Z;
            Utilities.SetStateChanged(beam, "CBeam", "m_vecEndPos");
            // spawn timer to remove beam
            _ = new CounterStrikeSharp.API.Modules.Timers.Timer(timeout, () =>
            {
                if (beam != null && beam.IsValid)
                {
                    beam.Remove();
                }
            });
        }
    }
}
