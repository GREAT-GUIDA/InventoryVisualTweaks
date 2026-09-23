using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace InventoryVisualTweaks.Content.Inventory {
    /// <summary>
    /// UI inventory draws must not use <see cref="Main.Rasterizer"/> — it switches to clockwise culling when gravity is flipped.
    /// </summary>
    internal static class InventoryUiSpriteBatch {
        public static void Resume(Matrix? transform = null) {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullCounterClockwise,
                null,
                transform ?? Main.UIScaleMatrix);
        }

        public static void BeginPass(BlendState blend, Effect effect, Matrix transform) {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                blend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullCounterClockwise,
                effect,
                transform);
        }
    }
}
