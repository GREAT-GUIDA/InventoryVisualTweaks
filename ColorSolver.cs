using InventoryVisualTweaks.Content.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria;
using Microsoft.Xna.Framework;
using static Terraria.UI.ItemSlot;
using tModPorter;

namespace InventoryVisualTweaks
{


    public enum UseColor {
        TypeColor,
        RarityColor,
        White
    }

    public enum ItemGlowStyle {
        Normal,
        Fancy
    }
    

    public static class ColorSolver {
        public static Color ResolveUseColor(UseColor based, Item item) => based switch {
            UseColor.RarityColor => GetItemRarityColor(item),
            UseColor.TypeColor => GetItemTypeColor(item),
            _ => Color.White
        };

        public static Color GetItemRarityColor(Item item) {
            bool normalRarity = true;
            Color abnormalColor = Color.White;
            if (item.IsACoin) {
                switch (item.type) {
                    case 71:
                        normalRarity = false;
                        abnormalColor = new Color(183, 88, 25);
                        break;
                    case 72:
                        normalRarity = false;
                        abnormalColor = new Color(124, 141, 142);
                        break;
                    case 73:
                        normalRarity = false;
                        abnormalColor = new Color(148, 126, 24);
                        break;
                    case 74:
                        normalRarity = false;
                        abnormalColor = new Color(136, 164, 176);
                        break;
                }
            } else if (item.expert || (item.rare == -12)) {
                normalRarity = false;
                abnormalColor = Main.DiscoColor;
            } else if (item.master || (item.rare == -13)) {
                normalRarity = false;
                abnormalColor = new Color(255f, Main.masterColor, 0f);
            } else if (item.rare == ItemRarityID.White) {
                normalRarity = false;
                // Vanilla ItemRarity.GetColor(0) uses Main.mouseTextColor, which pulses between 190 and 255.
                const byte fixedGray = (190 + 255) / 2;
                abnormalColor = new Color(fixedGray, fixedGray, fixedGray);
            } else if (item.rare >= 12) {
                ModRarity rarity = RarityLoader.GetRarity(item.rare);
                normalRarity = false;
                abnormalColor = rarity.RarityColor;
            }

            return (!normalRarity) ? abnormalColor : ItemRarity.GetColor(item.rare);
        }
        public static Color GetContextColor(Item[] inv, int context, int slot, Vector2 position, out int itemBackColorOpacity) {
            Player player = Main.player[Main.myPlayer];
            Item item = inv[slot];
            float inventoryScale = Main.inventoryScale;

            int value = 1;
            bool flag2 = false;
            bool highlightThingsForMouse = PlayerInput.SettingsForUI.HighlightThingsForMouse;
            if (item.type > 0 && item.stack > 0 && item.favorited && context != 13 && context != 21 && context != 22 && context != 14) {
                value = 10;
                if (context == 32)
                    value = 19;
            } else if (item.type > 0 && item.stack > 0 && ItemSlot.Options.HighlightNewItems && item.newAndShiny && context != 13 && context != 21 && context != 14 && context != 22) {
                value = 15;
            } else if (InventorySlotContextRules.IsHotbarSlot(context, slot)) {
                value = Main.playerInventory ? 9 : 24;
            } else {
                switch (context) {
                    case 28:
                        value = 7;
                        break;
                    case 16:
                    case 17:
                    case 18:
                    case 19:
                    case 20:
                    case Context.ModdedAccessorySlot: 
                        value = 3;
                        break;
                    case 8:
                    case 10:
                        value = 20;
                        break;
                    case 23:
                    case 24:
                    case 26:
                    case Context.ModdedVanityAccessorySlot:
                        value = 8;
                        break;
                    case 9:
                    case 11:
                        value = 21;
                        break;
                    case 25:
                    case 27:
                    case 33:
                    case Context.ModdedDyeSlot:
                        value = 12;
                        break;
                    case 12:
                        value = 22;
                        break;
                    case 3:
                        value = 5;
                        break;
                    case 4:
                    case 32:
                        value = 2;
                        break;
                    case 5:
                    case 7:
                        value = 4;
                        break;
                    case 6:
                        value = 7;
                        break;
                    case 13:
                        value = Main.playerInventory ? 9 : 24;
                        break;
                    case 14:
                    case 21:
                        flag2 = true;
                        break;
                    case 15:
                        value = 6;
                        break;
                    case 29:
                        value = 18;
                        break;
                    case 30:
                        flag2 = true;
                        break;
                    case 22:
                        value = 23;
                        if (DrawGoldBGForCraftingMaterial) {
                            value = 14;
                        }
                        break;
                }
            }
            if (context == 28 && Main.MouseScreen.Between(position, position + new Vector2(52) * inventoryScale) && !player.mouseInterface) {
                value = 14;
            }

            var config = ModContent.GetInstance<InventoryBackColorConfig>();
            var color = config.SlotBackColors.OtherColor;
            if (!flag2) {
                color = config.GetCustomBackgroundColor(value);
            }

            itemBackColorOpacity = InventoryBackColorConfig.ShouldUseItemBackColorInContext(context) ? 255 : 0;
            if ((context == 0 || context == 2) && FastItemSlotAccess.GetInventoryGlowTime()[slot] > 0 && !inv[slot].favorited && !inv[slot].IsAir) {
                Color value3 = Main.hslToRgb(FastItemSlotAccess.GetInventoryGlowHue()[slot], 1f, 0.7f);
                float num6 = (float)FastItemSlotAccess.GetInventoryGlowTime()[slot] / 300f;
                num6 *= num6;
                color = Color.Lerp(color, value3, num6 / 2f);
                color.A = (byte)Math.Clamp(255 - num6 * 255, 0, 255);
                itemBackColorOpacity = color.A;
            }

            if ((context == 4 || context == 32 || context == 3) && FastItemSlotAccess.GetInventoryGlowTimeChest()[slot] > 0 && !inv[slot].favorited && !inv[slot].IsAir) {
                Color value5 = Main.hslToRgb(FastItemSlotAccess.GetInventoryGlowHueChest()[slot], 1f, 0.7f);
                float num8 = (float)FastItemSlotAccess.GetInventoryGlowTimeChest()[slot] / 300f;
                num8 *= num8;
                color = Color.Lerp(color, value5, num8 / 2f);
                color.A = (byte)Math.Clamp(255 - num8 * 255, 0, 255);
                itemBackColorOpacity = color.A;
            }
            return color;

        }
        public static Color GetItemTypeColor(Item item) {
            if (item == null || item.IsAir)
                return new Color(30, 50, 220);

            if (item.IsACoin) {
                switch (item.type) {
                    case 71:
                        return new Color(183, 88, 25);
                    case 72:
                        return new Color(94, 141, 152);
                    case 73:
                        return new Color(188, 166, 24);
                    case 74:
                        return new Color(130, 160, 170);
                }
            }

            if (((item.bodySlot >= 0 || item.headSlot >= 0 || item.legSlot >= 0) && !item.vanity) || ((item.bodySlot >= 0 || item.headSlot >= 0 || item.legSlot >= 0) && item.vanity) || item.accessory)
                return new Color(140, 250, 30);//green

            if (item.pick > 0 || item.axe > 0 || item.hammer > 0 || (item.netID > 0 && ItemID.Sets.SortingPriorityTerraforming[item.netID] > -1))
                return new Color(255, 200, 0);//yellow

            if ((item.damage > 0 && item.ammo == 0) || (item.damage > 0 && item.ammo > 0) || item.ammo > 0)
                return new Color(255, 90, 80);//red

            if (Main.projHook[item.shoot] || (item.mountType != -1 && !MountID.Sets.Cart[item.mountType]) || (item.mountType != -1 && MountID.Sets.Cart[item.mountType]) || (item.buffType > 0 && Main.lightPet[item.buffType]) || (item.buffType > 0 && Main.vanityPet[item.buffType]))
                return new Color(40, 250, 140);

            if ((item.consumable && item.healLife > 0 && item.healMana < 1) || (item.consumable && item.healLife < 1 && item.healMana > 0) || (item.consumable && item.healLife > 0 && item.healMana > 0) || (item.consumable && item.buffType > 0) || item.dye > 0 || item.hairDye >= 0)
                return new Color(120, 160, 255);

            if ((item.netID > 0 && ItemID.Sets.SortingPriorityBossSpawns[item.netID] > -1) || ((item.netID > 0 && ItemID.Sets.SortingPriorityWiring[item.netID] > -1) || item.mech) || ((item.netID > 0 && ItemID.Sets.SortingPriorityPainting[item.netID] > -1) || item.paint > 0) || (item.netID > 0 && ItemID.Sets.SortingPriorityExtractibles[item.netID] > -1))
                return new Color(140, 90, 255);

            if (item.createWall > 0 || item.createTile >= 0) {
                return new Color(190, 40, 255);
            }

            return new Color(100, 150, 255);
        }
    }


