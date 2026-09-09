using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace GuidaSharedCode {
    // 动画曲线类型
    public enum AnimationCurve {
        Linear,          // 线性
        EaseIn,          // 缓入
        EaseOut,         // 缓出
        EaseInOut,       // 缓入缓出
        Bounce,          // 反弹
        Elastic,         // 弹性
        Custom           // 自定义函数
    }

    // 特效内部渲染层级定义（区别于粒子系统的Layer）
    public class EffectParticleLayer {
        public string TexturePath { get; set; }
        public Texture2D Texture { get; set; }
        public BlendState BlendState { get; set; } = BlendState.AlphaBlend;

        // 基础属性
        public Color BaseColor { get; set; } = Color.White;
        public float BaseScale { get; set; } = 1f;
        public Vector2 BaseScaleVector2 { get; set; } = new Vector2(1, 1);
        public float BaseOpacity { get; set; } = 1f;
        public float BaseRotation { get; set; } = 0f;

        // 动画参数
        public float ScaleSpeed { get; set; } = 0f;
        public float OpacitySpeed { get; set; } = 0f;
        public float RotationSpeed { get; set; } = 0f;
        public Vector2 PositionOffset { get; set; } = Vector2.Zero;
        public Vector2 PositionSpeed { get; set; } = Vector2.Zero;

        // 动画曲线
        public AnimationCurve ScaleCurve { get; set; } = AnimationCurve.Linear;
        public AnimationCurve OpacityCurve { get; set; } = AnimationCurve.Linear;
        public AnimationCurve RotationCurve { get; set; } = AnimationCurve.Linear;
        public AnimationCurve ColorCurve { get; set; } = AnimationCurve.Linear;

        // 自定义曲线函数
        public Func<float, float> CustomScaleCurve { get; set; }
        public Func<float, float> CustomOpacityCurve { get; set; }
        public Func<float, float> CustomRotationCurve { get; set; }
        public Func<float, Color> CustomColorCurve { get; set; }
        public Func<float, Vector2> CustomScaleVector2Curve { get; set; }
        public Func<float, Vector2> CustomPositionCurve { get; set; }

        // 生命周期控制
        public int StartFrame { get; set; } = 0;    // 开始帧
        public int EndFrame { get; set; } = -1;     // 结束帧（-1表示跟随粒子生命周期）
        public bool Loop { get; set; } = false;     // 是否循环

        // 运行时数据
        internal float _currentScale;
        internal Vector2 _currentScaleVector2;
        internal float _currentOpacity;
        internal float _currentRotation;
        internal Color _currentColor;
        internal Vector2 _currentPosition;
        internal bool _initialized = false;
        internal bool _updated = false;

        public float randomSeed;

        public EffectParticleLayer() { }

        public EffectParticleLayer(string texturePath, BlendState blendState = null) {
            TexturePath = texturePath;
            BlendState = blendState ?? BlendState.AlphaBlend;
        }

        public EffectParticleLayer(Texture2D texture, BlendState blendState = null) {
            Texture = texture;
            BlendState = blendState ?? BlendState.AlphaBlend;
        }

        // 初始化层级
        internal void Initialize() {
            if (!_initialized) {
                _currentScale = BaseScale;
                _currentOpacity = BaseOpacity;
                _currentRotation = BaseRotation;
                _currentColor = BaseColor;
                _currentPosition = PositionOffset;
                _currentScaleVector2 = BaseScaleVector2;
                _initialized = true;
                randomSeed = Main.rand.NextFloat(1);
            }
        }

        // 更新层级动画
        internal void Update(int currentFrame, int timeLeft, int totalFrames) {
            if (currentFrame < StartFrame) return;
            if (EndFrame > 0 && currentFrame > EndFrame) return;
            _updated = true;
            int effectiveFrame = currentFrame - StartFrame;
            int effectiveMaxFrames;

            // 根据层级的EndFrame计算正确的总帧数
            if (EndFrame > 0) {
                effectiveMaxFrames = EndFrame - StartFrame;
            } else {
                effectiveMaxFrames = totalFrames - StartFrame;
            }

            if (effectiveMaxFrames <= 0) return;

            float progress = (float)effectiveFrame / effectiveMaxFrames;
            if (Loop && progress > 1f) {
                progress = progress % 1f;
            }
            progress = Math.Max(0f, Math.Min(1f, progress));

            // 更新缩放
            if (CustomScaleCurve != null) {
                _currentScale = CustomScaleCurve(progress);
            } else {
                float scaleMultiplier = ApplyCurve(progress, ScaleCurve, null);
                _currentScale = BaseScale + ScaleSpeed * effectiveFrame * scaleMultiplier;
            }

            // 更新透明度
            if (CustomOpacityCurve != null) {
                _currentOpacity = MathHelper.Clamp(CustomOpacityCurve(progress), 0f, 1f);
            } else {
                float opacityMultiplier = ApplyCurve(progress, OpacityCurve, null);
                _currentOpacity = Math.Max(0f, BaseOpacity + OpacitySpeed * effectiveFrame * opacityMultiplier);
            }

            // 更新旋转
            if (CustomRotationCurve != null) {
                _currentRotation = CustomRotationCurve(progress);
            } else {
                float rotationMultiplier = ApplyCurve(progress, RotationCurve, null);
                _currentRotation = BaseRotation + RotationSpeed * effectiveFrame * rotationMultiplier;
            }

            // 更新颜色
            if (CustomColorCurve != null) {
                _currentColor = CustomColorCurve(progress);
            } else {
                float colorMultiplier = ApplyCurve(progress, ColorCurve, null);
                _currentColor = BaseColor;
            }

            // 更新缩放向量
            if (CustomScaleVector2Curve != null) {
                _currentScaleVector2 = CustomScaleVector2Curve(progress);
            } else {
                _currentScaleVector2 = BaseScaleVector2;
            }

            // 更新位置
            if (CustomPositionCurve != null) {
                _currentPosition = PositionOffset + CustomPositionCurve(progress);
            } else {
                _currentPosition = PositionOffset + PositionSpeed * effectiveFrame;
            }
        }

        private float ApplyCurve(float t, AnimationCurve curve, Func<float, float> customCurve) {
            if (curve == AnimationCurve.Custom && customCurve != null) {
                return customCurve(t);
            }

            return curve switch {
                AnimationCurve.Linear => t,
                AnimationCurve.EaseIn => t * t,
                AnimationCurve.EaseOut => 1f - (1f - t) * (1f - t),
                AnimationCurve.EaseInOut => t < 0.5f ? 2f * t * t : 1f - 2f * (1f - t) * (1f - t),
                AnimationCurve.Bounce => BounceEase(t),
                AnimationCurve.Elastic => ElasticEase(t),
                _ => t
            };
        }

        private float BounceEase(float t) {
            if (t < 1f / 2.75f) return 7.5625f * t * t;
            if (t < 2f / 2.75f) return 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f;
            if (t < 2.5f / 2.75f) return 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f;
            return 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f;
        }

        private float ElasticEase(float t) {
            if (t == 0f || t == 1f) return t;
            float p = 0.3f;
            float s = p / 4f;
            return (float)(Math.Pow(2f, -10f * t) * Math.Sin((t - s) * (2f * Math.PI) / p) + 1f);
        }
    }

    // 自定义事件参数
    public class EffectEventArgs {
        public int CurrentFrame { get; set; }
        public int MaxFrames { get; set; }
        public float Progress { get; set; }
        public Vector2 Center { get; set; }
        public Particle Particle { get; set; }
    } 

    public abstract class EffectParticle : Particle {
        protected List<EffectParticleLayer> _effectLayers = new List<EffectParticleLayer>();
        protected int _currentFrame = 0;
        protected int _originalTimeLeft = 60;

        public override Texture2D Texture => null;

        public event Action<EffectEventArgs> OnUpdate;
        public event Action<EffectEventArgs> OnDraw;
        public event Action<EffectEventArgs> OnStart;
        public event Action<EffectEventArgs> OnEnd;

        protected abstract void SetupEffectLayers();
        protected abstract void SetupParticleDefaults();

        public override void SetDefaults() {
            // 基础设置
            width = 10;
            height = 10;
            timeLeft = 120;
            scale = 1f;
            alpha = 1f;
            color = Color.White;
            drawLayer = ParticleLayer.BeforeProjectiles;
            tileCollide = false;
            active = true;

            // 调用子类自定义设置
            SetupParticleDefaults();

            _originalTimeLeft = timeLeft;

            // 设置特效层级
            SetupEffectLayers();

            // 加载纹理并初始化层级
            foreach (var layer in _effectLayers) {
                if (!string.IsNullOrEmpty(layer.TexturePath)) {
                    layer.Texture = ModContent.Request<Texture2D>(layer.TexturePath).Value;
                }
                layer.Initialize();
            }

            // 触发开始事件
            OnStart?.Invoke(new EffectEventArgs {
                CurrentFrame = 0,
                MaxFrames = _originalTimeLeft,
                Progress = 0f,
                Center = position,
                Particle = this
            });
        }

        public override void AI() {
            _currentFrame++;

            // 更新所有特效层级
            foreach (var layer in _effectLayers) {
                layer.Update(_currentFrame, timeLeft, _originalTimeLeft);
            }

            // 触发更新事件
            var eventArgs = new EffectEventArgs {
                CurrentFrame = _currentFrame,
                MaxFrames = _originalTimeLeft,
                Progress = (float)_currentFrame / _originalTimeLeft,
                Center = position,
                Particle = this
            };

            OnUpdate?.Invoke(eventArgs);

            // 调用基础AI
            base.AI();
        }

        public override void PostAI() {
            // 粒子结束时触发结束事件
            if (timeLeft <= 1) {
                var eventArgs = new EffectEventArgs {
                    CurrentFrame = _currentFrame,
                    MaxFrames = _originalTimeLeft,
                    Progress = (float)_currentFrame / _originalTimeLeft,
                    Center = position,
                    Particle = this
                };
                OnEnd?.Invoke(eventArgs);
            }

            base.PostAI();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            var eventArgs = new EffectEventArgs {
                CurrentFrame = _currentFrame,
                MaxFrames = _originalTimeLeft,
                Progress = (float)_currentFrame / _originalTimeLeft,
                Center = position,
                Particle = this
            };

            BlendState currentBlendState = null;

            // 按添加顺序绘制特效层级
            foreach (var layer in _effectLayers) {
                if (layer.Texture == null) continue;
                if (_currentFrame < layer.StartFrame) continue;
                if (layer.EndFrame > 0 && _currentFrame > layer.EndFrame) continue;
                if (layer._currentOpacity <= 0f) continue;
                if (!layer._updated) continue;

                // 只有当BlendState变化时才重新开始SpriteBatch
                if (currentBlendState != layer.BlendState) {
                    spriteBatch.End();

                    spriteBatch.Begin(SpriteSortMode.Deferred,
                        layer.BlendState,
                        SamplerState.PointClamp,
                        DepthStencilState.None,
                        RasterizerState.CullNone,
                        null,
                        Main.GameViewMatrix.TransformationMatrix);

                    currentBlendState = layer.BlendState;
                }

                DrawEffectLayer(spriteBatch, layer);
            }

            spriteBatch.End();
            // 恢复默认渲染状态
            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullCounterClockwise,
                null,
                Main.GameViewMatrix.TransformationMatrix);

            // 触发绘制事件
            OnDraw?.Invoke(eventArgs);

            return false; // 不绘制默认纹理
        }

        private void DrawEffectLayer(SpriteBatch sb, EffectParticleLayer layer) {
            if (layer.Texture == null || layer._currentOpacity <= 0f) return;

            Vector2 origin = new Vector2(layer.Texture.Width / 2, layer.Texture.Height / 2);
            Vector2 drawPosition = GetDrawPosition() + layer._currentPosition;

            // 确保正确应用透明度：层级颜色 * 层级透明度 * 粒子透明度
            Color finalColor = layer._currentColor;
            finalColor.A = (byte)(finalColor.A * layer._currentOpacity * alpha);

            sb.Draw(
                layer.Texture,
                drawPosition,
                null,
                finalColor,
                layer._currentRotation + rotation, // 叠加粒子自身旋转
                origin,
                layer._currentScale * layer._currentScaleVector2 * scale, // 叠加粒子自身缩放
                SpriteEffects.None,
                0f
            );
        }

        // 辅助方法：添加特效层级
        protected void AddEffectLayer(EffectParticleLayer layer) {
            _effectLayers.Add(layer);
        }
    }

}
