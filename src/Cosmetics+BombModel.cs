using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace Cosmetics
{
    public partial class Cosmetics : BasePlugin
    {
        private void InitializeBombModel()
        {
            if (!Config.EnableBombModel) return;
            if (_currentMap == "" || !Config.MapConfigs.ContainsKey(_currentMap)) return;
            var mapConfig = Config.MapConfigs[_currentMap];
            if (!mapConfig.EnableBombModel) return;
            RegisterEventHandler<EventBombPlanted>(BombModelOnBombPlanted);
        }

        private void ResetBombModel()
        {
            DeregisterEventHandler<EventBombPlanted>(BombModelOnBombPlanted);
        }

        private HookResult BombModelOnBombPlanted(EventBombPlanted @event, GameEventInfo info)
        {
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
