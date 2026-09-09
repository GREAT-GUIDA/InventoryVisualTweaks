using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace InventoryVisualTweaks.Content.Inventory {
    public class HotbarRarityNameSystem : ModSystem {
        public override void Load() {
            IL_Main.GUIHotbarDrawInner += PatchVanillaHotbarNameDraw;
        }

        private void PatchVanillaHotbarNameDraw(ILContext il) {
            ILCursor cursor = new(il);

            if (!cursor.TryGotoNext(MoveType.Before,
                    i => i.MatchCall("ReLogic.Graphics.DynamicSpriteFontExtensionMethods", "DrawString"))) {
                Mod.Logger.Warn("HotbarRarityNameSystem: could not find hotbar DrawString call, patch skipped.");
                return;
            }

            cursor.EmitDelegate(DrawHotbarName);
            cursor.Remove();
        }

        private void DrawHotbarName(
            SpriteBatch spriteBatch,
            DynamicSpriteFont spriteFont,
            string text,
            Vector2 position,
            Color color,
            float rotation,
            Vector2 origin,
            float scale,
            SpriteEffects effects,
            float layerDepth) {
            if (!ModContent.GetInstance<InventoryInteractionConfig>().EnableHotbarRarityName) {
                DynamicSpriteFontExtensionMethods.DrawString(
                    spriteBatch,
                    spriteFont,
                    text,
                    position,
                    color,
                    rotation,
                    origin,
                    scale,
                    effects,
                    layerDepth);
                return;
            }

            DrawRarityHotbarName(text, position);
        }

        private static void DrawRarityHotbarName(string text, Vector2 position) {
            Item item = Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem];
            Color color = ColorSolver.GetItemRarityColor(item);
            float alphaScale = item.IsAir || item.rare == 0 ? 1f : Main.mouseTextColor / 255f;

            var tooltipLine = new DrawableTooltipLine(new TooltipLine("ItemName", text), 0, (int)position.X, (int)position.Y, color);
            int lineIndex = 0;
            if (!ItemLoader.PreDrawTooltipLine(item, tooltipLine, ref lineIndex))
                return;

            ChatManager.DrawColorCodedStringWithShadow(
                Main.spriteBatch,
                FontAssets.MouseText.Value,
                text,
                position,
                color * alphaScale,
                0f,
                Vector2.Zero,
                Vector2.One,
                -1f,
                2f);

            ItemLoader.PostDrawTooltipLine(item, tooltipLine);
        }
    }
}
