using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using System.Drawing;

namespace Cosmetics.Classes
{
    public partial class SpectatorModel : ParentModule
    {
        public override List<string> Events =>
        [
            "EventPlayerTeam",
            "EventPlayerDisconnect",
            "EventRoundStart"
        ];
        public override List<string> Listeners =>
        [
            "CheckTransmit",
            "OnTick",
            "OnMapEnd"
        ];
        public override List<string> Precache =>
        [
            _config.Modules.SpectatorModel.Model
        ];
        private readonly Dictionary<CCSPlayerController, CDynamicProp> _spectators = [];

        public SpectatorModel(PluginConfig Config) : base(Config)
        {
            Console.WriteLine("[Cosmetics] Initializing SpectatorModel module...");
            // get spectators and spawn props for them
            foreach (CCSPlayerController player in Utilities.GetPlayers().Where(static p => p.Team == CsTeam.Spectator))
            {
                if (player.Team == CsTeam.Spectator && !_spectators.ContainsKey(player))
                {
                    CDynamicProp? prop = SpawnProp(
                        player,
                        _config.Modules.SpectatorModel.Model,
                        _config.Modules.SpectatorModel.Size
                    );
                    if (prop == null
                        || !prop.IsValid)
                    {
                        continue;
                    }
                    _spectators.Add(player, prop);
                }
            }
        }

        public override void Destroy()
        {
            Console.WriteLine("[Cosmetics] Destroying SpectatorModel module...");
            foreach (CDynamicProp prop in _spectators.Values)
            {
                RemoveProp(prop, false);
            }
            _spectators.Clear();
        }

        public void OnTick()
        {
            foreach (KeyValuePair<CCSPlayerController, CDynamicProp> kvp in _spectators.ToList())
            {
                // sanity checks
                if (!kvp.Key.IsValid
                    || kvp.Key.Pawn?.Value?.IsValid != true
                    || kvp.Key.Team != CsTeam.Spectator
                    || kvp.Key.Pawn.Value.ObserverServices == null
                    || !kvp.Value.IsValid)
                {
                    RemoveProp(kvp.Value, false);
                    _ = _spectators.Remove(kvp.Key);
                    continue;
                }
                if (kvp.Key.Pawn.Value.ObserverServices.ObserverMode != (byte)ObserverMode_t.OBS_MODE_ROAMING)
                {
                    if (kvp.Value.AbsOrigin!.X != -999 && kvp.Value.AbsOrigin!.Y != -999 && kvp.Value.AbsOrigin!.Z != -999)
                    {
                        RemoveProp(kvp.Value, true);
                    }
                }
                else if (!UpdateProp(kvp.Key, kvp.Value, _config.Modules.SpectatorModel.OffsetZ, _config.Modules.SpectatorModel.OffsetAngle))
                {
                    RemoveProp(kvp.Value, false);
                    _ = _spectators.Remove(kvp.Key);
                }
            }
        }

        public void CheckTransmit(CCheckTransmitInfoList infoList)
        {
            if (_spectators.Count == 0)
            {
                return;
            }
            // remove prop for the player itself
            foreach ((CCheckTransmitInfo info, CCSPlayerController? player) in infoList)
            {
                if (player == null
                    || !_spectators.TryGetValue(player, out CDynamicProp? prop)
                    || prop == null
                    || !prop.IsValid)
                {
                    continue;
                }
                info.TransmitEntities.Remove(prop);
            }
        }

        public HookResult EventPlayerTeam(EventPlayerTeam @event, GameEventInfo info)
        {
            CCSPlayerController? player = @event.Userid;
            if (player == null
                || !player.IsValid)
            {
                return HookResult.Continue;
            }
            // create prop for spectator
            if (@event.Team == (byte)CsTeam.Spectator && !_spectators.ContainsKey(player))
            {
                CDynamicProp? prop = SpawnProp(
                    player,
                    _config.Modules.SpectatorModel.Model,
                    _config.Modules.SpectatorModel.Size
                );
                if (prop != null
                    && prop.IsValid)
                {
                    _spectators.Add(player, prop);
                }
            }
            else if (@event.Team != (byte)CsTeam.Spectator && _spectators.TryGetValue(player, out _))
            {
                RemoveProp(_spectators[player], false);
                _ = _spectators.Remove(player);
            }
            return HookResult.Continue;
        }

        public HookResult EventPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
        {
            CCSPlayerController? player = @event.Userid;
            if (player == null
                || !player.IsValid)
            {
                return HookResult.Continue;
            }

            if (_spectators.TryGetValue(player, out _))
            {
                RemoveProp(_spectators[player], false);
                _ = _spectators.Remove(player);
            }
            return HookResult.Continue;
        }

        public HookResult EventRoundStart(EventRoundStart @event, GameEventInfo info)
        {
            // reset on round start
            _spectators.Clear();
            // get spectators and spawn props for them
            foreach (CCSPlayerController player in Utilities.GetPlayers().Where(static p => p.Team == CsTeam.Spectator))
            {
                if (player.Team == CsTeam.Spectator && !_spectators.ContainsKey(player))
                {
                    CDynamicProp? prop = SpawnProp(
                        player,
                        _config.Modules.SpectatorModel.Model,
                        _config.Modules.SpectatorModel.Size
                    );
                    if (prop == null
                        || !prop.IsValid)
                    {
                        continue;
                    }
                    _spectators.Add(player, prop);
                }
            }
            return HookResult.Continue;
        }

