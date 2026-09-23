using GuidaSharedCode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace GuidaSharedCode {
    public class TrailParticle : Particle {
        public Vector2[] trailPos = new Vector2[1];
        public float[] trailRot = new float[1];
        /// <summary>每个采样点当时的水平翻转状态，由 spriteDirection 采样得到。</summary>
        public SpriteEffects[] trailFlip = new SpriteEffects[1];
        public int trailLength = 50;
        public int trailStart;
        public int trailEnd;
        public override Texture2D Texture => customizeTexture ?? ModAsset.TexEmpty.Value;
        public Texture2D customizeTexture;
        public BlendState trailBlendState = BlendState.AlphaBlend;
        public float trailAfterImage = 0;
        public Rectangle sourceRectangle;
        new public Color? color2;
        /// <summary>
        /// 拖尾点随时间向后漂移的方向与强度：第 i 个采样点会额外位移 trailDrift * (i + i^1.6 * 0.1)。
        /// 序号越大（越靠后）漂移越多，尾巴就会朝这个方向被拉开。默认为零，不影响原有拖尾。
        /// </summary>
        public Vector2 trailDrift = Vector2.Zero;
        /// <summary>与 <see cref="SpriteBatch.Draw"/> 的 origin 一致；不设则用 source 矩形中心。</summary>
        public Vector2 trailOrigin;
        public bool useCustomTrailOrigin;
        /// <summary>非等比缩放（如 EntitySpriteDraw 的 scale 向量）；不设则用 uniform <see cref="Particle.scale"/>。</summary>
        public Vector2 trailScale2 = Vector2.One;
        public bool useTrailScale2;
        /// <summary>本帧已由外部 <see cref="PushTrailSample"/> 写入采样点时，<see cref="AI"/> 不再重复移位。</summary>
        public bool suppressAutoShift;
        /// <summary>为 true 时拖尾越旧（越靠后）绘制缩放越小。</summary>
        public bool taperTrailScale;
        /// <summary>宿主弹幕失效后不再采样，按剩余长度自然淡出。</summary>
        public int HostProjectileIndex = -1;

        public override void SetDefaults() {
            drawLayer = ParticleLayer.BeforeNPCs;
            base.SetDefaults();
            cutOffscreen = false;
        }

        public void PushTrailSample(Vector2 pos, float rot, SpriteEffects flip) {
            EnsureTrailBuffers();
            for (int i = trailPos.Length - 1; i > 0; i--) {
                trailPos[i] = trailPos[i - 1];
                trailRot[i] = trailRot[i - 1];
                trailFlip[i] = trailFlip[i - 1];
            }
            trailPos[0] = pos;
            trailRot[0] = rot;
            trailFlip[0] = flip;
            trailStart = Math.Min(trailStart + 1, trailEnd);
        }

        private void EnsureTrailBuffers() {
            if (trailPos.Length != trailLength) trailPos = new Vector2[trailLength];
            if (trailRot.Length != trailLength) trailRot = new float[trailLength];
            if (trailFlip.Length != trailLength) trailFlip = new SpriteEffects[trailLength];
        }

        public override void AI() {
            EnsureTrailBuffers();
            if (!suppressAutoShift) {
                PushTrailSample(position, rotation, spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            }
            suppressAutoShift = false;
            timeLeft -= 1;
            if (timeLeft <= 0) Kill();

            if (trailDrift != Vector2.Zero) {
                for (int i = trailEnd - 1; i >= trailStart; i--)
                    trailPos[i] += trailDrift;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            spriteBatch.EndAndBegin(trailBlendState);
            if (trailAfterImage > 0) {
                spriteBatch.EndAndBegin(trailBlendState, SamplerState.PointClamp, ModAsset.ShaAfterImage.Value);
                ModAsset.ShaAfterImage.Value.SetIntensity(trailAfterImage).SetColor(Color.White).Apply();
            }
            for (int i = trailEnd - 1; i >= trailStart; i--) {
                if (trailPos[i] != Vector2.Zero) {
                    float progress = (float)(trailEnd - i) / trailEnd;
                    var pos = trailPos[i];
                    if (type == 1) {
                        pos -= (i + (float)Math.Pow(i, 1.6f) * 0.1f + (float)Math.Sin(-Main.timeForVisualEffects * 0.12f + i * 0.2f) * 6f) * Vector2.UnitY;
                    }
                    Vector2 origin = useCustomTrailOrigin ? trailOrigin : sourceRectangle.Size() / 2f;
                    Vector2 drawScale = useTrailScale2 ? trailScale2 : new Vector2(scale);
                    if (taperTrailScale) {
                        float sizeT = 0.1f + 0.9f * progress;
                        drawScale *= sizeT;
                    }
                    spriteBatch.Draw(Texture, pos - Main.screenPosition, sourceRectangle,
                        Color.Lerp(color, color2 ?? color, progress).MultiplyRGBA(lightColor) * (progress * alpha),
                        trailRot[i], origin, drawScale, trailFlip[i], 0f);
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

        /// <summary>
        /// 采样一次弹幕状态。<paramref name="offset"/> 用于补上本体绘制时的额外偏移（如 gfxOffY、上下摇晃），
        /// 让拖尾头端和本体的绘制位置对齐。
        /// </summary>
        public void SetState(Projectile projectile, Vector2 offset = default(Vector2)) {
            position = projectile.Center + offset;
            rotation = projectile.rotation;
            spriteDirection = projectile.spriteDirection;
            timeLeft = 60;
        }
    }
}
