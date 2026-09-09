using InventoryVisualTweaks.Content.Inventory;
using Microsoft.Xna.Framework;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace InventoryVisualTweaks {
    public class InventoryBorderConfig : ModConfig {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [DefaultValue(true)]
        public bool EnableBorder { get; set; }

        [DefaultValue(UseColor.RarityColor)]
        public UseColor BorderColorBased { get; set; }

        [DefaultValue(0.6f)]
        [Range(0f, 1f)]
        [Slider]
        public float BorderOpacity { get; set; }

        [DefaultValue(1.2f)]
        [Range(0f, 2f)]
        [Slider]
        public float BorderSaturation { get; set; }

        [DefaultValue(-1)]
        public int DoNotUseBorderIfRarityIsLessThan { get; set; }

        [DefaultValue(true)]
        public bool AdaptiveBorderOrRectangleBorder { get; set; }

        [DefaultValue(1)]
        [Range(0, 10)]
        [Slider]
        public int BorderWidth { get; set; }

        [DefaultValue(true)]
        public bool EnableOutline { get; set; }

        [DefaultValue(40)]
        [Range(0, 255)]
        [Slider]
        public int OutlineBrightness { get; set; }

        [DefaultValue(false)]
        public bool EnableCornerFrame { get; set; }

        [DefaultValue(3)]
        [Range(1, 10)]
        [Slider]
        public int CornerFrameWidth { get; set; }

        [DefaultValue(0)]
        [Range(0, 10)]
        [Slider]
        public int UnderlineHeight { get; set; }

        [DefaultValue(0)]
        [Range(0, 10)]
        [Slider]
        public int AbovelineHeight { get; set; }

        public static bool ShouldUseBorderInContext(int context) =>
            InventorySlotContextRules.SupportsInventoryVisuals(context);

        public override void OnChanged() {
            InventoryBorderTextureGenerator.RegenerateTexture();
        }
    }

    public class InventoryBackColorConfig : ModConfig {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [DefaultValue(false)]
        public bool EnableInventoryBackColorsOverride { get; set; }

        [DefaultValue(1f)]
        [Range(0f, 2f)]
        [Slider]
        public float InventoryBackColorsIntensity { get; set; }

        [DefaultValue(true)]
        public bool EnableEmptySlotTransparency { get; set; }

        public SlotBackColorsConfig SlotBackColors { get; set; } = new();

        public Color GetCustomBackgroundColor(int value) => SlotBackColors.GetColor(value);

        [DefaultValue(false)]
        public bool EnableItemBackColor { get; set; }

        [DefaultValue(UseColor.RarityColor)]
        public UseColor ItemBackColorBased { get; set; }

        [DefaultValue(-1)]
        public int DoNotUseItemBackColorIfRarityIsLessThan { get; set; }

        public static bool ShouldUseItemBackColorInContext(int context) =>
            InventorySlotContextRules.SupportsInventoryVisuals(context);
    }

    public class InventoryInteractionConfig : ModConfig {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [DefaultValue(true)]
        public bool EnableSlotTransferMotion { get; set; }

        [DefaultValue(true)]
        public bool EnableMouseItemMotion { get; set; }

        [DefaultValue(true)]
        public bool EnableHoverScale { get; set; }

        [DefaultValue(1.2f)]
        [Range(1f, 1.8f)]
        [Slider]
        public float HoverScaleAmount { get; set; }

        [DefaultValue(0.55f)]
        [Range(0f, 1f)]
        [Slider]
        public float PickupFlashIntensity { get; set; }

        [DefaultValue(0.75f)]
        [Range(0f, 2f)]
        [Slider]
        public float NewItemEffectIntensity { get; set; }

        [DefaultValue(0.39f)]
        [Range(0f, 1f)]
        [Slider]
        public float ItemIconOutlineIntensity { get; set; }

        [DefaultValue(true)]
        public bool EnableHotbarRarityName { get; set; }

        [DefaultValue(true)]
        public bool EnableOpenInventoryHotbarHighlight { get; set; }

        [DefaultValue(0.7f)]
        [Range(0f, 1f)]
        [Slider]
        public float OpenInventoryHotbarHighlightOpacity { get; set; }

        [DefaultValue(true)]
        public bool EnableUseCooldownMask { get; set; }

        [DefaultValue(0.35f)]
        [Range(0f, 1f)]
        [Slider]
        public float UseCooldownMaskOpacity { get; set; }

        [DefaultValue(false)]
        public bool UseCooldownMaskOnAllHotbarSlots { get; set; }
    }

    public class DrawItemInWorldConfig : ModConfig {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("OutlineSettings")]
        [DefaultValue(true)]
        public bool EnableOutline { get; set; }

        [DefaultValue(UseColor.RarityColor)]
        public UseColor OutlineColorBased { get; set; }

        [DefaultValue(true)]
        public bool OutLineHighlight { get; set; }

        [Slider]
        [Range(0, 255)]
        [DefaultValue(100)]
        public int OutlineIntensity { get; set; }

        [Header("LusterSettings")]
        [DefaultValue(true)]
        public bool EnableLuster { get; set; }

        [DefaultValue(UseColor.RarityColor)]
        public UseColor LusterColorBased { get; set; }

        [Slider]
        [Range(0, 255)]
        [DefaultValue(100)]
        public int LusterIntensity { get; set; }

        [Header("TrailSettings")]
        [DefaultValue(true)]
        public bool EnableTrail { get; set; }

        [DefaultValue(UseColor.RarityColor)]
        public UseColor TrailColorBased { get; set; }

        [Slider]
        [Range(0, 255)]
        [DefaultValue(120)]
        public int TrailIntensity { get; set; }

        [DefaultValue(true)]
        public bool TrailScaleWithRarity { get; set; }

        [Header("ItemGlowSettings")]
        [DefaultValue(true)]
        public bool EnableItemGlow { get; set; }

        [Slider]
        [Range(0, 255)]
        [DefaultValue(100)]
        public int ItemGlowIntensity { get; set; }

        [DefaultValue(ItemGlowStyle.Fancy)]
        public ItemGlowStyle ItemGlowStyle { get; set; }

        [DefaultValue(true)]
        public bool ItemGlowScaleWithRarity { get; set; }

        [DefaultValue(true)]
        public bool EnableExtraParticles { get; set; }

        [Header("StackCountSettings")]
        [DefaultValue(true)]
        public bool EnableStackCount { get; set; }
    }
}
