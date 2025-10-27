using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Drawing;

namespace Cosmetics.Utils
{
    public static class Glow
    {
        public static (CDynamicProp?, CDynamicProp?) Create(CBaseEntity entity, Color color, string type = "prop_dynamic")
        {
            CDynamicProp? _glowProxy = Utilities.CreateEntityByName<CDynamicProp>(type);
            CDynamicProp? _glow = Utilities.CreateEntityByName<CDynamicProp>(type);
            if (_glowProxy == null
                || _glow == null)
            {
                return (null, null);
            }

            string modelName = entity.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName;
            // create proxy
            _glowProxy.Spawnflags = 256u;
            _glowProxy.RenderMode = RenderMode_t.kRenderNone;
            _glowProxy.SetModel(modelName);
            _glowProxy.AcceptInput("FollowEntity", entity, _glowProxy, "!activator");
            _glowProxy.DispatchSpawn();
            // create glow
            _glow.SetModel(modelName);
            _glow.AcceptInput("FollowEntity", _glowProxy, _glow, "!activator");
            _glow.DispatchSpawn();
            _glow.Render = Color.FromArgb(255, 255, 255, 255);
            _glow.Glow.GlowColorOverride = color;
            _glow.Spawnflags = 256u;
            _glow.RenderMode = RenderMode_t.kRenderGlow;
            _glow.Glow.GlowRange = 5000;
            _glow.Glow.GlowTeam = -1;
            _glow.Glow.GlowType = 3;
            _glow.Glow.GlowRangeMin = 0;
            return (_glowProxy, _glow);
        }

        public static void RemoveGlow(CBaseEntity? glowProxy, CBaseEntity? glow)
        {
            Entities.RemoveEntity(glowProxy);
            Entities.RemoveEntity(glow);
        }
    }
}