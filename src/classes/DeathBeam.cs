using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;

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

        public DeathBeam(PluginConfig Config) : base(Config)
        {
            Console.WriteLine("[Cosmetics] Initializing DeathBeam module...");
        }

        public new void Destroy()
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
            Vector attackerEyePos = attacker.Pawn.Value.AbsOrigin! + new Vector(0, 0, attacker.Pawn.Value.CameraServices.OldPlayerViewOffsetZ - 5);
            // set end of beam to last bullet impact position if available
            Vector victimHitVector = _lastBulletImpact.TryGetValue(attacker, out Vector? value) ? value : victim.Pawn.Value.AbsOrigin! + new Vector(0, 0, 40);
            CreateBeam(attackerEyePos, victimHitVector, attacker.Team, 0.5f, 1.5f);
            return HookResult.Continue;
        }

        private static void CreateBeam(Vector startOrigin, Vector endOrigin, CsTeam team = CsTeam.None, float width = 1f, float timeout = 2f)
        {
            CEnvBeam beam = Utilities.CreateEntityByName<CEnvBeam>("env_beam")!;
            beam.Width = width;
            beam.Render = team == CsTeam.CounterTerrorist ? Color.Blue : team == CsTeam.Terrorist ? Color.Red : Color.White;

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