    public class FastItemSlotAccess : ModSystem {
        private static Func<float[]> getInventoryGlowHue;
        private static Func<int[]> getInventoryGlowTime;
        private static Func<float[]> getInventoryGlowHueChest;
        private static Func<int[]> getInventoryGlowTimeChest;

        public override void PostSetupContent() {
            var hueField = typeof(ItemSlot).GetField("inventoryGlowHue", BindingFlags.NonPublic | BindingFlags.Static);
            getInventoryGlowHue = () => (float[])hueField.GetValue(null);

            var timeField = typeof(ItemSlot).GetField("inventoryGlowTime", BindingFlags.NonPublic | BindingFlags.Static);
            getInventoryGlowTime = () => (int[])timeField.GetValue(null);

            var hueChestField = typeof(ItemSlot).GetField("inventoryGlowHueChest", BindingFlags.NonPublic | BindingFlags.Static);
            getInventoryGlowHueChest = () => (float[])hueChestField.GetValue(null);

            var timeChestField = typeof(ItemSlot).GetField("inventoryGlowTimeChest", BindingFlags.NonPublic | BindingFlags.Static);
            getInventoryGlowTimeChest = () => (int[])timeChestField.GetValue(null);
        }
        public static float[] GetInventoryGlowHue() => getInventoryGlowHue();
        public static int[] GetInventoryGlowTime() => getInventoryGlowTime();
        public static float[] GetInventoryGlowHueChest() => getInventoryGlowHueChest();
        public static int[] GetInventoryGlowTimeChest() => getInventoryGlowTimeChest();
    }
}
