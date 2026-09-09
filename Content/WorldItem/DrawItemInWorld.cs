using Microsoft.Xna.Framework;
using GuidaSharedCode;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace InventoryVisualTweaks.Content.WorldItem {
    public class GlobalDrawItemInWorld : GlobalItem {
        public override bool InstancePerEntity => false;
        

        public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
            if (item.IsAir || string.IsNullOrEmpty(item.Name))
                return base.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);

            var config = ModContent.GetInstance<DrawItemInWorldConfig>();
            if (!config.EnableOutline)
                return base.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);

            if (!TryGetItemDrawData(item, rotation, scale, out var texture, out var frame, out var position, out var origin))
                return base.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);

            Color sourceColor = WorldItemEffectColorHelper.PrepareBaseColor(ColorSolver.ResolveUseColor(config.OutlineColorBased, item));
            var offsets = GetOutlineOffsets(rotation, scale);

            if (config.OutLineHighlight) {
                Color darkBase = WorldItemEffectColorHelper.ForOutline(sourceColor, false)
                    .WithRgbScale(WorldItemVisualTuning.HighlightBaseDarkness)
                    .WithAlpha(config.OutlineIntensity / 255f, WorldItemVisualTuning.HighlightBaseAlpha);
                DrawOutlinePass(spriteBatch, texture, frame, position, rotation, scale, origin, offsets, darkBase, darkBase, BlendState.NonPremultiplied);

                Color addColor = WorldItemEffectColorHelper.ForOutline(sourceColor, true)
                    .WithAlpha(config.OutlineIntensity / 255f);
                DrawOutlinePass(spriteBatch, texture, frame, position, rotation, scale, origin, offsets, addColor, addColor, BlendState.Additive);
            } else {
                Color drawColor = WorldItemEffectColorHelper.ForOutline(sourceColor, false)
                    .WithAlpha(config.OutlineIntensity / 255f);
                DrawOutlinePass(spriteBatch, texture, frame, position, rotation, scale, origin, offsets, drawColor, drawColor, BlendState.NonPremultiplied);
            }
            spriteBatch.EndAndBeginDefault();

            return base.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
        }

        public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
            if (item.IsAir || string.IsNullOrEmpty(item.Name))
                return;
            
            DrawItemSpawnFlash(item, spriteBatch, rotation, scale, whoAmI);

            var config = ModContent.GetInstance<DrawItemInWorldConfig>();
            if (config.EnableLuster) {
                if (TryGetItemDrawData(item, rotation, scale, out var texture, out var frame, out var position, out _)) {
                    Color effectColor = WorldItemEffectColorHelper.ForLuster(
                        WorldItemEffectColorHelper.PrepareBaseColor(ColorSolver.ResolveUseColor(config.LusterColorBased, item)));
                    float intensity = config.LusterIntensity / 255f;
                    float lusterWidth = WorldItemVisualTuning.LusterWidth * 2f - 1f;

                    spriteBatch.DrawWithEffect(BlendState.Additive, ModAsset.BorderLuster.Value, Main.GameViewMatrix.TransformationMatrix, effect => {
                        effect
                            .SetTime((float)Main.timeForVisualEffects / 100f * WorldItemVisualTuning.LusterSpeed)
                            .SetFrequency(WorldItemVisualTuning.LusterFrequency)
                            .SetRotation(WorldItemVisualTuning.LusterRotation)
                            .SetColor(effectColor)
                            .SetImageSize0(texture)
                            .SetOpacity(intensity)
                            .SetWidth(lusterWidth)
                            .SetSourceRect(texture)
                            .Apply();

                        spriteBatch.Draw(texture, position, frame, Color.White.WithAlpha(effectColor.A / 255f), rotation, frame.Size() / 2f, scale, SpriteEffects.None, 0f);
                    });
                }
            }

            DrawItemStackCount(item, spriteBatch, rotation, scale, alphaColor, config);
        }

        private static void DrawItemSpawnFlash(Item item, SpriteBatch spriteBatch, float rotation, float scale, int whoAmI) {
            var config = ModContent.GetInstance<DrawItemInWorldConfig>();
            if (!config.EnableTrail && !config.EnableItemGlow)
                return;

            WorldItemTrailParticle trail = ModContent.GetInstance<WorldItemTrailSystem>().GetTrailForItem(whoAmI);
            if (trail == null || !trail.ShouldDrawSpawnFlash)
                return;

            if (!TryGetItemDrawData(item, rotation, scale, out var texture, out var frame, out var position, out var origin))
                return;

            float flash = trail.SpawnFlashAlpha * WorldItemVisualTuning.ItemSpawnFlashIntensity;

            spriteBatch.DrawWithEffect(BlendState.Additive, ModAsset.ShaAfterImage.Value, Main.GameViewMatrix.TransformationMatrix, effect => {
                effect.SetIntensity(flash).SetColor(Color.White).Apply();
                spriteBatch.Draw(texture, position, frame, Color.White.WithAlpha(flash), rotation, origin, scale, SpriteEffects.None, 0f);
            });
            spriteBatch.EndAndBeginDefault();

        }

        private static void DrawItemStackCount(Item item, SpriteBatch spriteBatch, float rotation, float scale, Color drawColor, DrawItemInWorldConfig config) {
            if (!config.EnableStackCount || item.stack <= 1)
                return;

            if (!TryGetItemDrawData(item, rotation, scale, out _, out var frame, out var position, out var origin))
                return;

            Vector2 corner = new Vector2(
                -origin.X + WorldItemVisualTuning.StackCountOffsetX,
                frame.Height - origin.Y + WorldItemVisualTuning.StackCountOffsetY) * scale;
            Vector2 textPosition = position + corner.RotatedBy(rotation);
            float fontScale = 0.8f * scale;
            Color textColor = Color.White.WithAlpha(drawColor.A / 255f * WorldItemVisualTuning.StackCountAlpha);

            ChatManager.DrawColorCodedStringWithShadow(
                spriteBatch,
                FontAssets.ItemStack.Value,
                item.stack.ToString(),
                textPosition,
                textColor,
                0f,
                Vector2.Zero,
                new Vector2(fontScale),
                -1f,
                fontScale);
        }

        private static bool TryGetItemDrawData(Item item, float rotation, float scale, out Texture2D texture, out Rectangle frame, out Vector2 position, out Vector2 origin) {
            if (!WorldItemVisualScaleHelper.TryGetItemDrawAnchor(item, out texture, out frame, out Vector2 worldCenter, out origin)) {
                position = default;
                return false;
            }

            position = worldCenter - Main.screenPosition;
            return true;
        }

        private static Vector2[] GetOutlineOffsets(float rotation, float scale) {
            float offset = WorldItemVisualTuning.OutlineOffset * scale;
            return new[] {
                Vector2.UnitX.RotatedBy(rotation) * offset,
                -Vector2.UnitX.RotatedBy(rotation) * offset,
                Vector2.UnitY.RotatedBy(rotation) * offset,
                -Vector2.UnitY.RotatedBy(rotation) * offset
            };
        }

        private static void DrawOutlinePass(
            SpriteBatch spriteBatch,
            Texture2D texture,
            Rectangle frame,
            Vector2 position,
            float rotation,
            float scale,
            Vector2 origin,
            Vector2[] offsets,
            Color drawColor,
            Color shaderColor,
            BlendState blendState) {
            spriteBatch.DrawWithEffect(blendState, ModAsset.ShaAfterImage.Value, Main.GameViewMatrix.TransformationMatrix, effect => {
                effect.SetIntensity(1f).SetColor(shaderColor).Apply();

                foreach (var offset in offsets)
                    spriteBatch.Draw(texture, position + offset, frame, drawColor, rotation, origin, scale, SpriteEffects.None, 0f);
            });
        }
    }
}
