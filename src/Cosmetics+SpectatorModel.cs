using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Cosmetics
{
    public partial class Cosmetics : BasePlugin
    {
        private Dictionary<CCSPlayerController, int> _spectatorModelPlayers = new();
        private readonly string _spectatorModel = "models/vehicles/airplane_medium_01/airplane_medium_01_landed.vmdl";

        private void InitializeSpectatorModel()
        {
            if (!Config.EnableSpectatorModel) return;
            if (_currentMap == "" || !Config.MapConfigs.ContainsKey(_currentMap)) return;
            var mapConfig = Config.MapConfigs[_currentMap];
            if (!mapConfig.EnableSpectatorModel) return;
            RegisterEventHandler<EventPlayerTeam>(EventSpectatorModelOnPlayerTeam);
            RegisterEventHandler<EventPlayerDisconnect>(EventSpectatorModelOnPlayerDisconnect);
        }

        private void ResetSpectatorModel()
        {
            RemoveListener<Listeners.OnTick>(EventSpectatorModelOnTick);
            RemoveListener<Listeners.CheckTransmit>(EventSpectatorModelCheckTransmit);
            DeregisterEventHandler<EventPlayerTeam>(EventSpectatorModelOnPlayerTeam);
            DeregisterEventHandler<EventPlayerDisconnect>(EventSpectatorModelOnPlayerDisconnect);
            _spectatorModelPlayers.Clear();
        }
        private void EventSpectatorModelOnTick()
        {
            bool hasSpectators = false;
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
                    ) continue;
                    // initial spawn
                    if (!_spectatorModelPlayers.ContainsKey(player) && player.Pawn.Value.LifeState == (byte)LifeState_t.LIFE_DEAD)
                    {
                        hasSpectators = true;
                        // continue if player is currently not in correct roaming mode
                        if (player.Pawn.Value.ObserverServices == null
                            || player.Pawn.Value.ObserverServices.ObserverMode != (byte)ObserverMode_t.OBS_MODE_ROAMING) continue;
                        // start transmission if nobody else has yet
                        if (_spectatorModelPlayers.Count == 0) RegisterListener<Listeners.CheckTransmit>(EventSpectatorModelCheckTransmit);
                        // spawn prop
                        _spectatorModelPlayers.Add(player,
                            SpawnProp(
                                player,
                                _spectatorModel,
                                0.02f
                        ));
                    }
                    else if (_spectatorModelPlayers.ContainsKey(player)
                        && (player.Pawn.Value.LifeState != (byte)LifeState_t.LIFE_DEAD
                        || (player.Pawn.Value.ObserverServices != null && player.Pawn.Value.ObserverServices.ObserverMode != (byte)ObserverMode_t.OBS_MODE_ROAMING)))
                    {
                        hasSpectators = true;
                        RemoveProp(
                            _spectatorModelPlayers[player],
                            true
                        );
                        _spectatorModelPlayers.Remove(player);
                    }
                    else if (_spectatorModelPlayers.ContainsKey(player) && player.Pawn.Value.LifeState == (byte)LifeState_t.LIFE_DEAD)
                    {
                        if (!UpdateProp(
                            player,
                            _spectatorModelPlayers[player],
                            -1,
                            0
                        ))
                        {
                            hasSpectators = true;
                            _spectatorModelPlayers.Remove(player);
                        }
                    }
                }
                catch (Exception e)
                {
                    // log error
                    Console.WriteLine(Localizer["core.error"].Value.Replace("{error}", e.Message));
                }
            }
            if (_spectatorModelPlayers.Count() == 0 && !hasSpectators)
            {
                RemoveListener<Listeners.OnTick>(EventSpectatorModelOnTick);
            }
        }

        private void EventSpectatorModelCheckTransmit(CCheckTransmitInfoList infoList)
        {
            // remove listener if no players to save resources
            if (_spectatorModelPlayers.Count() == 0)
            {
                RemoveListener<Listeners.CheckTransmit>(EventSpectatorModelCheckTransmit);
                return;
            }
            // worker
            foreach ((CCheckTransmitInfo info, CCSPlayerController? player) in infoList)
            {
                if (player == null) continue;
                if (!_spectatorModelPlayers.ContainsKey(player)) continue;
                var prop = Utilities.GetEntityFromIndex<CDynamicProp>(_spectatorModelPlayers[player]);
                if (prop == null) continue;
                info.TransmitEntities.Remove(prop);
            }
        }

        private HookResult EventSpectatorModelOnPlayerTeam(EventPlayerTeam @event, GameEventInfo info)
        {
            if (@event.Team == (byte)CsTeam.Spectator)
            {
                // start listener if the first player joined spectator team
                if (_spectatorModelPlayers.Count() == 0)
                {
                    RegisterListener<Listeners.OnTick>(EventSpectatorModelOnTick);
                }
            }
            return HookResult.Continue;
        }

        private HookResult EventSpectatorModelOnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
        {
            CCSPlayerController player = @event.Userid!;
            if (_spectatorModelPlayers.ContainsKey(player))
            {
                RemoveProp(
                    _spectatorModelPlayers[player],
                    true
                );
                _spectatorModelPlayers.Remove(player);
            }
            return HookResult.Continue;
        }
    }
}
