using Microsoft.Xna.Framework;

namespace InventoryVisualTweaks.Content.Inventory {
    internal static class InventorySlotVisualTuning {
        public static int PickupFlashTime = 12;

        public static int PickupFlashMinInterval = 6;

        public static int MousePickupMotionMinInterval = 6;

        public static int PickupSquashTime = 18;

        public static float PickupSquashScaleX = 1.25f;

        public static float PickupSquashScaleY = 0.80f;

        public static float HoverLerp = 0.22f;

        public static Vector2 HoverShadowOffset = new(3f, 4f);

        public static float HoverShadowAlpha = 0.42f;

        public static Vector2 HoverStackOffset = new(-1f, 4f);

        public static Vector2 HoverItemOffset = new(0f, -2f);

        public const float ReferenceHoverScaleAmount = 1.1f;

        public static float MotionDuration = 8f;

        public static float MotionGhostAlpha = 0.35f;

        public static float SlotCellSize = 56f;

        public static float BorderShadingIntensity = 0.6f;

        public static float BorderShadingOffset = 0.1f;

        public static float BorderShadingDirection = 0.5f;

        public static float NewItemDashLineAlpha = 0.75f;

        public static float NewItemFlareScale = 50f;

        public static float NewItemFlareAlpha = 0.22f;

        public static float NewItemFlareWhiten = 0.88f;

        public static float NewItemFlareRotationSpeed = 0.014f;

        public static Color UseCooldownMaskColor = new(0, 0, 0, 220);
    }
}
