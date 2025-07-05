using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;

namespace Cosmetics
{
    public partial class Cosmetics : BasePlugin
    {
        private DeathTombstoneConfig? deathTombstoneConfig;

        private void InitializeDeathTombstone()
        {
            if (Config == null || Server.MapName == null)
            {
                return;
            }

            var mapName = Server.MapName.ToLower(System.Globalization.CultureInfo.CurrentCulture);

            // Use map-specific config if it exists, otherwise use global
            if (Config.MapConfigs.TryGetValue(mapName, out var mapConfig) && mapConfig.DeathTombstone != null)
            {
                deathTombstoneConfig = mapConfig.DeathTombstone;
            }
            else
            {
                deathTombstoneConfig = Config.Global.DeathTombstone;
            }

            // Disable if globally or map specifically disabled
            if (deathTombstoneConfig == null || !deathTombstoneConfig.Enable)
            {
                return;
            }

            RegisterEventHandler<EventPlayerDeath>(DeathTombstoneOnPlayerDeath);
        }

        private void ResetDeathTombstone()
        {
            // unregister event handlers
            DeregisterEventHandler<EventPlayerDeath>(DeathTombstoneOnPlayerDeath);
        }

        private HookResult DeathTombstoneOnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
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
                || !prop.IsValid
                || deathTombstoneConfig == null)
            {
                return;
            }
            // set attributes
            prop.MoveType = MoveType_t.MOVETYPE_VPHYSICS;
            prop.Collision.SolidType = SolidType_t.SOLID_VPHYSICS;
            prop.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DEFAULT;
            prop.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DEFAULT;
            prop.DispatchSpawn();
            prop.SetModel(deathTombstoneConfig.Model);
            prop.CBodyComponent!.SceneNode!.Scale = deathTombstoneConfig.Size;
            // give prop some health
            if (deathTombstoneConfig.Health > 0)
            {
                prop.MaxHealth = deathTombstoneConfig.Health;
                prop.Health = deathTombstoneConfig.Health;
                Utilities.SetStateChanged(prop, "CBaseEntity", "m_iHealth");
                Utilities.SetStateChanged(prop, "CBaseEntity", "m_iMaxHealth");
                prop.TakesDamage = true;
                prop.TakeDamageFlags = TakeDamageFlags_t.DFLAG_ALWAYS_FIRE_DAMAGE_EVENTS;
            }
            // teleport prop to position
            prop.Teleport(vector, angle);
        }
    }
}
