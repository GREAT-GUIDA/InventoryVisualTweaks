using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using ReLogic.Content;
using Terraria.GameContent;
using System;

namespace GuidaSharedCode {
    public class MyCustomSky : CustomSky {
        public bool _isActive;
        public float Intensity;
        public int timer = 0;

        public static RenderTarget2D cachedTempTarget;
        public static int cachedWidth = -1;
        public static int cachedHeight = -1;
        public float Brightness = 1;

        public override void OnLoad() {
            Main.RunOnMainThread(() => {
                cachedTempTarget?.Dispose();
                cachedTempTarget = null;
            });
            cachedWidth = -1;
            cachedHeight = -1;
        }

        public override void Update(GameTime gameTime) {
            timer += 1;
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth) {
            if (Intensity < 0.01f) return;
        }

        public void RecreateRenderTarget(int width, int height) {
            if (cachedTempTarget != null && !cachedTempTarget.IsDisposed) {
                cachedTempTarget.Dispose();
            }
            cachedTempTarget = new RenderTarget2D(
                Main.instance.GraphicsDevice,
                width / 2, height / 2,
                false, SurfaceFormat.Color, DepthFormat.None
            );
            cachedWidth = width;
            cachedHeight = height;
        }

        
        public override float GetCloudAlpha() {
            return 1f;
        }

        public override void Activate(Vector2 position, params object[] args) {
            _isActive = true;
            Intensity = 0f;
        }

        public override void Deactivate(params object[] args) {
            _isActive = false;
        }

        public override void Reset() {
            _isActive = false;
            Intensity = 0f;
            timer = 0;
        }

        public override bool IsActive() {
            return _isActive;
        }
    }
}