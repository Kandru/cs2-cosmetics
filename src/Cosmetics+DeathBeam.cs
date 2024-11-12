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
            "knife",
            "knife_t",
            "knife_ct",
            "env_fire",
        };

        private void InitializeDeathBeams()
        {
            RegisterEventHandler<EventPlayerDeath>(DeathBeamsOnPlayerDeath);
        }

        private HookResult DeathBeamsOnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
        {
            CCSPlayerController attacker = @event.Attacker!;
            CCSPlayerController victim = @event.Userid!;
            if (attacker == null || victim == null
                || attacker.Pawn == null || !attacker.Pawn.IsValid || attacker.Pawn.Value == null
                || victim.Pawn == null || !victim.Pawn.IsValid || victim.Pawn.Value == null
                || _deathbeamIgnoreWeapons.Contains(@event.Weapon)) return HookResult.Continue;

            Vector attackerEyePos = attacker.Pawn.Value.AbsOrigin! + new Vector(0, 0, 40);
            Vector victimEyePos = victim.Pawn.Value.AbsOrigin! + new Vector(0, 0, 40);
            CreateBeam(attackerEyePos, victimEyePos, attacker.Team, 0.5f, 1f);
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
