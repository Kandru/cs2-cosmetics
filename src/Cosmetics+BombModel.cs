using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace Cosmetics
{
    public partial class Cosmetics : BasePlugin
    {
        private BombModelConfig? bombModelConfig;
        private void InitializeBombModel()
        {
            // disable if globally disabled
            if (!Config.Global.BombModel.Enable)
            {
                return;
            }
            // disable if map specific disabled
            if (Config == null || Server.MapName == null ||
                (Config.MapConfigs.ContainsKey(Server.MapName.ToLower(System.Globalization.CultureInfo.CurrentCulture))
                && !Config.MapConfigs[Server.MapName.ToLower(System.Globalization.CultureInfo.CurrentCulture)].BombModel.Enable))
            {
                return;
            }
            // set map specific configuration
            if (Config.MapConfigs.ContainsKey(Server.MapName.ToLower(System.Globalization.CultureInfo.CurrentCulture)))
            {
                bombModelConfig = Config.MapConfigs[Server.MapName.ToLower(System.Globalization.CultureInfo.CurrentCulture)].BombModel;
            }
            // disable if map specific disabled
            if (bombModelConfig != null && !bombModelConfig.Enable)
            {
                return;
            }
            // register event handler
            if ((bombModelConfig != null && bombModelConfig.ModifyPlantedC4)
                || (bombModelConfig == null && Config.Global.BombModel.ModifyPlantedC4))
            {
                RegisterEventHandler<EventBombPlanted>(BombModelOnBombPlanted);
            }

            if ((bombModelConfig != null && bombModelConfig.ModifyWeaponC4)
                || (bombModelConfig == null && Config.Global.BombModel.ModifyWeaponC4))
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
