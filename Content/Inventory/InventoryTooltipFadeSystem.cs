using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using ReLogic.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI.Chat;

namespace InventoryVisualTweaks.Content.Inventory {
    public class InventoryTooltipFadeSystem : ModSystem {
        private const int CancelFadeOutVisibleFrames = 2;
        private const int VanillaHeldItemTooltipOffsetX = 34;

        private enum CachedTooltipKind {
            None,
            Inventory,
            Fake
        }

        private static float FadeProgress;
        private static int HiddenFrames;
        private static int VisibleStreak;
        private static CachedTooltipKind CachedKind;
        private static Item CachedHoverItem = new();
        private static string CachedHoverItemName;
        private static string CachedFakeTooltipText;

        private static bool FakeTooltipLive;
        private static bool SuppressFakeTooltipLive;
        private static bool FadeQueuedDraw;
        private static bool PendingInventoryLinger;
        private static bool PendingFakeLinger;
        private static bool DrawingItemTooltip;
        private static bool TooltipFadingOut;
        private static uint FadeLastUpdateCounter;
        private static float HeldItemTooltipOffsetProgress;

        internal static float TooltipFadeAlpha { get; private set; }

        public override void Load() {
            On_Main.DrawInterface_33_MouseText += OnDrawMouseText;
            On_Main.DrawPendingMouseText += OnDrawPendingMouseText;
            On_Main.MouseText_string_int_byte_int_int_int_int_int += OnQueueMouseText;
            On_Utils.DrawInvBG_SpriteBatch_Rectangle_Color += OnDrawInvBg;
            On_ChatManager.DrawColorCodedStringWithShadow_SpriteBatch_DynamicSpriteFont_string_Vector2_Color_float_Vector2_Vector2_float_float += OnDrawTooltipText;
            IL_Main.MouseTextInner += PatchMouseTextHeldItemOffset;
        }

        public override void PostUpdateEverything() {
            var config = ModContent.GetInstance<InventoryInteractionConfig>();
            if (config.EnableHeldItemTooltipOffsetEase)
                AdvanceHeldItemTooltipOffset(config);
        }

        public override void OnWorldLoad() {
            ResetState();
        }

        private static void ResetState() {
            FadeProgress = 0f;
            TooltipFadeAlpha = 0f;
            HiddenFrames = 0;
            VisibleStreak = 0;
            CachedKind = CachedTooltipKind.None;
            CachedHoverItem = new Item();
            CachedHoverItemName = null;
            CachedFakeTooltipText = null;
            FakeTooltipLive = false;
            SuppressFakeTooltipLive = false;
            FadeQueuedDraw = false;
            PendingInventoryLinger = false;
            PendingFakeLinger = false;
            DrawingItemTooltip = false;
            TooltipFadingOut = false;
            FadeLastUpdateCounter = uint.MaxValue;
            HeldItemTooltipOffsetProgress = 0f;
        }

        private void OnDrawMouseText(On_Main.orig_DrawInterface_33_MouseText orig, Main self) {
            var config = ModContent.GetInstance<InventoryInteractionConfig>();
            if (!config.EnableTooltipFade) {
                TooltipFadeAlpha = 1f;
                FadeProgress = 1f;
                orig(self);
                return;
            }

            UpdateTooltipFadeState(config);

            string savedHoverName = null;
            if (PendingInventoryLinger) {
                savedHoverName = Main.hoverItemName;
                Main.hoverItemName = "";
            }

            orig(self);

            if (PendingInventoryLinger) {
                Main.hoverItemName = savedHoverName;
                QueueInventoryLingeringTooltip(self);
            }
        }

        private static void OnQueueMouseText(
            On_Main.orig_MouseText_string_int_byte_int_int_int_int_int orig,
            Main self,
            string cursorText,
            int rare,
            byte diff,
            int hackedMouseX,
            int hackedMouseY,
            int hackedScreenWidth,
            int hackedScreenHeight,
            int pushWidthX) {
            TrackFakeTooltipQueue(cursorText);
            orig(self, cursorText, rare, diff, hackedMouseX, hackedMouseY, hackedScreenWidth, hackedScreenHeight, pushWidthX);
        }

        private static void TrackFakeTooltipQueue(string cursorText) {
            var config = ModContent.GetInstance<InventoryInteractionConfig>();
            if (!config.EnableTooltipFade
                || !Main.SettingsEnabled_OpaqueBoxBehindTooltips
                || SuppressFakeTooltipLive
                || cursorText != string.Empty
                || !IsFakeTooltipItem(Main.HoverItem)) {
                return;
            }

            FakeTooltipLive = true;
        }

