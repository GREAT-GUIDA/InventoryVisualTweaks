using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.Config;

namespace InventoryVisualTweaks {
    [SeparatePage]
    public class SlotBackColorsConfig {
        [DefaultValue(typeof(Color), "30, 50, 250, 255")]
        public Color InventoryColor { get; set; }

        [DefaultValue(typeof(Color), "30, 50, 250, 255")]
        public Color HotbarColor { get; set; }

        [DefaultValue(typeof(Color), "43, 110, 250, 255")]
        public Color HotbarOpenInventoryColor { get; set; }

        [DefaultValue(typeof(Color), "255, 205, 28, 255")]
        public Color HighlightedSlotColor { get; set; }

        [DefaultValue(typeof(Color), "48, 135, 255, 255")]
        public Color NewItemHighlightColor { get; set; }

        [DefaultValue(typeof(Color), "16, 140, 240, 255")]
        public Color TrashSlotColor { get; set; }

        [DefaultValue(typeof(Color), "30, 240, 36, 0")]
        public Color EquipmentColor { get; set; }

        [DefaultValue(typeof(Color), "46, 106, 98, 0")]
        public Color VanityColor { get; set; }

        [DefaultValue(typeof(Color), "5, 175, 235, 0")]
        public Color DyeColor { get; set; }

        [DefaultValue(typeof(Color), "30, 240, 36, 255")]
        public Color MiscEquipmentColor { get; set; }

        [DefaultValue(typeof(Color), "46, 106, 98, 255")]
        public Color MiscVanityColor { get; set; }

        [DefaultValue(typeof(Color), "5, 175, 235, 255")]
        public Color MiscDyeColor { get; set; }

        [DefaultValue(typeof(Color), "255, 12, 120, 255")]
        public Color PiggyBankColor { get; set; }

        [DefaultValue(typeof(Color), "250, 42, 12, 255")]
        public Color ChestColor { get; set; }

        [DefaultValue(typeof(Color), "60, 30, 230, 255")]
        public Color NPCInteractionColor { get; set; }

        [DefaultValue(typeof(Color), "180, 238, 4, 255")]
        public Color ShopColor { get; set; }

        [DefaultValue(typeof(Color), "60, 30, 230, 255")]
        public Color CraftingColor { get; set; }

        [DefaultValue(typeof(Color), "30, 50, 250, 0")]
        public Color JourneyInfiniteColor { get; set; }

        [DefaultValue(typeof(Color), "30, 50, 250, 0")]
        public Color OtherColor { get; set; }

        public Color GetColor(int value) => value switch {
            1 or 10 => InventoryColor,
            24 => HotbarColor,
            9 => HotbarOpenInventoryColor,
            14 or 17 => HighlightedSlotColor,
            15 => NewItemHighlightColor,
            7 => TrashSlotColor,
            20 => EquipmentColor,
            21 => VanityColor,
            22 => DyeColor,
            3 => MiscEquipmentColor,
            8 => MiscVanityColor,
            12 => MiscDyeColor,
            2 or 19 => PiggyBankColor,
            5 => ChestColor,
            4 => NPCInteractionColor,
            6 => ShopColor,
            23 => CraftingColor,
            18 => JourneyInfiniteColor,
            _ => OtherColor
        };

        public override bool Equals(object obj) {
            if (obj is not SlotBackColorsConfig other)
                return false;

            return InventoryColor == other.InventoryColor
                && HotbarColor == other.HotbarColor
                && HotbarOpenInventoryColor == other.HotbarOpenInventoryColor
                && HighlightedSlotColor == other.HighlightedSlotColor
                && NewItemHighlightColor == other.NewItemHighlightColor
                && TrashSlotColor == other.TrashSlotColor
                && EquipmentColor == other.EquipmentColor
                && VanityColor == other.VanityColor
                && DyeColor == other.DyeColor
                && MiscEquipmentColor == other.MiscEquipmentColor
                && MiscVanityColor == other.MiscVanityColor
                && MiscDyeColor == other.MiscDyeColor
                && PiggyBankColor == other.PiggyBankColor
                && ChestColor == other.ChestColor
                && NPCInteractionColor == other.NPCInteractionColor
                && ShopColor == other.ShopColor
                && CraftingColor == other.CraftingColor
                && JourneyInfiniteColor == other.JourneyInfiniteColor
                && OtherColor == other.OtherColor;
        }

        public override int GetHashCode() {
            HashCode hash = new HashCode();
            hash.Add(InventoryColor);
            hash.Add(HotbarColor);
            hash.Add(HotbarOpenInventoryColor);
            hash.Add(HighlightedSlotColor);
            hash.Add(NewItemHighlightColor);
            hash.Add(TrashSlotColor);
            hash.Add(EquipmentColor);
            hash.Add(VanityColor);
            hash.Add(DyeColor);
            hash.Add(MiscEquipmentColor);
            hash.Add(MiscVanityColor);
            hash.Add(MiscDyeColor);
            hash.Add(PiggyBankColor);
            hash.Add(ChestColor);
            hash.Add(NPCInteractionColor);
            hash.Add(ShopColor);
            hash.Add(CraftingColor);
            hash.Add(JourneyInfiniteColor);
            hash.Add(OtherColor);
            return hash.ToHashCode();
        }
    }
}
