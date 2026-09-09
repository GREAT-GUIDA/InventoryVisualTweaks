using GuidaSharedCode;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;
using Terraria.ID;
using Terraria;

namespace GuidaSharedCode {
    public class TrailParticle : Particle {
        public Vector2[] trailPos = new Vector2[1];
        public float[] trailRot = new float[1];
        public int trailLength = 50;
        public int trailStart;
        public int trailEnd;
        public override Texture2D Texture => customizeTexture ?? ModAsset.TexEmpty.Value;
        public Texture2D customizeTexture;
        public BlendState trailBlendState = BlendState.AlphaBlend;
        public float trailAfterImage = 0;
        public Rectangle sourceRectangle;
        new public Color? color2;

        public override void SetDefaults() {
            drawLayer = ParticleLayer.BeforeNPCs;
            base.SetDefaults();
            cutOffscreen = false;
            
        }
        public override void AI() {
            if (trailPos.Length != trailLength) trailPos = new Vector2[trailLength];
            if (trailRot.Length != trailLength) trailRot = new float[trailLength];
            for (int i = trailPos.Length - 1; i > 0; i--) {
                trailPos[i] = trailPos[i - 1];
                trailRot[i] = trailRot[i - 1];
            }
            trailPos[0] = position;
            trailRot[0] = rotation;
            trailStart = Math.Min(trailStart + 1, trailEnd);
            timeLeft -= 1;
            if (timeLeft <= 0) Kill();
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            spriteBatch.EndAndBegin(trailBlendState);
            if (trailAfterImage > 0) {
                spriteBatch.EndAndBegin(trailBlendState, SamplerState.LinearClamp, ModAsset.ShaAfterImage.Value);
                ModAsset.ShaAfterImage.Value.SetIntensity(trailAfterImage).SetColor(Color.White).Apply();
            }
            for (int i = trailEnd - 1; i >= trailStart; i--) {
                if (trailPos[i] != Vector2.Zero) {
                    float progress = (float)(trailEnd - i) / trailEnd;
                    var pos = trailPos[i];
                    if(type == 1){
                        pos -= (i + (float)Math.Pow(i, 1.6f) * 0.1f + (float)Math.Sin(-Main.timeForVisualEffects * 0.12f + i * 0.2f) * 6f) * Vector2.UnitY; 
                    }
                    spriteBatch.Draw(
                        Texture,
                        pos - Main.screenPosition,
                        sourceRectangle,
                        Color.Lerp(color, color2 ?? color, progress).MultiplyRGBA(lightColor) * (progress * alpha),
                        trailRot[i],
                        sourceRectangle.Size() / 2,
                        scale,
                        SpriteEffects.None,
                        0f
                    );
                }
            }
            spriteBatch.EndAndBeginDefault();
            return false;
        }

        public void SetUp(int len, Texture2D tex, Rectangle rect, BlendState bs, float alp = 1, float scl = 1) {
            trailLength = len;
            customizeTexture = tex;
            sourceRectangle = rect;
            trailBlendState = bs;
            scale = scl;
            alpha = alp;
        }

        public void SetState(NPC npc) {
            position = npc.Center;
            rotation = npc.rotation;
            timeLeft = 60;
        }

        public void SetState(Projectile projectile) {
            position = projectile.Center;
            rotation = projectile.rotation;
            timeLeft = 60;
        }
    }
}
