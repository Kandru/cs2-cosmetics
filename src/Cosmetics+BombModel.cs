using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace Cosmetics
{
    public partial class Cosmetics : BasePlugin
    {
        private void InitializeBombModel()
        {
            // disable if globally disabled
            if (!Config.Global.BombModel.Enable) return;
            // disable if map specific disabled
            if (Config.MapConfigs.ContainsKey(Server.MapName.ToLower())
                && !Config.MapConfigs[Server.MapName.ToLower()].BombModel.Enable) return;
            // register event handler
            RegisterEventHandler<EventBombPlanted>(BombModelOnBombPlanted);
        }

        private void ResetBombModel()
        {
            // unregister event handler
            DeregisterEventHandler<EventBombPlanted>(BombModelOnBombPlanted);
        }

        private HookResult BombModelOnBombPlanted(EventBombPlanted @event, GameEventInfo info)
        {
            // delay one frame to allow c4 model to exist
            Server.NextFrame(() =>
            {
                var bombEntities = Utilities.FindAllEntitiesByDesignerName<CPlantedC4>("planted_c4");
                foreach (var bombEntity in bombEntities)
                {
                    float randomScale = 3.0f;
                    if (Config.MapConfigs.ContainsKey(Server.MapName.ToLower()))
                    {
                        var mapConfig = Config.MapConfigs[Server.MapName.ToLower()];
                        randomScale = (float)(new Random().NextDouble() * (mapConfig.BombModel.MaxSize - mapConfig.BombModel.MinSize) + mapConfig.BombModel.MinSize);
                    }
                    else
                    {
                        randomScale = (float)(new Random().NextDouble() * (Config.Global.BombModel.MaxSize - Config.Global.BombModel.MinSize) + Config.Global.BombModel.MinSize);
                    }
                    bombEntity.CBodyComponent!.SceneNode!.GetSkeletonInstance().Scale = randomScale;
                }
            });
            return HookResult.Continue;
        }
    }
}