        private CDynamicProp? SpawnProp(CCSPlayerController player, string model, float scale = 1.0f)
        {
            // sanity checks
            if (player == null
            || player.PlayerPawn == null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value == null)
            {
                return null;
            }
            // create dynamic prop
            CDynamicProp prop;
            prop = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic_override")!;
            // set attributes
            prop.MoveType = MoveType_t.MOVETYPE_NOCLIP;
            prop.Collision.SolidType = SolidType_t.SOLID_NONE;
            prop.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_NONE;
            prop.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_NONE;
            // spawn it
            prop.DispatchSpawn();
            prop.SetModel(_config.Modules.SpectatorModel.Model);
            prop.Teleport(new Vector(-999, -999, -999));
            // set random color
            prop.Render = Color.FromArgb(
                255,
                (byte)_random.Next(0, 255),
                (byte)_random.Next(0, 255),
                (byte)_random.Next(0, 255)
            );
            prop.AnimGraphUpdateEnabled = false;
            prop.CBodyComponent!.SceneNode!.Scale = scale;
            return prop;
        }

        private static bool UpdateProp(CCSPlayerController player, CDynamicProp prop, float offset_z = 0, float offset_angle = 0)
        {
            // sanity checks
            if (prop == null
                || !prop.IsValid
                || prop.AbsRotation == null
                || player == null
                || player.Pawn == null
                || player.Pawn.Value == null
                || !player.Pawn.IsValid)
            {
                return false;
            }
            // get player pawn
            CBasePlayerPawn playerPawn = player!.Pawn!.Value;
            // calculate tilt based on player's movement and angle
            float tiltSpeed = 1.0f; // Speed at which tilt angle is adjusted
            float maxTiltAngle = 55.0f; // Maximum tilt angle in degrees
            float maxSpeedForFullTilt = 2.0f; // Speed at which full tilt is achieved
            float tiltAngle;
            float targetTiltAngle;
            float currentTiltAngle = prop.AbsRotation.Z;
            // Calculate the difference in angle
            float angleDifference = playerPawn.V_angle.Y - playerPawn.V_anglePrevious.Y;
            // Determine target tilt angle based on player's look direction and speed
            if (Math.Abs(angleDifference) > 0.1f) // Check if player is looking left or right
            {
                float speedFactor = Math.Clamp(Math.Abs(angleDifference) / maxSpeedForFullTilt, 0.0f, 1.0f);
                targetTiltAngle = maxTiltAngle * speedFactor * -Math.Sign(angleDifference);
            }
            else
            {
                targetTiltAngle = 0; // Player is not moving, reset tilt angle
            }
            // Smoothly adjust tilt angle to target tilt angle
            if (currentTiltAngle < targetTiltAngle)
            {
                tiltAngle = Math.Min(currentTiltAngle + tiltSpeed, targetTiltAngle); // Adjust tilt angle positively
            }
            else if (currentTiltAngle > targetTiltAngle)
            {
                tiltAngle = Math.Max(currentTiltAngle - tiltSpeed, targetTiltAngle); // Adjust tilt angle negatively
            }
            else
            {
                tiltAngle = currentTiltAngle; // No change needed
            }
            // build vectors
            Vector playerOrigin = new(
                (float)Math.Round(playerPawn.AbsOrigin!.X, 5),
                (float)Math.Round(playerPawn.AbsOrigin!.Y, 5),
                (float)Math.Round(playerPawn.AbsOrigin!.Z, 5) + offset_z
            );
            Vector propOrigin = new(
                (float)Math.Round(prop.AbsOrigin!.X, 5),
                (float)Math.Round(prop.AbsOrigin!.Y, 5),
                (float)Math.Round(prop.AbsOrigin!.Z, 5)
            );
            QAngle playerRotation = new(
                (float)Math.Round(playerPawn.V_angle!.X, 5),
                (float)Math.Round(playerPawn.V_angle!.Y, 5) + offset_angle,
                tiltAngle
            );
            QAngle propRotation = new(
                0,
                (float)Math.Round(prop.AbsRotation!.Y, 5),
                0
            );
            // check if vectors changed enough to avoid frequent updates
            if (playerOrigin.X == propOrigin.X
                && playerOrigin.Y == propOrigin.Y
                && playerOrigin.Z == propOrigin.Z
                && playerRotation.Y == propRotation.Y)
            {
                return true;
            }

            prop.Teleport(playerOrigin, playerRotation);
            return true;
        }

        private static void RemoveProp(CDynamicProp? prop, bool softRemove = false)
        {
            if (prop == null
                || !prop.IsValid)
            {
                return;
            }

            if (softRemove)
            {
                prop.Teleport(new Vector(-999, -999, -999));
            }
            else
            {
                prop.Remove();
            }
        }
    }
}
