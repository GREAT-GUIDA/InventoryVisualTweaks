using System;
using GuidaSharedCode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace InventoryVisualTweaks.Content.WorldItem {
    public enum GlowSparkVariant {
        Normal,
        Streak
    }

    public class WorldItemGlowSparkParticle : Particle {
        private Color _haloColor;
        private Color _coreColor;
        private Vector2 _haloScale;
        private Vector2 _coreScale;

        public override Texture2D Texture => null;

        public override void SetDefaults() {
            drawLayer = ParticleLayer.BeforeItems;
            useLighting = false;
            cutOffscreen = true;
            width = 12;
            height = 12;
            base.SetDefaults();
        }

        public void Configure(Color trailColor, float glowScale, float intensityAlpha, GlowSparkVariant variant = GlowSparkVariant.Normal) {
            if (variant == GlowSparkVariant.Streak) {
                SetupLifetime(
                    Main.rand.Next(
                        WorldItemVisualTuning.ItemGlowSparkStreakLifetimeMin,
                        WorldItemVisualTuning.ItemGlowSparkStreakLifetimeMax + 1),
                    glowScale,
                    WorldItemVisualTuning.ItemGlowSparkStreakLifetimeScale,
                    WorldItemVisualTuning.ItemGlowSparkStreakLifetimeMinFrames);

                SetupScales(1f, WorldItemVisualTuning.ItemGlowSparkStreakHeightScale);

                velocity = new Vector2(
                    0f,
                    -Main.rand.NextFloat(
                        WorldItemVisualTuning.ItemGlowSparkStreakUpSpeedMin,
                        WorldItemVisualTuning.ItemGlowSparkStreakUpSpeedMax));
            } else {
                SetupLifetime(
                    Main.rand.Next(
                        WorldItemVisualTuning.ItemGlowSparkLifetimeMin,
                        WorldItemVisualTuning.ItemGlowSparkLifetimeMax + 1),
                    glowScale,
                    WorldItemVisualTuning.ItemGlowSparkLifetimeScale,
                    WorldItemVisualTuning.ItemGlowSparkLifetimeMinFrames);

                SetupScales(1f, 1f);

                velocity = new Vector2(
                    Main.rand.NextFloat(-WorldItemVisualTuning.ItemGlowSparkDrift, WorldItemVisualTuning.ItemGlowSparkDrift),
                    -Main.rand.NextFloat(
                        WorldItemVisualTuning.ItemGlowSparkUpSpeedMin,
                        WorldItemVisualTuning.ItemGlowSparkUpSpeedMax));
            }

            SetupColors(trailColor, intensityAlpha);
            alpha = 1f;
        }

        private void SetupLifetime(int baseLifetime, float glowScale, float lifetimeScale, int minFrames) {
            float scaledLifetime = baseLifetime * glowScale * lifetimeScale;
            maxTimeLeft = Math.Max(minFrames, (int)Math.Round(scaledLifetime));
            timeLeft = maxTimeLeft;
        }

        private void SetupScales(float widthScale, float heightScale) {
            float sizeJitter = 1f + Main.rand.NextFloat(
                -WorldItemVisualTuning.ItemGlowSparkSizeJitter,
                WorldItemVisualTuning.ItemGlowSparkSizeJitter);
            float particleDiameter = WorldItemVisualTuning.ItemGlowSparkReferenceGlowRadius * 2f * sizeJitter;
            float haloRadius = particleDiameter * WorldItemVisualTuning.ItemGlowSparkHaloRadiusScale / ModAsset.TexItemSolidBloom.Value.Width;
            float coreRadius = particleDiameter * WorldItemVisualTuning.ItemGlowSparkCoreRadiusScale / ModAsset.PartiGlowPMA.Value.Width;
            _haloScale = new Vector2(haloRadius * widthScale, haloRadius * heightScale);
            _coreScale = new Vector2(coreRadius * widthScale, coreRadius * heightScale);
        }

        private void SetupColors(Color trailColor, float intensityAlpha) {
            _haloColor = WorldItemEffectColorHelper.ForGlow(trailColor, true)
                .WithAlpha(intensityAlpha, WorldItemVisualTuning.ItemGlowSparkHaloAlpha);

            _coreColor = WorldItemEffectColorHelper.ForGlowSparkCore(trailColor)
                .WithAlpha(intensityAlpha);
        }

        public override void AI() {
            base.AI();

            float lifeFade = timeLeft / (float)maxTimeLeft;
            int age = maxTimeLeft - timeLeft;
            float fadeIn = MathHelper.Clamp(
                age / WorldItemVisualTuning.ItemGlowSparkFadeInFrames,
                0f,
                1f);
            alpha = lifeFade * fadeIn;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            if (alpha <= 0.01f)
                return false;

            Vector2 drawPosition = GetDrawPosition();
            spriteBatch.EndAndBeginImmediate(BlendState.Additive);
            spriteBatch.DrawCentered(ModAsset.TexItemSolidBloom.Value, drawPosition, _haloColor.MultiplyAlpha(alpha), _haloScale);
            spriteBatch.DrawCentered(ModAsset.PartiGlowPMA.Value, drawPosition, _coreColor.MultiplyAlpha(alpha), _coreScale);
            spriteBatch.EndAndBeginWorld();
            return false;
        }
    }
}
