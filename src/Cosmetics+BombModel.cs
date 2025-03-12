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
                    bombEntity.CBodyComponent!.SceneNode!.GetSkeletonInstance().Scale = 10f;
                }
            });
            return HookResult.Continue;
        }
    }
}
