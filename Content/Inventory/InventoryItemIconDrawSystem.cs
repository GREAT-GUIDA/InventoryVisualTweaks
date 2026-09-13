using System;
using GuidaSharedCode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.ModLoader;
using Terraria.UI;
using static Terraria.UI.ItemSlot;

namespace InventoryVisualTweaks.Content.Inventory {
    public class InventoryItemIconDrawSystem : ModSystem {
        private static float _useCooldownDisplayFraction;
        private static int _useCooldownTrackSlot = -1;
        private static int _useCooldownPreviousAnimation = -1;
        private static int _useCooldownPreviousItemTime = -1;
        private static int _useCooldownPreviousReuseDelay = -1;

        public override void Load() {
            On_ItemSlot.DrawItemIcon += DrawItemIcon;
        }

        public override void OnWorldLoad() {
            InventoryBorderTextureGenerator.RegenerateTexture();
        }

        public override void PostUpdateEverything() {
            InventoryBorderTextureGenerator.EnsureTextures();
        }

        private float DrawItemIcon(
            On_ItemSlot.orig_DrawItemIcon orig,
            Item item,
            int context,
            SpriteBatch spriteBatch,
            Vector2 screenPositionForItemCenter,
            float scale,
            float sizeLimit,
            Color environmentColor) {
            if (InventorySlotContextRules.ShouldUseVanillaItemIconOnly(context))
                return orig(item, context, spriteBatch, screenPositionForItemCenter, scale, sizeLimit, environmentColor);

            if (InventorySlotContextRules.IsMouseItemContext(context)) {
                if (InventorySlotContextRules.IsPrimaryMouseItemDraw(item, context)) {
                    InventorySlotVisualSystem.RecordMouseItemDrawCenter(screenPositionForItemCenter);
                    screenPositionForItemCenter += InventorySlotVisualSystem.GetMouseItemPickupOffset(item, screenPositionForItemCenter);
                }

                return orig(item, context, spriteBatch, screenPositionForItemCenter, scale, sizeLimit, environmentColor);
            }

            if (InventoryBorderTextureGenerator.BorderTexture == null)
                return orig(item, context, spriteBatch, screenPositionForItemCenter, scale, sizeLimit, environmentColor);

            if (item.IsAir || string.IsNullOrEmpty(item.Name))
                return orig(item, context, spriteBatch, screenPositionForItemCenter, scale, sizeLimit, environmentColor);

            var borderConfig = ModContent.GetInstance<InventoryBorderConfig>();
            var interactionConfig = ModContent.GetInstance<InventoryInteractionConfig>();

            Color borderColor = ColorSolver.ResolveUseColor(borderConfig.BorderColorBased, item).ScaleSaturation(borderConfig.BorderSaturation);
            if (borderConfig.DoNotUseBorderIfRarityIsLessThan >= 0 && item.rare < borderConfig.DoNotUseBorderIfRarityIsLessThan)
                borderColor = Color.Transparent;

            bool highlightNewItem = interactionConfig.NewItemEffectIntensity > 0f
                && item.type > 0 && item.stack > 0
                && Options.HighlightNewItems && item.newAndShiny
                && context != ItemSlot.Context.HotbarItem
                && !InventorySlotContextRules.IsTransientDisplayContext(context);

            Main.spriteBatch.EndAndBegin(BlendState.AlphaBlend, SamplerState.LinearClamp, ModAsset.BorderShade.Value, Main.UIScaleMatrix);
            ModAsset.BorderShade.Value
                .SetIntensity(InventorySlotVisualTuning.BorderShadingIntensity)
                .SetColor(borderColor)
                .SetProgress(InventorySlotVisualTuning.BorderShadingDirection)
                .SetSaturation(InventorySlotVisualTuning.BorderShadingOffset)
                .Apply();

            if (borderConfig.EnableBorder && InventoryBorderConfig.ShouldUseBorderInContext(context)) {
                spriteBatch.Draw(
                    InventoryBorderTextureGenerator.BorderTexture,
                    screenPositionForItemCenter,
                    new Rectangle(0, 0, 52, 52),
                    Color.White * borderConfig.BorderOpacity * (borderColor.A / 255f),
                    0f,
                    new Vector2(26, 26),
                    Main.inventoryScale,
                    SpriteEffects.None,
                    0f);
            }

            TryDrawOpenInventoryHotbarHighlight(spriteBatch, screenPositionForItemCenter, interactionConfig);

            if (highlightNewItem) {
                Main.spriteBatch.EndAndBegin(BlendState.AlphaBlend, SamplerState.LinearClamp, ModAsset.BorderDashLine.Value, Main.UIScaleMatrix);
                ModAsset.BorderDashLine.Value
                    .SetTime((float)Main.timeForVisualEffects / 30f)
                    .SetProgress(4f)
                    .SetWidth(0f)
                    .Apply();

                spriteBatch.Draw(
                    InventoryBorderTextureGenerator.DashLineTexture,
                    screenPositionForItemCenter,
                    new Rectangle(0, 0, 52, 52),
                    Color.White * borderConfig.BorderOpacity * (borderColor.A / 255f) * InventorySlotVisualTuning.NewItemDashLineAlpha * interactionConfig.NewItemEffectIntensity,
                    0f,
                    new Vector2(26, 26),
                    Main.inventoryScale,
                    SpriteEffects.None,
                    0f);
            }

            if (!TryComputeItemFrame(item, scale, sizeLimit, out Texture2D spriteCopy, out Rectangle frame, out Vector2 uniformDrawScale)) {
                Main.spriteBatch.EndAndBegin(BlendState.AlphaBlend, SamplerState.LinearClamp, null, Main.UIScaleMatrix);
                return orig(item, context, spriteBatch, screenPositionForItemCenter, scale, sizeLimit, environmentColor);
            }

            InventorySlotVisualSystem.TryGetActiveState(out InventorySlotVisualState visualState);
            float hoverScale = visualState?.HoverScale ?? 1f;
            float squashX = visualState?.ScaleX ?? 1f;
            float squashY = visualState?.ScaleY ?? 1f;
            Vector2 itemDrawScale = new(uniformDrawScale.X * hoverScale * squashX, uniformDrawScale.Y * hoverScale * squashY);

            float pickupFlash = interactionConfig.PickupFlashIntensity > 0f
                && visualState != null
                && !InventorySlotContextRules.IsTransientDisplayContext(context)
                && !InventorySlotDrawScope.HasActiveMotion
                && visualState.PickupFlashTimer > 0
                ? (visualState.PickupFlashTimer + 1f) / (InventorySlotVisualTuning.PickupFlashTime + 1f) * interactionConfig.PickupFlashIntensity
                : 0f;

            float hoverProgress = InventorySlotDrawScope.HoverProgress;
            Vector2 itemOffset = InventorySlotDrawScope.VisualOffset;
            Vector2 hoverItemOffset = InventorySlotVisualSystem.GetHoverItemOffset(hoverProgress, InventorySlotDrawScope.HoverScaleAmount);
            Vector2 targetCenter = screenPositionForItemCenter + hoverItemOffset;
            Vector2 itemCenter = targetCenter + itemOffset;

            if (InventorySlotDrawScope.Active && !InventorySlotContextRules.IsTransientDisplayContext(context)) {
                InventorySlotVisualSystem.RecordSlotItemIconCenter(InventorySlotDrawScope.SlotKey, screenPositionForItemCenter);
                InventorySlotVisualSystem.RecordSlotItemDrawCenter(InventorySlotDrawScope.SlotKey, itemCenter);
            }

            float iconScale = scale * hoverScale;
            Color iconEnvironmentColor = context == ItemSlot.Context.HotbarItem ? Color.White : environmentColor;

            if (InventorySlotDrawScope.HasActiveMotion)
                DrawMotionGhost(spriteBatch, spriteCopy, frame, targetCenter, itemDrawScale, iconEnvironmentColor, InventorySlotVisualTuning.MotionGhostAlpha);

            if (hoverProgress > 0.001f)
                DrawHoverShadow(spriteBatch, spriteCopy, frame, itemCenter, itemDrawScale, scale, borderColor.A / 255f, hoverProgress);

            Main.spriteBatch.EndAndBegin(BlendState.NonPremultiplied, SamplerState.LinearClamp, ModAsset.ShaAfterImage.Value, Main.UIScaleMatrix);
            ModAsset.ShaAfterImage.Value
                .SetIntensity(1f)
                .SetColor(borderColor)
                .Apply();

            if (borderConfig.ItemIconOutlineIntensity > 0f
                && (borderConfig.DoNotUseItemIconOutlineIfRarityIsLessThan < 0
                    || item.rare >= borderConfig.DoNotUseItemIconOutlineIfRarityIsLessThan)) {
                Color col = Color.White;
                col.A = (byte)(255f * borderConfig.ItemIconOutlineIntensity * (borderColor.A / 255f));
                spriteBatch.Draw(spriteCopy, itemCenter + Vector2.UnitX * 2 * scale, frame, col, 0f, frame.Size() * 0.5f, itemDrawScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(spriteCopy, itemCenter - Vector2.UnitX * 2 * scale, frame, col, 0f, frame.Size() * 0.5f, itemDrawScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(spriteCopy, itemCenter + Vector2.UnitY * 2 * scale, frame, col, 0f, frame.Size() * 0.5f, itemDrawScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(spriteCopy, itemCenter - Vector2.UnitY * 2 * scale, frame, col, 0f, frame.Size() * 0.5f, itemDrawScale, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.EndAndBegin(BlendState.AlphaBlend, SamplerState.LinearClamp, null, Main.UIScaleMatrix);

            float scl = DrawItemIconWithTransform(
                orig,
                item,
                context,
                spriteBatch,
                itemCenter,
                iconScale,
                sizeLimit,
                iconEnvironmentColor,
                squashX,
                squashY);

            DrawPickupFlash(spriteBatch, spriteCopy, frame, itemCenter, itemDrawScale, pickupFlash);

            if (highlightNewItem)
                DrawNewItemHighlight(spriteBatch, borderConfig, interactionConfig, borderColor, spriteCopy, frame, itemCenter, itemDrawScale);

            TryDrawUseCooldownMask(spriteBatch, screenPositionForItemCenter, interactionConfig);

            Main.spriteBatch.EndAndBegin(BlendState.AlphaBlend, SamplerState.LinearClamp, null, Main.UIScaleMatrix);
            return scl;
        }

        private static bool TryComputeItemFrame(Item item, float scale, float sizeLimit, out Texture2D texture, out Rectangle frame, out Vector2 drawScale) {
            texture = null;
            frame = default;
            drawScale = Vector2.One * scale;

            if (item == null || item.IsAir)
                return false;

            Main.instance.LoadItem(item.type);
            texture = TextureAssets.Item[item.type].Value;
            frame = texture.Frame(1, 1, 0, 0, 0, 0);
            if (Main.itemAnimations[item.type] != null)
                frame = Main.itemAnimations[item.type].GetFrame(texture, -1);

            float fit = 1f;
            if (frame.Width > sizeLimit || frame.Height > sizeLimit)
                fit = frame.Width <= frame.Height ? sizeLimit / frame.Height : sizeLimit / frame.Width;

            float uniform = scale * fit;
            drawScale = new Vector2(uniform, uniform);
            return true;
        }

        private static void DrawMotionGhost(
            SpriteBatch spriteBatch,
            Texture2D texture,
            Rectangle frame,
            Vector2 center,
            Vector2 drawScale,
            Color environmentColor,
            float alpha) {
            if (alpha <= 0.01f)
                return;

            Color ghostColor = environmentColor * alpha;
            ghostColor.A = (byte)(ghostColor.A * alpha);
            spriteBatch.Draw(texture, center, frame, ghostColor, 0f, frame.Size() * 0.5f, drawScale, SpriteEffects.None, 0f);
        }

        private static void DrawPickupFlash(
            SpriteBatch spriteBatch,
            Texture2D texture,
            Rectangle frame,
            Vector2 center,
            Vector2 drawScale,
            float flashAlpha) {
            if (flashAlpha <= 0.01f)
                return;

            spriteBatch.EndAndBegin(BlendState.Additive, SamplerState.LinearClamp, ModAsset.ShaAfterImage.Value, Main.UIScaleMatrix);
            ModAsset.ShaAfterImage.Value.SetIntensity(flashAlpha).SetColor(Color.White).Apply();
            spriteBatch.Draw(texture, center, frame, Color.White.WithAlpha(flashAlpha), 0f, frame.Size() * 0.5f, drawScale, SpriteEffects.None, 0f);
            spriteBatch.EndAndBegin(BlendState.AlphaBlend, SamplerState.LinearClamp, null, Main.UIScaleMatrix);
        }

        private static void DrawHoverShadow(
            SpriteBatch spriteBatch,
            Texture2D texture,
            Rectangle frame,
            Vector2 center,
            Vector2 drawScale,
            float iconScale,
            float colorAlpha,
            float hoverProgress) {
            if (hoverProgress <= 0.001f)
                return;

            Vector2 shadowOffset = InventorySlotVisualTuning.HoverShadowOffset * iconScale * hoverProgress;
            Color shadowColor = Color.Black.WithAlpha(InventorySlotVisualTuning.HoverShadowAlpha * colorAlpha * hoverProgress);
            spriteBatch.Draw(texture, center + shadowOffset, frame, shadowColor, 0f, frame.Size() * 0.5f, drawScale, SpriteEffects.None, 0f);
        }

        private static float DrawItemIconWithTransform(
            On_ItemSlot.orig_DrawItemIcon orig,
            Item item,
            int context,
            SpriteBatch spriteBatch,
            Vector2 center,
            float scale,
            float sizeLimit,
            Color environmentColor,
            float squashX,
            float squashY) {
            if (Math.Abs(squashX - 1f) < 0.001f && Math.Abs(squashY - 1f) < 0.001f)
                return orig(item, context, spriteBatch, center, scale, sizeLimit, environmentColor);

            Matrix transform = Matrix.CreateTranslation(-center.X, -center.Y, 0f)
                * Matrix.CreateScale(squashX, squashY, 1f)
                * Matrix.CreateTranslation(center.X, center.Y, 0f)
                * Main.UIScaleMatrix;
            spriteBatch.EndAndBegin(BlendState.AlphaBlend, SamplerState.LinearClamp, null, transform);
            float result = orig(item, context, spriteBatch, center, scale, sizeLimit, environmentColor);
            spriteBatch.EndAndBegin(BlendState.AlphaBlend, SamplerState.LinearClamp, null, Main.UIScaleMatrix);
            return result;
        }

        private static void TryDrawUseCooldownMask(
            SpriteBatch spriteBatch,
            Vector2 slotCenter,
            InventoryInteractionConfig interactionConfig) {
            if (!interactionConfig.EnableUseCooldownMask || interactionConfig.UseCooldownMaskOpacity <= 0f)
                return;

            if (!InventorySlotDrawScope.Active)
                return;

            int context = InventorySlotDrawScope.Context;
            int slot = InventorySlotDrawScope.Slot;

            if (!ShouldDrawUseCooldownOnSlot(context, slot, interactionConfig))
                return;

            Player player = Main.LocalPlayer;
            if (!TryGetUseCooldownRemainingFraction(player, out float remainingFraction))
                return;

            DrawUseCooldownMask(
                spriteBatch,
                slotCenter,
                context,
                slot,
                remainingFraction,
                interactionConfig.UseCooldownMaskOpacity);
        }

        private static bool ShouldDrawUseCooldownOnSlot(int context, int slot, InventoryInteractionConfig config) {
            if (!InventorySlotContextRules.IsHotbarSlot(context, slot))
                return false;

            Player player = Main.LocalPlayer;
            if (player == null || !player.active || !IsPlayerUsingItem(player))
                return false;

            if (config.UseCooldownMaskOnAllHotbarSlots)
                return true;

            return slot == player.selectedItem;
        }

        private static bool IsPlayerUsingItem(Player player) =>
            player.itemAnimation > 0 || player.itemTime > 0 || player.reuseDelay > 0;

        private static bool TryGetUseCooldownRemainingFraction(Player player, out float remainingFraction) {
            remainingFraction = 0f;

            if (!IsPlayerUsingItem(player)) {
                ResetUseCooldownDisplay();
                return false;
            }

            if (player.selectedItem != _useCooldownTrackSlot) {
                _useCooldownTrackSlot = player.selectedItem;
                ResetUseCooldownTracking();
            }

            float raw = GetRawUseCooldownFraction(player);
            bool newUse = player.itemAnimation > _useCooldownPreviousAnimation
                || (player.itemAnimation <= 0 && player.itemTime > _useCooldownPreviousItemTime)
                || (player.itemAnimation <= 0 && player.reuseDelay > _useCooldownPreviousReuseDelay);
            bool animationEnded = _useCooldownPreviousAnimation > 0 && player.itemAnimation <= 0;

            _useCooldownPreviousAnimation = player.itemAnimation;
            _useCooldownPreviousItemTime = player.itemTime;
            _useCooldownPreviousReuseDelay = player.reuseDelay;

            if (_useCooldownDisplayFraction <= 0.001f || newUse || animationEnded)
                _useCooldownDisplayFraction = raw;
            else
                _useCooldownDisplayFraction = Math.Min(_useCooldownDisplayFraction, raw);

            remainingFraction = _useCooldownDisplayFraction;
            return remainingFraction > 0.001f;
        }

        private static void ResetUseCooldownDisplay() {
            _useCooldownTrackSlot = -1;
            ResetUseCooldownTracking();
        }

        private static void ResetUseCooldownTracking() {
            _useCooldownDisplayFraction = 0f;
            _useCooldownPreviousAnimation = -1;
            _useCooldownPreviousItemTime = -1;
            _useCooldownPreviousReuseDelay = -1;
        }

        private static float GetRawUseCooldownFraction(Player player) {
            if (player.itemAnimation > 0) {
                if (player.itemAnimationMax <= 2 || player.itemAnimation <= 2)
                    return 0f;

                return (player.itemAnimation - 2) / (float)(player.itemAnimationMax - 2);
            }

            if (player.reuseDelay > 0) {
                int maxReuse = Math.Max(player.HeldItem.reuseDelay, player.reuseDelay);
                return maxReuse > 0 ? player.reuseDelay / (float)maxReuse : 1f;
            }

            if (player.itemTime > 0) {
                return player.itemTimeMax > 0
                    ? player.itemTime / (float)player.itemTimeMax
                    : 1f;
            }

            return 0f;
        }

        private static Texture2D ResolveCooldownSlotBackTexture(int context, int slot) {
            Player player = Main.LocalPlayer;

            if (context == ItemSlot.Context.HotbarItem) {
                if (slot == player.selectedItem)
                    return TextureAssets.InventoryBack14.Value;

                return TextureAssets.InventoryBack.Value;
            }

            if (context == ItemSlot.Context.InventoryItem && slot < 10)
                return TextureAssets.InventoryBack9.Value;

            return TextureAssets.InventoryBack.Value;
        }

        private static void DrawUseCooldownMask(
            SpriteBatch spriteBatch,
            Vector2 slotCenter,
            int context,
            int slot,
            float remainingFraction,
            float opacity) {
            if (remainingFraction <= 0.001f || opacity <= 0f)
                return;

            Texture2D backTexture = ResolveCooldownSlotBackTexture(context, slot);
            if (backTexture == null)
                return;

            float textureSize = backTexture.Width;
            float slotSize = textureSize * Main.inventoryScale;
            float half = slotSize * 0.5f;
            float clearedFromTop = slotSize * (1f - remainingFraction);
            float maskHeight = slotSize - clearedFromTop;

            if (maskHeight < 0.5f)
                return;

            int sourceY = (int)MathF.Floor(clearedFromTop / slotSize * textureSize);
            int sourceHeight = (int)MathF.Ceiling(textureSize) - sourceY;
            sourceY = Math.Clamp(sourceY, 0, (int)textureSize - 1);
            sourceHeight = Math.Clamp(sourceHeight, 1, (int)textureSize - sourceY);

            var sourceRect = new Rectangle(0, sourceY, (int)textureSize, sourceHeight);
            var origin = new Vector2(textureSize * 0.5f, sourceHeight * 0.5f);
            var position = new Vector2(
                slotCenter.X,
                slotCenter.Y - half + clearedFromTop + maskHeight * 0.5f);

            Color maskColor = InventorySlotVisualTuning.UseCooldownMaskColor * opacity;

            spriteBatch.Draw(
                backTexture,
                position,
                sourceRect,
                maskColor,
                0f,
                origin,
                Main.inventoryScale,
                SpriteEffects.None,
                0f);
        }

        private static void TryDrawOpenInventoryHotbarHighlight(
            SpriteBatch spriteBatch,
            Vector2 screenPositionForItemCenter,
            InventoryInteractionConfig interactionConfig) {
            if (!interactionConfig.EnableOpenInventoryHotbarHighlight
                || interactionConfig.OpenInventoryHotbarHighlightOpacity <= 0f
                || !InventorySlotDrawScope.Active)
                return;

            if (!InventorySlotContextRules.IsSelectedHotbarSlotWhenInventoryOpen(
                    InventorySlotDrawScope.Context,
                    InventorySlotDrawScope.Slot))
                return;

            Texture2D frame = TextureAssets.InventoryBack14.Value;
            Vector2 origin = frame.Size() * 0.5f;
            Color highlightColor = Color.White * interactionConfig.OpenInventoryHotbarHighlightOpacity;

            spriteBatch.Draw(
                frame,
                screenPositionForItemCenter,
                null,
                highlightColor,
                0f,
                origin,
                Main.inventoryScale,
                SpriteEffects.None,
                0f);
        }

        private static void DrawNewItemHighlight(
            SpriteBatch spriteBatch,
            InventoryBorderConfig borderConfig,
            InventoryInteractionConfig interactionConfig,
            Color borderColor,
            Texture2D spriteCopy,
            Rectangle frame,
            Vector2 itemCenter,
            Vector2 itemDrawScale) {
            Main.spriteBatch.EndAndBegin(BlendState.Additive, SamplerState.LinearClamp, ModAsset.BorderLuster.Value, Main.UIScaleMatrix);
            ModAsset.BorderLuster.Value
                .SetTime((float)Main.timeForVisualEffects / 100f)
                .SetFrequency(0.5f)
                .SetRotation(0.8f)
                .SetColor(Color.White)
                .SetImageSize0(spriteCopy)
                .SetOpacity(0.4f)
                .SetWidth(-0.6f)
                .SetSourceRect(spriteCopy)
                .Apply();

            spriteBatch.Draw(
                spriteCopy,
                itemCenter,
                frame,
                Color.White * borderConfig.BorderOpacity * (borderColor.A / 255f) * 0.5f * interactionConfig.NewItemEffectIntensity,
                0f,
                frame.Size() * 0.5f,
                itemDrawScale,
                SpriteEffects.None,
                0f);

            Color flareColor = Color.Lerp(borderColor, Color.White, InventorySlotVisualTuning.NewItemFlareWhiten)
                .WithAlpha(borderConfig.BorderOpacity * (borderColor.A / 255f) * InventorySlotVisualTuning.NewItemFlareAlpha * interactionConfig.NewItemEffectIntensity);
            float flareScale = InventorySlotVisualTuning.NewItemFlareScale * Main.inventoryScale / ModAsset.TexItemFlare.Value.Width;
            float flareRotation = (float)Main.timeForVisualEffects * InventorySlotVisualTuning.NewItemFlareRotationSpeed;

            Main.spriteBatch.EndAndBegin(BlendState.Additive, SamplerState.LinearClamp, null, Main.UIScaleMatrix);
            spriteBatch.DrawCentered(ModAsset.TexItemFlare.Value, itemCenter, flareColor, flareRotation, flareScale);
            spriteBatch.DrawCentered(ModAsset.TexItemFlare.Value, itemCenter, flareColor, -flareRotation, flareScale);
            Main.spriteBatch.EndAndBegin(BlendState.AlphaBlend, SamplerState.LinearClamp, null, Main.UIScaleMatrix);
        }
    }
}
