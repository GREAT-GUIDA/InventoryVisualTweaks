using GuidaSharedCode;
using Microsoft.Xna.Framework;

namespace InventoryVisualTweaks.Content.WorldItem {
    internal static class WorldItemEffectColorHelper {
        public static Color PrepareBaseColor(Color color) =>
            color.BoostLowSaturation(
                WorldItemVisualTuning.ItemEffectLowSaturationMin,
                WorldItemVisualTuning.ItemEffectLowSaturationThreshold,
                WorldItemVisualTuning.ItemEffectLowSaturationTarget);

        public static Color ForTrail(Color color, bool highlight) =>
            color.OffsetHSL(
                highlight ? WorldItemVisualTuning.ItemEffectHueTrailHighlight : WorldItemVisualTuning.ItemEffectHueTrailBase,
                highlight ? WorldItemVisualTuning.ItemEffectSaturationHighlight : WorldItemVisualTuning.ItemEffectSaturationBase,
                0f);

        public static Color ForTrailTail(Color color) =>
            color.OffsetHSL(
                WorldItemVisualTuning.ItemEffectHueTrailTail,
                WorldItemVisualTuning.ItemEffectSaturationBase,
                0f);

        public static Color ForGlow(Color color, bool highlight) =>
            color.OffsetHSL(
                highlight ? WorldItemVisualTuning.ItemEffectHueGlowHighlight : WorldItemVisualTuning.ItemEffectHueGlowBase,
                highlight ? WorldItemVisualTuning.ItemEffectSaturationHighlight : WorldItemVisualTuning.ItemEffectSaturationGlowBase,
                0f);

        public static Color ForBeam(Color color, bool highlight) =>
            color.OffsetHSL(
                highlight ? WorldItemVisualTuning.ItemEffectHueBeamHighlight : WorldItemVisualTuning.ItemEffectHueBeamBase,
                highlight ? WorldItemVisualTuning.ItemEffectSaturationHighlight : WorldItemVisualTuning.ItemEffectSaturationBase,
                0f);

        public static Color ForBeamTail(Color color, bool highlight) =>
            color.OffsetHSL(
                highlight ? WorldItemVisualTuning.ItemEffectHueBeamHighlight : WorldItemVisualTuning.ItemEffectHueBeamTail,
                highlight ? WorldItemVisualTuning.ItemEffectSaturationHighlight : WorldItemVisualTuning.ItemEffectSaturationBase,
                0f);

        public static Color ForBeamSwipe(Color color) =>
            color.OffsetHSL(
                WorldItemVisualTuning.ItemEffectHueBeamSwipe,
                WorldItemVisualTuning.ItemEffectSaturationHighlight,
                0f);

        public static Color ForStar(Color color) =>
            color.OffsetHSL(
                WorldItemVisualTuning.ItemEffectHueStar,
                WorldItemVisualTuning.ItemEffectSaturationHighlight,
                0f);

        public static Color ForRipple(Color color, bool highlight) =>
            color.OffsetHSL(
                highlight ? WorldItemVisualTuning.ItemEffectHueRippleHighlight : WorldItemVisualTuning.ItemEffectHueRippleBase,
                highlight ? WorldItemVisualTuning.ItemEffectSaturationHighlight : WorldItemVisualTuning.ItemEffectSaturationBase,
                0f);

        public static Color ForOutline(Color color, bool highlight) =>
            color.OffsetHSL(
                highlight ? WorldItemVisualTuning.ItemEffectHueOutlineHighlight : WorldItemVisualTuning.ItemEffectHueOutlineBase,
                highlight ? WorldItemVisualTuning.ItemEffectSaturationHighlight : WorldItemVisualTuning.ItemEffectSaturationBase,
                0f);

        public static Color ForGlowSparkCore(Color color) {
            Color tinted = ForGlow(color, true);
            return Color.Lerp(tinted, Color.White, WorldItemVisualTuning.ItemGlowSparkCoreWhiten);
        }

        public static Color ForLuster(Color color) =>
            color.OffsetHSL(
                WorldItemVisualTuning.ItemEffectHueLuster,
                WorldItemVisualTuning.ItemEffectSaturationHighlight,
                0f);
    }
}
