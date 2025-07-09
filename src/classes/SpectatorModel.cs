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
            "EventPlayerDisconnect"
        ];
        public override List<string> Listeners =>
        [
            "CheckTransmit",
            "OnTick"
        ];
        public override List<string> Precache =>
        [
            "models/vehicles/airplane_medium_01/airplane_medium_01_landed.vmdl"
        ];
        private readonly Dictionary<CCSPlayerController, int> _spectatorModelPlayers = [];
        private readonly string _spectatorModel = "models/vehicles/airplane_medium_01/airplane_medium_01_landed.vmdl";

        public SpectatorModel(PluginConfig Config) : base(Config)
        {
            Console.WriteLine("[Cosmetics] Initializing SpectatorModel module...");
        }

        public new void Destroy()
        {
            Console.WriteLine("[Cosmetics] Destroying SpectatorModel module...");
        }

        public void OnTick()
        {
            //bool hasSpectators = false;
            foreach (CCSPlayerController player in Utilities.GetPlayers())
            {
                try
                {
                    // sanity checks
                    if (player == null
                    || player.Pawn == null
                    || player.Pawn.Value == null
                    || !player.Pawn.IsValid
                    || (player.Team != CsTeam.Spectator && !_spectatorModelPlayers.ContainsKey(player))
                    )
                    {
                        continue;
                    }
                    // initial spawn
                    if (!_spectatorModelPlayers.ContainsKey(player) && player.Pawn.Value.LifeState == (byte)LifeState_t.LIFE_DEAD)
                    {
                        //hasSpectators = true;
                        // continue if player is currently not in correct roaming mode
                        if (player.Pawn.Value.ObserverServices == null
                            || player.Pawn.Value.ObserverServices.ObserverMode != (byte)ObserverMode_t.OBS_MODE_ROAMING)
                        {
                            continue;
                        }
                        // start transmission if nobody else has yet
                        if (_spectatorModelPlayers.Count == 0)
                        {
                            // TODO: optimize loop by not using Utilities.GetPlayers() (performance hit otherwise)
                            return;
                        }
                        // spawn prop
                        _spectatorModelPlayers.Add(player,
                            SpawnProp(
                                player,
                                _spectatorModel,
                                0.03f
                        ));
                    }
                    else if (_spectatorModelPlayers.TryGetValue(player, out int value)
                        && (player.Pawn.Value.LifeState != (byte)LifeState_t.LIFE_DEAD
                        || (player.Pawn.Value.ObserverServices != null && player.Pawn.Value.ObserverServices.ObserverMode != (byte)ObserverMode_t.OBS_MODE_ROAMING)))
                    {
                        //hasSpectators = true;
                        RemoveProp(
                            value,
                            true
                        );
                        _ = _spectatorModelPlayers.Remove(player);
                    }
                    else if (_spectatorModelPlayers.ContainsKey(player) && player.Pawn.Value.LifeState == (byte)LifeState_t.LIFE_DEAD)
                    {
                        if (!UpdateProp(
                            player,
                            _spectatorModelPlayers[player],
                            -10,
                            0
                        ))
                        {
                            //hasSpectators = true;
                            _ = _spectatorModelPlayers.Remove(player);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    // TODO: proper error handling
                }
            }
        }

        public void CheckTransmit(CCheckTransmitInfoList infoList)
        {
            // remove listener if no players to save resources
            if (_spectatorModelPlayers.Count == 0)
            {
                // TODO: proper handling
                return;
            }
            // worker
            foreach ((CCheckTransmitInfo info, CCSPlayerController? player) in infoList)
            {
                if (player == null)
                {
                    continue;
                }

                if (!_spectatorModelPlayers.ContainsKey(player))
                {
                    continue;
                }

                CDynamicProp? prop = Utilities.GetEntityFromIndex<CDynamicProp>(_spectatorModelPlayers[player]);
                if (prop == null)
                {
                    continue;
                }

                info.TransmitEntities.Remove(prop);
            }
        }

        public HookResult EventPlayerTeam(EventPlayerTeam @event, GameEventInfo info)
        {
            if (@event.Team == (byte)CsTeam.Spectator)
            {
                // start listener if the first player joined spectator team
                if (_spectatorModelPlayers.Count == 0)
                {
                    // TODO: optimize loop by not using Utilities.GetPlayers() (performance hit otherwise)
                }
            }
            return HookResult.Continue;
        }

        public HookResult EventPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
        {
            CCSPlayerController player = @event.Userid!;
            if (_spectatorModelPlayers.TryGetValue(player, out int value))
            {
                RemoveProp(
                    value,
                    true
                );
                _ = _spectatorModelPlayers.Remove(player);
            }
            return HookResult.Continue;
        }

        private int SpawnProp(CCSPlayerController player, string model, float scale = 1.0f)
        {
            // sanity checks
            if (player == null
            || player.PlayerPawn == null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value == null)
            {
                return -1;
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
            prop.SetModel(model);
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
            return (int)prop.Index;
        }

        private static bool UpdateProp(CCSPlayerController player, int index, int offset_z = 0, int offset_angle = 0)
        {
            CDynamicProp? prop = Utilities.GetEntityFromIndex<CDynamicProp>(index);
            // sanity checks
            if (prop == null
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

        private static void RemoveProp(int index, bool softRemove = false)
        {
            CDynamicProp? prop = Utilities.GetEntityFromIndex<CDynamicProp>(index);
            // remove plant entity
            if (prop == null)
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
