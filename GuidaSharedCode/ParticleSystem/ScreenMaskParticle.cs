using GuidaSharedCode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;

namespace GuidaSharedCode {
    /// <summary>
    /// Rendering mode for screen mask particles.
    /// </summary>
    public enum ScreenMaskMode {
        Stretch,
        Tile
    }

    public class ScreenMaskParticle : Particle {
        public Texture2D customTexture;

        public ScreenMaskMode renderMode = ScreenMaskMode.Stretch;

        public Rectangle? customSourceRectangle;

        public Vector2 tileOffset = Vector2.Zero;

        public float tileScale = 1f;
        public bool nonPremultiplied = false;

        public override Texture2D Texture => customTexture ?? TextureAssets.MagicPixel.Value;

        public override Rectangle? SourceRectangle => customSourceRectangle ?? null;

        public override Vector2 Origin => Vector2.Zero;

        public override void SetDefaults() {
            base.SetDefaults();
            drawLayer = ParticleLayer.BeforeSolidTiles;
            useLighting = false;
            tileCollide = false;
            width = 1;
            height = 1;
            scale = 1f;
            renderMode = ScreenMaskMode.Stretch;
            color = new Color(255, 255, 255, 255);
        }
        public void SetTexture(Texture2D texture, ScreenMaskMode mode = ScreenMaskMode.Stretch, Rectangle? sourceRect = null) {
            customTexture = texture;
            renderMode = mode;
            customSourceRectangle = sourceRect;
        }

        protected override Vector2 GetDrawPosition() {
            return Vector2.Zero;
        }

        public override void Draw(SpriteBatch spriteBatch, Color lightColor) {
            if (Texture == null) return;

            Color finalColor = color * alpha;
            if(nonPremultiplied) spriteBatch.EndAndBeginAlpha();
            switch (renderMode) {
                case ScreenMaskMode.Stretch:
                    DrawStretched(spriteBatch, finalColor);
                    break;
                case ScreenMaskMode.Tile:
                    DrawTiled(spriteBatch, finalColor);
                    break;
            }
            spriteBatch.EndAndBeginDefault();

        }

        private void DrawStretched(SpriteBatch spriteBatch, Color finalColor) {
            Rectangle screenRec = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
            spriteBatch.Draw(
                Texture,
                screenRec,
                SourceRectangle,
                finalColor,
                rotation,
                Origin,
                SpriteEffects.None,
                0f
            );
        }

        private void DrawTiled(SpriteBatch spriteBatch, Color finalColor) {
            Rectangle sourceRect = SourceRectangle ?? new Rectangle(0, 0, Texture.Width, Texture.Height);

            Vector2 tileSize = new Vector2(sourceRect.Width, sourceRect.Height) * tileScale * scale;

            int tilesX = (int)Math.Ceiling(Main.screenWidth / tileSize.X) + 1;
            int tilesY = (int)Math.Ceiling(Main.screenHeight / tileSize.Y) + 1;

            Vector2 startPos = new Vector2(
                (tileOffset.X * tileScale * scale) % tileSize.X,
                (tileOffset.Y * tileScale * scale) % tileSize.Y
            );

            if (startPos.X > 0) startPos.X -= tileSize.X;
            if (startPos.Y > 0) startPos.Y -= tileSize.Y;

            for (int y = 0; y < tilesY; y++) {
                for (int x = 0; x < tilesX; x++) {
                    Vector2 tilePos = startPos + new Vector2(x * tileSize.X, y * tileSize.Y);
                    spriteBatch.Draw(
                        Texture,
                        tilePos,
                        sourceRect,
                        finalColor,
                        rotation,
                        Vector2.Zero,
                        tileScale * scale,
                        SpriteEffects.None,
                        0f
                    );
                }
            }
        }

        protected override bool ShouldCullOffscreen() {
            return false;
        }

        protected override bool IsOnScreen() {
            return true;
        }
    }
}