using GuidaSharedCode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InventoryVisualTweaks.Content.Inventory {
    internal static class InventoryBorderTextureGenerator {
        public static Texture2D BorderTexture;
        public static Texture2D DashLineTexture;

        public static void RegenerateTexture() {
            var config = ModContent.GetInstance<InventoryBorderConfig>();
            if (config == null)
                return;

            Main.QueueMainThreadAction(() => {
                if (BorderTexture != null)
                    BorderTexture.Dispose();

                BorderTexture = GenerateOutlineTexture(
                    TextureAssets.InventoryBack.Value,
                    config.AdaptiveBorderOrRectangleBorder,
                    config.BorderWidth,
                    config.EnableOutline,
                    config.OutlineBrightness,
                    config.EnableCornerFrame,
                    config.CornerFrameWidth,
                    config.UnderlineHeight,
                    config.AbovelineHeight);

                if (DashLineTexture != null)
                    DashLineTexture.Dispose();

                DashLineTexture = GenerateOutlineTexture(TextureAssets.InventoryBack.Value, true, 0, true, 255, false, 0, 0, 0);
            });
        }

        public static void EnsureTextures() {
            if (BorderTexture == null || BorderTexture.IsDisposed || DashLineTexture == null || DashLineTexture.IsDisposed)
                RegenerateTexture();
        }

        private static Texture2D GenerateOutlineTexture(
            Texture2D original,
            bool adaptive,
            int borderWidth,
            bool useOutline,
            int outlineBrightness,
            bool useAngle,
            int angleWidth,
            int underlineHeight,
            int abovelineHeight) {
            bool useUnderline = underlineHeight > 0;
            bool useAboveline = abovelineHeight > 0;
            if (Main.netMode == NetmodeID.Server || original == null)
                return null;

            GraphicsDevice graphics = Main.graphics.GraphicsDevice;
            Texture2D texture2D = new(graphics, original.Width, original.Height);
            Color[] originalData = new Color[original.Width * original.Height];
            original.GetData(originalData);
            Color[] outlineData = new Color[original.Width * original.Height];

            if (!adaptive) {
                for (int chunkY = 0; chunkY < original.Width; chunkY++) {
                    for (int chunkX = 0; chunkX < original.Height; chunkX++) {
                        int outlineIndex = chunkY * original.Width + chunkX;
                        originalData[outlineIndex].A = 255;
                    }
                }
            }

            for (int chunkY = 0; chunkY < original.Width; chunkY++) {
                for (int chunkX = 0; chunkX < original.Height; chunkX++) {
                    bool shouldBeOutline = false;
                    if (originalData[chunkY * original.Width + chunkX].A > 0) {
                        for (int px = -2; px <= 2; px++) {
                            for (int py = -2; py <= 2; py++) {
                                if (px == 0 || py == 0) {
                                    int originIndex = (chunkY + py) * original.Width + chunkX + px;
                                    if (chunkX + px >= original.Width || chunkX + px < 0 || chunkY + py >= original.Height || chunkY + py < 0) {
                                        shouldBeOutline = true;
                                        break;
                                    }

                                    if (originalData[originIndex].A <= 0) {
                                        shouldBeOutline = true;
                                        break;
                                    }
                                }
                            }

                            if (shouldBeOutline)
                                break;
                        }
                    }

                    int outlineIndex = chunkY * original.Width + chunkX;
                    outlineData[outlineIndex] = shouldBeOutline ? Color.White : Color.Transparent;
                    if (shouldBeOutline)
                        originalData[outlineIndex].A = 1;
                }
            }

            for (int border = 0; border < borderWidth; border++) {
                for (int chunkY = 0; chunkY < original.Width; chunkY++) {
                    for (int chunkX = 0; chunkX < original.Height; chunkX++) {
                        bool shouldBeOutline = false;
                        int outlineIndex = chunkY * original.Width + chunkX;
                        if (originalData[outlineIndex].A > 0 && outlineData[outlineIndex].A <= 0) {
                            for (int px = -2; px <= 2; px++) {
                                for (int py = -2; py <= 2; py++) {
                                    if (px == 0 || py == 0) {
                                        int originIndex = (chunkY + py) * original.Width + chunkX + px;
                                        if (chunkX + px >= original.Width || chunkX + px < 0 || chunkY + py >= original.Height || chunkY + py < 0) {
                                            shouldBeOutline = true;
                                            break;
                                        }

                                        if (originalData[originIndex].A == border + 1) {
                                            shouldBeOutline = true;
                                            break;
                                        }
                                    }
                                }

                                if (shouldBeOutline)
                                    break;
                            }
                        }

                        if (shouldBeOutline) {
                            outlineData[outlineIndex] = Color.Black;
                            originalData[outlineIndex].A = (byte)(border + 2);
                        }
                    }
                }
            }

            if (useAngle) {
                for (int chunkY = 0; chunkY < original.Width; chunkY++) {
                    for (int chunkX = 0; chunkX < original.Height; chunkX++) {
                        int outlineIndex = chunkY * original.Width + chunkX;
                        if (chunkY >= angleWidth * 2 + 2 && chunkY < 52 - angleWidth * 2 - 2)
                            outlineData[outlineIndex] = Color.Transparent;

                        if (chunkX >= angleWidth * 2 + 2 && chunkX < 52 - angleWidth * 2 - 2)
                            outlineData[outlineIndex] = Color.Transparent;
                    }
                }
            }

            if (useUnderline) {
                for (int chunkY = 0; chunkY < original.Width; chunkY++) {
                    for (int chunkX = 0; chunkX < original.Height; chunkX++) {
                        int outlineIndex = chunkY * original.Width + chunkX;
                        if (chunkY < 50 - underlineHeight * 2) {
                        } else if (originalData[outlineIndex].A > 1) {
                            outlineData[outlineIndex] = Color.Black;
                        } else if (originalData[outlineIndex].A == 1) {
                            outlineData[outlineIndex] = Color.White;
                        }
                    }
                }
            }

            if (useAboveline) {
                for (int chunkY = 0; chunkY < original.Width; chunkY++) {
                    for (int chunkX = 0; chunkX < original.Height; chunkX++) {
                        int outlineIndex = chunkY * original.Width + chunkX;
                        if (chunkY >= abovelineHeight * 2 + 2) {
                        } else if (originalData[outlineIndex].A > 1) {
                            outlineData[outlineIndex] = Color.Black;
                        } else if (originalData[outlineIndex].A == 1) {
                            outlineData[outlineIndex] = Color.White;
                        }
                    }
                }
            }

            for (int chunkY = 0; chunkY < original.Width; chunkY++) {
                for (int chunkX = 0; chunkX < original.Height; chunkX++) {
                    int outlineIndex = chunkY * original.Width + chunkX;
                    if (outlineData[outlineIndex] == Color.Black) {
                        outlineData[outlineIndex] = Color.White;
                    } else if (outlineData[outlineIndex] == Color.White) {
                        outlineData[outlineIndex] = new Color(outlineBrightness, outlineBrightness, outlineBrightness);
                        if (!useOutline)
                            outlineData[outlineIndex] = Color.Transparent;
                    }
                }
            }

            texture2D.SetData(outlineData);
            return texture2D;
        }
    }
}
