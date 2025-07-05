using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace Cosmetics
{
    public partial class Cosmetics : BasePlugin
    {
        private BombModelConfig? bombModelConfig;

        private void InitializeBombModel()
        {
            if (Config == null || Server.MapName == null)
            {
                return;
            }

            string mapName = Server.MapName.ToLower(System.Globalization.CultureInfo.CurrentCulture);
            bool hasMapConfig = Config.MapConfigs.ContainsKey(mapName);
            BombModelConfig? mapConfig = hasMapConfig ? Config.MapConfigs[mapName].BombModel : null;

            // Merge mapConfig with global as fallback
            bombModelConfig = mapConfig ?? Config.Global.BombModel;

            // Disable if globally or map-specific disabled
            if (!Config.Global.BombModel.Enable || (mapConfig != null && !mapConfig.Enable))
            {
                return;
            }

            // Register event handlers based on config
            if (bombModelConfig.ModifyPlantedC4)
            {
                RegisterEventHandler<EventBombPlanted>(BombModelOnBombPlanted);
            }

            if (bombModelConfig.ModifyWeaponC4)
            {
                RegisterEventHandler<EventBombPickup>(BombModelOnBombPickup);
            }
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
                BombModelConfig? mapConfig = Config.MapConfigs.ContainsKey(Server.MapName.ToLower(System.Globalization.CultureInfo.CurrentCulture))
                    ? Config.MapConfigs[Server.MapName.ToLower(System.Globalization.CultureInfo.CurrentCulture)].BombModel
                    : Config.Global.BombModel;
                // find all planted c4 entities
                IEnumerable<CPlantedC4> bombEntities = Utilities.FindAllEntitiesByDesignerName<CPlantedC4>("planted_c4");
                foreach (CPlantedC4 bombEntity in bombEntities)
                {
                    float randomScale = (float)((new Random().NextDouble() * (mapConfig.MaxSize - mapConfig.MinSize)) + mapConfig.MinSize);
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
                // find all planted c4 entities
                IEnumerable<CBaseEntity> bombEntities = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("weapon_c4");
                foreach (CBaseEntity bombEntity in bombEntities)
                {
                    float randomScale = bombModelConfig != null
                        ? (float)((new Random().NextDouble() * (bombModelConfig.MaxSize - bombModelConfig.MinSize)) + bombModelConfig.MinSize)
                        : (float)((new Random().NextDouble() * (Config.Global.BombModel.MaxSize - Config.Global.BombModel.MinSize)) + Config.Global.BombModel.MinSize);
                    bombEntity.CBodyComponent!.SceneNode!.GetSkeletonInstance().Scale = randomScale;
                }
            });
            return HookResult.Continue;
        }
    }
}