        private static void OnDrawPendingMouseText(On_Main.orig_DrawPendingMouseText orig) {
            var config = ModContent.GetInstance<InventoryInteractionConfig>();
            if (config.EnableTooltipFade)
                UpdateTooltipFadeState(config);

            if (PendingFakeLinger)
                QueueFakeLingeringTooltip();

            DrawingItemTooltip = ShouldApplyTooltipFade(config);
            try {
                orig();
            } finally {
                DrawingItemTooltip = false;
                FadeQueuedDraw = false;
                PendingInventoryLinger = false;
                PendingFakeLinger = false;
                FakeTooltipLive = false;
            }
        }

        private static void UpdateTooltipFadeState(InventoryInteractionConfig config) {
            if (FadeLastUpdateCounter == Main.GameUpdateCount)
                return;

            FadeLastUpdateCounter = Main.GameUpdateCount;
            FadeQueuedDraw = false;
            PendingInventoryLinger = false;
            PendingFakeLinger = false;

            bool inventoryVisible = IsInventoryTooltipVisible();
            bool fakeVisible = IsFakeTooltipVisible();
            bool visible = inventoryVisible || fakeVisible;

            bool fadingOut = UpdateFadeIntent(visible);
            bool shouldLinger = fadingOut && FadeProgress > 0f && HasCachedTooltip();
            TooltipFadingOut = fadingOut || shouldLinger;

            ApplyFadeStep(visible && !fadingOut, shouldLinger, config);

            if (visible && !fadingOut)
                CacheActiveTooltip(inventoryVisible, fakeVisible);

            if (visible && !fadingOut)
                FadeQueuedDraw = true;

            if (shouldLinger) {
                FadeQueuedDraw = true;
                if (CachedKind == CachedTooltipKind.Inventory)
                    PendingInventoryLinger = true;
                else if (CachedKind == CachedTooltipKind.Fake)
                    PendingFakeLinger = true;
            }

            TooltipFadeAlpha = FadeProgress;

            if (FadeProgress <= 0f)
                ClearCachedTooltip();
        }

        private static bool IsInventoryTooltipVisible() =>
            !string.IsNullOrEmpty(Main.hoverItemName) && Main.mouseItem.type == 0 && Main.HoverItem.type > 0 && !IsFakeTooltipItem(Main.HoverItem);

        private static bool IsFakeTooltipVisible() =>
            Main.SettingsEnabled_OpaqueBoxBehindTooltips && FakeTooltipLive;

        private static bool HasCachedTooltip() =>
            CachedKind == CachedTooltipKind.Inventory
                ? CachedHoverItem.type > 0
                : CachedKind == CachedTooltipKind.Fake && !string.IsNullOrEmpty(CachedFakeTooltipText);

        private static void CacheActiveTooltip(bool inventoryVisible, bool fakeVisible) {
            if (inventoryVisible) {
                CachedKind = CachedTooltipKind.Inventory;
                CachedHoverItem = Main.HoverItem.Clone();
                CachedHoverItemName = Main.hoverItemName;
                return;
            }

            if (fakeVisible) {
                CachedKind = CachedTooltipKind.Fake;
                CachedFakeTooltipText = Main.HoverItem.HoverName;
            }
        }

        private static void ClearCachedTooltip() {
            HiddenFrames = 0;
            VisibleStreak = 0;
            CachedKind = CachedTooltipKind.None;
            CachedHoverItem = new Item();
            CachedHoverItemName = null;
            CachedFakeTooltipText = null;
        }

        private static bool ShouldApplyTooltipFade(InventoryInteractionConfig config) {
            if (!config.EnableTooltipFade || !FadeQueuedDraw || FadeProgress <= 0f || Main.HoverItem.type <= 0)
                return false;

            TooltipFadeAlpha = FadeProgress;
            return true;
        }

        private static bool IsFakeTooltipItem(Item item) =>
            item.type > 0 && item.value == -1 && item.scale == 0f;

        private static void OnDrawInvBg(On_Utils.orig_DrawInvBG_SpriteBatch_Rectangle_Color orig, SpriteBatch sb, Rectangle R, Color c) {
            if (DrawingItemTooltip)
            c = ScaleTooltipBackgroundColor(c);
            //c = Color.Red;
            orig(sb, R, c);
        }

        private static Vector2 OnDrawTooltipText(
            On_ChatManager.orig_DrawColorCodedStringWithShadow_SpriteBatch_DynamicSpriteFont_string_Vector2_Color_float_Vector2_Vector2_float_float orig,
            SpriteBatch spriteBatch,
            DynamicSpriteFont font,
            string text,
            Vector2 position,
            Color baseColor,
            float rotation,
            Vector2 origin,
            Vector2 baseScale,
            float maxWidth,
            float spread) {
            if (DrawingItemTooltip)
                baseColor = ScaleTooltipTextColor(baseColor);

            return orig(spriteBatch, font, text, position, baseColor, rotation, origin, baseScale, maxWidth, spread);
        }

