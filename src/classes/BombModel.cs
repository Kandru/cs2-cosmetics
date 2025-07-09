using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace Cosmetics.Classes
{
    public class BombModel : ParentModule
    {
        public override List<string> Events => [
            "EventBombPlanted",
            "EventBombPickup"
        ];

        public BombModel(PluginConfig Config) : base(Config)
        {
            Console.WriteLine("[Cosmetics] Initializing BombModel module...");
        }

        public new void Destroy()
        {
            Console.WriteLine("[Cosmetics] Destroying BombModel module...");
        }

        public HookResult EventBombPlanted(EventBombPlanted @event, GameEventInfo info)
        {
            // delay one frame to allow c4 model to exist
            Server.NextFrame(() => ChangeBombScale("planted_c4"));
            return HookResult.Continue;
        }

        public HookResult EventBombPickup(EventBombPickup @event, GameEventInfo info)
        {
            // delay one frame to allow c4 model to exist
            Server.NextFrame(() => ChangeBombScale("weapon_c4"));
            return HookResult.Continue;
        }

        private void ChangeBombScale(string entityName)
        {
            // find all planted c4 entities
            IEnumerable<CBaseEntity> bombEntities = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>(entityName);
            foreach (CBaseEntity bombEntity in bombEntities)
            {
                float randomScale = (float)((new Random().NextDouble() *
                    (_config.Modules.BombModel.MaxSize - _config.Modules.BombModel.MinSize)) + _config.Modules.BombModel.MinSize);
                bombEntity.CBodyComponent!.SceneNode!.GetSkeletonInstance().Scale = randomScale;
            }
        }
    }
}
