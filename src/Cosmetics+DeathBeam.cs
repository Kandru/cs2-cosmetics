using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;

namespace Cosmetics
{
    public partial class Cosmetics : BasePlugin
    {
        private List<string> _deathbeamIgnoreWeapons = new List<string> {
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
        };

        private Dictionary<CCSPlayerController, Vector> _deathbeamLastBulletImpact = new();

        private void InitializeDeathBeams()
        {
            if (!Config.EnableDeathBeam) return;
            if (_currentMap == "" || !Config.MapConfigs.ContainsKey(_currentMap)) return;
            var mapConfig = Config.MapConfigs[_currentMap];
            if (!mapConfig.EnableDeathBeam) return;
            RegisterEventHandler<EventPlayerDeath>(DeathBeamsOnPlayerDeath);
            RegisterEventHandler<EventBulletImpact>(DeathBeamsOnBulletImpact);
        }

        private void ResetDeathBeams()
        {
            DeregisterEventHandler<EventPlayerDeath>(DeathBeamsOnPlayerDeath);
            DeregisterEventHandler<EventBulletImpact>(DeathBeamsOnBulletImpact);
        }

        private HookResult DeathBeamsOnBulletImpact(EventBulletImpact @event, GameEventInfo info)
        {
            CCSPlayerController attacker = @event.Userid!;
            if (attacker == null) return HookResult.Continue;
            _deathbeamLastBulletImpact[attacker] = new Vector(@event.X, @event.Y, @event.Z);
            return HookResult.Continue;
        }

        private HookResult DeathBeamsOnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
        {
            CCSPlayerController attacker = @event.Attacker!;
            CCSPlayerController victim = @event.Userid!;
            if (attacker == null || victim == null
                || attacker.Pawn == null || !attacker.Pawn.IsValid || attacker.Pawn.Value == null || attacker.Pawn.Value.CameraServices == null
                || victim.Pawn == null || !victim.Pawn.IsValid || victim.Pawn.Value == null || victim.Pawn.Value.CameraServices == null
                || attacker.Index == victim.Index
                || _deathbeamIgnoreWeapons.Contains(@event.Weapon)) return HookResult.Continue;
            // create beam from attackers eye position
            Vector attackerEyePos = attacker.Pawn.Value.AbsOrigin! + new Vector(0, 0, attacker.Pawn.Value.CameraServices.OldPlayerViewOffsetZ - 5);
            // set end of beam to last bullet impact position if available
            Vector victimHitVector = _deathbeamLastBulletImpact.ContainsKey(attacker) ? _deathbeamLastBulletImpact[attacker] : victim.Pawn.Value.AbsOrigin! + new Vector(0, 0, 40);
            CreateBeam(attackerEyePos, victimHitVector, attacker.Team, 0.5f, 1.5f);
            return HookResult.Continue;
        }

        private void CreateBeam(Vector startOrigin, Vector endOrigin, CsTeam team = CsTeam.None, float width = 1f, float timeout = 2f)
        {
            CEnvBeam beam = Utilities.CreateEntityByName<CEnvBeam>("env_beam")!;
            beam.Width = width;
            if (team == CsTeam.CounterTerrorist) beam.Render = Color.Blue;
            else if (team == CsTeam.Terrorist) beam.Render = Color.Red;
            else beam.Render = Color.White;
            beam.SetModel("materials/sprites/laserbeam.vtex");
            beam.Teleport(startOrigin);
            beam.EndPos.X = endOrigin.X;
            beam.EndPos.Y = endOrigin.Y;
            beam.EndPos.Z = endOrigin.Z;
            Utilities.SetStateChanged(beam, "CBeam", "m_vecEndPos");
            AddTimer(timeout, () =>
            {
                if (beam != null && beam.IsValid) beam.Remove();
            });
        }
    }
}
