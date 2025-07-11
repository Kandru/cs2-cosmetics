using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;

namespace Cosmetics.Classes
{
    public class DeathTombstone : ParentModule
    {
        public override List<string> Events =>
        [
            "EventPlayerDeath"
        ];
        public override List<string> Precache =>
        [
            _config.Modules.DeathTombstone.Model
        ];

        public DeathTombstone(PluginConfig Config, IStringLocalizer Localizer) : base(Config, Localizer)
        {
            Console.WriteLine("[Cosmetics] Initializing DeathTombstone module...");
        }

        public override void Destroy()
        {
            Console.WriteLine("[Cosmetics] Destroying DeathTombstone module...");
        }

        public HookResult EventPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
        {
            CCSPlayerController victim = @event.Userid!;
            if (victim?.Pawn?.Value?.AbsOrigin == null)
            {
                return HookResult.Continue;
            }
            SpawnTombstone(victim.Pawn.Value.AbsOrigin, new QAngle(0, victim.Pawn.Value.V_angle.Y + 90, 0));
            return HookResult.Continue;
        }

        private void SpawnTombstone(Vector vector, QAngle angle)
        {
            // create pole prop
            CPhysicsProp? prop = Utilities.CreateEntityByName<CPhysicsProp>("prop_physics_override");
            if (prop == null
                || !prop.IsValid)
            {
                return;
            }
            // set attributes
            prop.MoveType = MoveType_t.MOVETYPE_VPHYSICS;
            prop.Collision.SolidType = SolidType_t.SOLID_VPHYSICS;
            prop.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DEFAULT;
            prop.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DEFAULT;
            prop.DispatchSpawn();
            prop.SetModel(_config.Modules.DeathTombstone.Model);
            prop.CBodyComponent!.SceneNode!.Scale = _config.Modules.DeathTombstone.Size;
            // give prop some health
            if (_config.Modules.DeathTombstone.Health > 0)
            {
                prop.MaxHealth = _config.Modules.DeathTombstone.Health;
                prop.Health = _config.Modules.DeathTombstone.Health;
                Utilities.SetStateChanged(prop, "CBaseEntity", "m_iHealth");
                Utilities.SetStateChanged(prop, "CBaseEntity", "m_iMaxHealth");
                prop.TakesDamage = true;
                prop.TakeDamageFlags = TakeDamageFlags_t.DFLAG_ALWAYS_FIRE_DAMAGE_EVENTS;
            }
            // teleport prop to position
            prop.Teleport(vector, angle);
            // spawn timer to remove tombstone
            if (_config.Modules.DeathTombstone.Timeout > 0)
            {
                _ = new CounterStrikeSharp.API.Modules.Timers.Timer(_config.Modules.DeathTombstone.Timeout, () =>
                {
                    if (prop != null && prop.IsValid)
                    {
                        prop.Remove();
                    }
                });
            }
        }
    }
}
