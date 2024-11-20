using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace Cosmetics
{
    public partial class Cosmetics : BasePlugin
    {
        private Dictionary<CCSPlayerController, int> _spectatorModelPlayers = new();
        private readonly string _spectatorModel = "models/vehicles/airplane_medium_01/airplane_medium_01_landed.vmdl";

        private void InitializeSpectatorModel()
        {
            if (!Config.EnableSpectatorModel) return;
            if (_currentMap == "") return;
            var mapConfig = Config.MapConfigs[_currentMap];
            if (!mapConfig.EnableSpectatorModel) return;
            RegisterListener<Listeners.OnTick>(EventSpectatorModelOnTick);
        }

        private void ResetSpectatorModel()
        {
            RemoveListener<Listeners.OnTick>(EventSpectatorModelOnTick);
            RemoveListener<Listeners.CheckTransmit>(EventSpectatorModelCheckTransmit);
        }
        private void EventSpectatorModelOnTick()
        {
            foreach (CCSPlayerController player in Utilities.GetPlayers())
            {
                try
                {
                    // sanity checks
                    if (player == null
                    || player.Pawn == null
                    || player.Pawn.Value == null
                    || !player.Pawn.IsValid) continue;
                    if (!_spectatorModelPlayers.ContainsKey(player) && player.Pawn.Value.LifeState == (byte)LifeState_t.LIFE_DEAD
                         && player.Pawn.Value.ObserverServices != null
                         && player.Pawn.Value.ObserverServices.ObserverMode == (byte)ObserverMode_t.OBS_MODE_ROAMING)
                    {
                        Console.WriteLine("Spawning spectator model for player: " + player.PlayerName);
                        if (_spectatorModelPlayers.Count == 0) RegisterListener<Listeners.CheckTransmit>(EventSpectatorModelCheckTransmit);
                        _spectatorModelPlayers.Add(player,
                            SpawnProp(
                                player,
                                _spectatorModel,
                                0.01f
                        ));
                    }
                    else if (_spectatorModelPlayers.ContainsKey(player)
                        && (player.Pawn.Value.LifeState != (byte)LifeState_t.LIFE_DEAD
                        || (player.Pawn.Value.ObserverServices != null && player.Pawn.Value.ObserverServices.ObserverMode != (byte)ObserverMode_t.OBS_MODE_ROAMING)))
                    {
                        Console.WriteLine("Removing spectator model for player: " + player.PlayerName);
                        RemoveProp(
                            _spectatorModelPlayers[player],
                            true
                        );
                        _spectatorModelPlayers.Remove(player);
                    }
                    else if (_spectatorModelPlayers.ContainsKey(player) && player.Pawn.Value.LifeState == (byte)LifeState_t.LIFE_DEAD)
                    {
                        UpdateProp(
                            player,
                            _spectatorModelPlayers[player],
                            0,
                            0
                        );
                    }
                }
                catch (Exception e)
                {
                    // log error
                    Console.WriteLine(Localizer["core.error"].Value.Replace("{error}", e.Message));
                }
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
    }
}