        private static void ApplyFadeStep(bool fadeIn, bool fadeOut, InventoryInteractionConfig config) {
            if (fadeIn)
                FadeProgress = MathHelper.Clamp(FadeProgress + 1f / config.TooltipFadeInDuration, 0f, 1f);
            else if (fadeOut)
                FadeProgress = MathHelper.Clamp(FadeProgress - 1f / config.TooltipFadeOutDuration, 0f, 1f);
        }

        private static bool UpdateFadeIntent(bool visible) {
            if (visible) {
                VisibleStreak++;
                if (VisibleStreak >= CancelFadeOutVisibleFrames)
                    HiddenFrames = 0;
            } else {
                VisibleStreak = 0;
                HiddenFrames++;
            }

            return HiddenFrames > 0 && FadeProgress > 0f && VisibleStreak < CancelFadeOutVisibleFrames;
        }

        private static void QueueInventoryLingeringTooltip(Main self) {
            Main.HoverItem = CachedHoverItem.Clone();

            if (Main.SettingsEnabled_OpaqueBoxBehindTooltips)
                self.MouseText(CachedHoverItemName, CachedHoverItem.rare, 0, Main.mouseX + 6, Main.mouseY + 6);
            else
                self.MouseText(CachedHoverItemName, CachedHoverItem.rare, 0);
        }

        private static void QueueFakeLingeringTooltip() {
            if (string.IsNullOrEmpty(CachedFakeTooltipText))
                return;

            SuppressFakeTooltipLive = true;
            try {
                UICommon.TooltipMouseText(CachedFakeTooltipText);
            } finally {
                SuppressFakeTooltipLive = false;
            }
        }

        private void PatchMouseTextHeldItemOffset(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(VanillaHeldItemTooltipOffsetX))) {
                Mod.Logger.Warn("InventoryTooltipFadeSystem: could not find held-item tooltip X offset.");
                return;
            }

            cursor.Remove();
            cursor.EmitDelegate(GetHeldItemTooltipOffsetX);
        }

        private static int GetHeldItemTooltipOffsetX() {
            var config = ModContent.GetInstance<InventoryInteractionConfig>();
            if (!config.EnableHeldItemTooltipOffsetEase)
                return Main.mouseItem.IsAir ? 0 : VanillaHeldItemTooltipOffsetX;

            if (HeldItemTooltipOffsetProgress <= 0f)
                return 0;

            float eased = EaseOutCubic(HeldItemTooltipOffsetProgress);
            return (int)Math.Round(VanillaHeldItemTooltipOffsetX * eased);
        }

        private static void AdvanceHeldItemTooltipOffset(InventoryInteractionConfig config) {
            float target = Main.mouseItem.IsAir ? 0f : 1f;
            if (MathHelper.Distance(HeldItemTooltipOffsetProgress, target) <= 0.001f) {
                HeldItemTooltipOffsetProgress = target;
                return;
            }

            float step = 1f / config.HeldItemTooltipOffsetDuration;
            if (HeldItemTooltipOffsetProgress < target)
                HeldItemTooltipOffsetProgress = MathHelper.Clamp(HeldItemTooltipOffsetProgress + step, 0f, 1f);
            else
                HeldItemTooltipOffsetProgress = MathHelper.Clamp(HeldItemTooltipOffsetProgress - step, 0f, 1f);
        }

        private static float EaseOutCubic(float t) => 1f - MathF.Pow(1f - t, 3f);

        private static Color ScaleTooltipBackgroundColor(Color color) {
            var config = ModContent.GetInstance<InventoryInteractionConfig>();
            /*if (!config.EnableTooltipFade)
                return color;*/

            float alpha = TooltipFadeAlpha;
            /*if (alpha >= 0.999f)
                return color;*/

            float rgbScale = TooltipFadingOut ? alpha * alpha : alpha;
            return new Color(
                (byte)(color.R * rgbScale),
                (byte)(color.G * rgbScale),
                (byte)(color.B * rgbScale),
                (byte)(Math.Max(color.A * alpha, 1)));
        }

        private static Color ScaleTooltipTextColor(Color color) {
            var config = ModContent.GetInstance<InventoryInteractionConfig>();
            if (!config.EnableTooltipFade)
                return color;

            float alpha = TooltipFadeAlpha;
            if (alpha >= 0.999f)
                return color;

            return new Color(
                (byte)(color.R * alpha),
                (byte)(color.G * alpha),
                (byte)(color.B * alpha),
                (byte)(color.A * alpha));
        }
    }
}
