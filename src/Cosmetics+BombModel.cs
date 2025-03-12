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
            if (Config.MapConfigs[Server.MapName.ToLower()].BombModel.ModifyPlantedC4)
                RegisterEventHandler<EventBombPlanted>(BombModelOnBombPlanted);
            if (Config.MapConfigs[Server.MapName.ToLower()].BombModel.ModifyWeaponC4)
                RegisterEventHandler<EventBombPickup>(BombModelOnBombPickup);
        }

        private void ResetBombModel()
        {
            // unregister event handler
            DeregisterEventHandler<EventBombPlanted>(BombModelOnBombPlanted);
            DeregisterEventHandler<EventBombPickup>(BombModelOnBombPickup);
        }

        private HookResult BombModelOnBombPlanted(EventBombPlanted @event, GameEventInfo info)
        {
            // delay one frame to allow c4 model to exist
            Server.NextFrame(() =>
            {
                // check for map specific configuration
                BombModelConfig? mapConfig = null;
                if (Config.MapConfigs.ContainsKey(Server.MapName.ToLower()))
                    mapConfig = Config.MapConfigs[Server.MapName.ToLower()].BombModel;
                else
                    mapConfig = Config.Global.BombModel;
                // find all planted c4 entities
                var bombEntities = Utilities.FindAllEntitiesByDesignerName<CPlantedC4>("planted_c4");
                foreach (var bombEntity in bombEntities)
                {
                    float randomScale = (float)(new Random().NextDouble() * (mapConfig.MaxSize - mapConfig.MinSize) + mapConfig.MinSize);
                    bombEntity.CBodyComponent!.SceneNode!.GetSkeletonInstance().Scale = randomScale;
                }
            });
            return HookResult.Continue;
        }

        private HookResult BombModelOnBombPickup(EventBombPickup @event, GameEventInfo info)
        {
            // delay one frame to allow c4 model to exist
            Server.NextFrame(() =>
            {
                // check for map specific configuration
                BombModelConfig? mapConfig = null;
                if (Config.MapConfigs.ContainsKey(Server.MapName.ToLower()))
                    mapConfig = Config.MapConfigs[Server.MapName.ToLower()].BombModel;
                else
                    mapConfig = Config.Global.BombModel;
                // find all planted c4 entities
                var bombEntities = Utilities.FindAllEntitiesByDesignerName<CPlantedC4>("weapon_c4");
                foreach (var bombEntity in bombEntities)
                {
                    float randomScale = (float)(new Random().NextDouble() * (mapConfig.MaxSize - mapConfig.MinSize) + mapConfig.MinSize);
                    bombEntity.CBodyComponent!.SceneNode!.GetSkeletonInstance().Scale = randomScale;
                }
            });
            return HookResult.Continue;
        }
    }
}
