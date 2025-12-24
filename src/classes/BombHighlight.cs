using System.Drawing;
using Cosmetics.Utils;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Localization;

namespace Cosmetics.Classes
{
    public class BombHighlight : ParentModule
    {
        public override List<string> Events => [
            "EventBombPlanted",
            "EventBombDropped",
            "EventBombPickup",
            "EventRoundEnd"
        ];
        private bool _allow_bomb_highlight = false;
        private (CDynamicProp?, CDynamicProp?) _highlightedBomb = (null, null);

        public BombHighlight(PluginConfig Config, IStringLocalizer Localizer) : base(Config, Localizer)
        {
            Console.WriteLine("[Cosmetics] Initializing BombHighlight module...");
        }

        public override void Destroy()
        {
            Console.WriteLine("[Cosmetics] Destroying BombHighlight module...");
            _allow_bomb_highlight = false;
            Glow.RemoveGlow(_highlightedBomb.Item1, _highlightedBomb.Item2);
            _highlightedBomb = (null, null);
        }

        public HookResult EventBombPlanted(EventBombPlanted @event, GameEventInfo info)
        {
            if (!_config.Modules.BombHighlight.HighlightPlantedC4)
            {
                return HookResult.Continue;
            }
            _allow_bomb_highlight = true;
            // delay at least one frame to allow model to exist
            _ = new CounterStrikeSharp.API.Modules.Timers.Timer(_config.Modules.BombHighlight.DelayHighlight,
                () => HighlightBomb("planted_c4"));
            return HookResult.Continue;
        }

        public HookResult EventBombDropped(EventBombDropped @event, GameEventInfo info)
        {
            if (!_config.Modules.BombHighlight.HighlightLostC4)
            {
                return HookResult.Continue;
            }
            _allow_bomb_highlight = true;
            // delay at least one frame to allow model to exist
            _ = new CounterStrikeSharp.API.Modules.Timers.Timer(_config.Modules.BombHighlight.DelayHighlight,
                () => HighlightBomb("weapon_c4"));
            return HookResult.Continue;
        }

        public HookResult EventBombPickup(EventBombPickup @event, GameEventInfo info)
        {
            _allow_bomb_highlight = false;
            Glow.RemoveGlow(_highlightedBomb.Item1, _highlightedBomb.Item2);
            _highlightedBomb = (null, null);
            return HookResult.Continue;
        }

        public HookResult EventRoundEnd(EventRoundEnd @event, GameEventInfo info)
        {
            _allow_bomb_highlight = false;
            Glow.RemoveGlow(_highlightedBomb.Item1, _highlightedBomb.Item2);
            _highlightedBomb = (null, null);
            return HookResult.Continue;
        }

        private void HighlightBomb(string entityName)
        {
            // stop highlight if not allowed
            if (!_allow_bomb_highlight)
            {
                return;
            }
            // find all planted c4 entities
            IEnumerable<CBaseEntity> bombEntities = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>(entityName);
            foreach (CBaseEntity bombEntity in bombEntities)
            {
                int r = 255, g = 255, b = 255;
                string[] parts = _config.Modules.DeathBeam.ColorCounterTerrorists.Split(' ');
                if (parts.Length == 3)
                {
                    _ = int.TryParse(parts[0], out r);
                    _ = int.TryParse(parts[1], out g);
                    _ = int.TryParse(parts[2], out b);
                }
                (_highlightedBomb.Item1, _highlightedBomb.Item2) = Glow.Create(bombEntity, Color.FromArgb(r, g, b), "prop_physics");
                return;
            }
        }
    }
}
