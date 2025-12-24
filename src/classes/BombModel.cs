using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;

namespace Cosmetics.Classes
{
    public class BombModel : ParentModule
    {
        public override List<string> Events => [
            "EventBombPlanted",
            "EventBombPickup",
            "EventBombDropped"
        ];
        private bool _allow_bombmodel_change = false;

        public BombModel(PluginConfig Config, IStringLocalizer Localizer) : base(Config, Localizer)
        {
            Console.WriteLine("[Cosmetics] Initializing BombModel module...");
        }

        public override void Destroy()
        {
            Console.WriteLine("[Cosmetics] Destroying BombModel module...");
            _allow_bombmodel_change = false;
        }

        public HookResult EventBombPlanted(EventBombPlanted @event, GameEventInfo info)
        {
            if (!_config.Modules.BombModel.ChangeSizeOnPlant)
            {
                return HookResult.Continue;
            }
            _allow_bombmodel_change = true;
            // delay one frame to allow c4 model to exist
            Server.NextFrame(() => ChangeBombScale("planted_c4", false));
            return HookResult.Continue;
        }

        public HookResult EventBombPickup(EventBombPickup @event, GameEventInfo info)
        {
            if (!_config.Modules.BombModel.ChangeSizeOnEquip)
            {
                return HookResult.Continue;
            }
            _allow_bombmodel_change = false;
            return HookResult.Continue;
        }

        public HookResult EventBombDropped(EventBombDropped @event, GameEventInfo info)
        {
            if (!_config.Modules.BombModel.ChangeSizeOnEquip)
            {
                return HookResult.Continue;
            }
            _allow_bombmodel_change = true;
            // delay one frame to allow c4 model to exist
            _ = new CounterStrikeSharp.API.Modules.Timers.Timer(_config.Modules.BombModel.DelaySizeChange, () => ChangeBombScale("weapon_c4", true));
            return HookResult.Continue;
        }

        private void ChangeBombScale(string entityName, bool useKeyValues = true)
        {
            // stop model change if not allowed
            if (!_allow_bombmodel_change)
            {
                return;
            }
            _allow_bombmodel_change = false;
            // find all planted c4 entities
            IEnumerable<CBaseEntity> bombEntities = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>(entityName);
            foreach (CBaseEntity bombEntity in bombEntities)
            {
                float randomScale = (float)((new Random().NextDouble() *
                    (_config.Modules.BombModel.MaxSize - _config.Modules.BombModel.MinSize)) + _config.Modules.BombModel.MinSize);
                if (!useKeyValues)
                {
                    bombEntity.CBodyComponent!.SceneNode!.GetSkeletonInstance().Scale = randomScale;
                    bombEntity.CBodyComponent!.SceneNode!.GetSkeletonInstance().AbsScale = randomScale;
                    return;
                }
                CEntityKeyValues kv = new();
                kv.SetFloat("modelscale", randomScale);
                bombEntity.DispatchSpawn(kv);
                bombEntity.Teleport(
                    new Vector(
                        bombEntity.AbsOrigin!.X,
                        bombEntity.AbsOrigin!.Y,
                        bombEntity.AbsOrigin!.Z + 20)
                );
            }
            return;
        }
    }
}
