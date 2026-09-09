using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace InventoryVisualTweaks.Content.WorldItem {
    internal static class WorldItemVisualScaleHelper {
        public static Rectangle GetItemFrame(Item item) {
            Texture2D texture = TextureAssets.Item[item.type].Value;
            if (texture == null)
                return Rectangle.Empty;

            Rectangle frame = texture.Frame();
            if (Main.itemAnimations[item.type] != null)
                frame = Main.itemAnimations[item.type].GetFrame(texture, -1);

            return frame;
        }

        public static bool TryGetItemDrawAnchor(Item item, out Texture2D texture, out Rectangle frame, out Vector2 worldCenter, out Vector2 origin) {
            texture = TextureAssets.Item[item.type].Value;
            if (texture == null) {
                frame = default;
                worldCenter = default;
                origin = default;
                return false;
            }

            frame = GetItemFrame(item);
            if (frame.IsEmpty) {
                worldCenter = default;
                origin = default;
                return false;
            }

            origin = frame.Size() / 2f;
            Vector2 anchor = new Vector2(item.width / 2f - origin.X, item.height - frame.Height);
            worldCenter = item.position + origin + anchor;
            return true;
        }

        public static float GetWorldDrawScale(Item item, Rectangle frame) {
            if (frame.IsEmpty)
                return 1f;

            float maxDim = Math.Max(frame.Width, frame.Height);
            float reference = WorldItemVisualTuning.ItemEffectReferenceSize;
            float scale = maxDim > reference ? reference / maxDim : 1f;

            if (item.scale > 0f)
                scale *= item.scale;

            return scale;
        }

        /// <summary>
        /// Affine scale: constant base + size contribution. Not purely proportional to item size.
        /// </summary>
        public static float GetEffectScale(Item item) {
            Rectangle frame = GetItemFrame(item);
            if (frame.IsEmpty)
                return 1f;

            float maxDim = Math.Max(frame.Width, frame.Height);
            float drawScale = GetWorldDrawScale(item, frame);
            float visualSize = maxDim * drawScale;
            float normalized = visualSize / WorldItemVisualTuning.ItemEffectReferenceSize;

            return WorldItemVisualTuning.ItemEffectScaleConstant
                + WorldItemVisualTuning.ItemEffectScalePerSize * normalized;
        }

        /// <summary>
        /// Matches vanilla world item draw anchor (sprite center), not the hitbox center.
        /// </summary>
        public static Vector2 GetItemVisualCenter(Item item) {
            if (!TryGetItemDrawAnchor(item, out _, out _, out Vector2 worldCenter, out _))
                return item.Center;

            return worldCenter;
        }

        public static void GetGlowWorldRadii(float itemEffectScale, out float radiusX, out float radiusY) {
            radiusX = WorldItemVisualTuning.ItemGlowScale * itemEffectScale * WorldItemVisualTuning.ItemGlowFlatScaleX * 0.5f;
            radiusY = WorldItemVisualTuning.ItemGlowScale * itemEffectScale * WorldItemVisualTuning.ItemGlowFlatScaleY * 0.5f;
        }

        public static float GetGlowWorldRadius(float itemEffectScale) {
            GetGlowWorldRadii(itemEffectScale, out float radiusX, out float radiusY);
            return Math.Max(radiusX, radiusY);
        }
    }
}
