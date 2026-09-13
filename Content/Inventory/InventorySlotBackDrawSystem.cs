using System;
using GuidaSharedCode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ModLoader;
using Terraria.UI;
using static Terraria.UI.ItemSlot;

namespace InventoryVisualTweaks.Content.Inventory {
    public class InventorySlotBackDrawSystem : ModSystem {
        public override void Load() {
            On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += DrawSlotBack;
        }

        private void DrawSlotBack(
            On_ItemSlot.orig_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color orig,
            SpriteBatch spriteBatch,
            Item[] inv,
            int context,
            int slot,
            Vector2 position,
            Color lightColor) {
            var config = ModContent.GetInstance<InventoryBackColorConfig>();

            InventorySlotVisualSystem.BeginSlotDraw(inv, context, slot, ref position);

            Item currentItem = inv[slot];
            bool isEmpty = currentItem == null || currentItem.IsAir;
            bool applySlotTint = config.EnableInventoryBackColorsOverride;
            bool applyItemTint = config.EnableItemBackColor && !isEmpty;
            bool applyEmptyTransparency = config.EnableEmptySlotTransparency && isEmpty;

            if (!applySlotTint && !applyItemTint && !applyEmptyTransparency) {
                orig(spriteBatch, inv, context, slot, position, lightColor);
                InventorySlotVisualSystem.EndSlotDraw();
                return;
            }

            Main.spriteBatch.EndAndBegin(BlendState.AlphaBlend, SamplerState.LinearClamp, ModAsset.BorderBackShade.Value, Main.UIScaleMatrix);

            int itemBackColorOpacity;
            Color itemColor = ColorSolver.GetContextColor(inv, context, slot, position, out itemBackColorOpacity);

            float tintIntensity = 0f;
            if (applySlotTint) {
                tintIntensity = itemColor.A / 255f * config.InventoryBackColorsIntensity;
            } else {
                itemColor = Color.White;
            }

            if (applyItemTint && itemBackColorOpacity > 0) {
                Color col = ColorSolver.ResolveUseColor(config.ItemBackColorBased, currentItem);
                if (config.DoNotUseItemBackColorIfRarityIsLessThan < 0 || currentItem.rare >= config.DoNotUseItemBackColorIfRarityIsLessThan) {
                    float mix = itemBackColorOpacity / 255f;
                    itemColor = Color.Lerp(itemColor, col, mix);
                    if (!applySlotTint)
                        tintIntensity = mix * config.InventoryBackColorsIntensity;
                }
            }

            Vector3 vec = Main.rgbToHsl(itemColor);
            vec.Z = Math.Min(vec.Z * 2f, 10);
            vec.Y = Math.Min(vec.Y * 1.1f, 1);

            if (applyItemTint && config.ItemBackColorBased == UseColor.RarityColor)
                vec.Z = MathHelper.Lerp(vec.Z, 0.6f + vec.Z * 0.2f, itemBackColorOpacity / 255f);

            if (isEmpty) {
                vec.Y *= 0.5f;
                vec.Z *= 0.5f;
            }

            float opa = applyEmptyTransparency ? 0.6f : 1f;

            ModAsset.BorderBackShade.Value
                .SetColor(vec)
                .SetIntensity(tintIntensity)
                .SetOpacity(opa)
                .Apply();

            orig(spriteBatch, inv, context, slot, position, lightColor);

            Main.spriteBatch.EndAndBegin(BlendState.AlphaBlend, SamplerState.LinearClamp, null, Main.UIScaleMatrix);
            InventorySlotVisualSystem.EndSlotDraw();
        }
    }
}
