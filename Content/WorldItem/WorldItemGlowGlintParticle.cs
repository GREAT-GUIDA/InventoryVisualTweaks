using System;
using GuidaSharedCode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace InventoryVisualTweaks.Content.WorldItem {
    public class WorldItemGlowGlintParticle : Particle {
        private Color _drawColor;

        public override Texture2D Texture => ModAsset.TexItemGlowingStar.Value;

        public override void SetDefaults() {
            drawLayer = ParticleLayer.BeforeRain;
            useLighting = false;
            cutOffscreen = true;
            width = 16;
            height = 16;
            base.SetDefaults();
        }

        public void Configure(Color trailColor, float intensityAlpha, ItemGlowStyle style) {
            maxTimeLeft = Main.rand.Next(
                WorldItemVisualTuning.ItemGlowGlintLifetimeMin,
                WorldItemVisualTuning.ItemGlowGlintLifetimeMax + 1);
            timeLeft = maxTimeLeft;

            bool fancy = style == ItemGlowStyle.Fancy;
            float sizeScale = fancy ? 1f : WorldItemVisualTuning.ItemGlowStyleNormalGlintSizeScale;
            float size = Main.rand.NextFloat(
                WorldItemVisualTuning.ItemGlowGlintScaleMin,
                WorldItemVisualTuning.ItemGlowGlintScaleMax) * sizeScale;
            scale = size / Texture.Width;

            velocity = Vector2.Zero;

            float alphaScale = fancy ? 1f : WorldItemVisualTuning.ItemGlowStyleNormalGlintAlphaScale;
            float intensity = intensityAlpha * WorldItemVisualTuning.ItemGlowGlintAlpha * alphaScale;
            _drawColor = (WorldItemEffectColorHelper.ForGlowSparkCore(trailColor) with { A = 0 }) * intensity;
        }

        public override void AI() {
            oldPosition = position;
            velocity = Vector2.Zero;
            timeLeft--;
            if (timeLeft <= 0)
                Kill();

            int age = maxTimeLeft - timeLeft;
            float progress = age / (float)maxTimeLeft;
            float flicker = MathF.Sin(progress * MathF.PI);
            alpha = flicker;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            if (alpha <= 0.01f)
                return false;

            spriteBatch.EndAndBeginImmediate(BlendState.AlphaBlend);
            spriteBatch.DrawCentered(Texture, GetDrawPosition(), _drawColor * alpha, scale);
            spriteBatch.EndAndBeginWorld();
            return false;
        }
    }
}
