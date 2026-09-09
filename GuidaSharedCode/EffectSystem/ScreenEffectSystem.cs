using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Capture;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace GuidaSharedCode {
    public class ScreenEffectSystem : ModSystem {
        public static RenderTarget2D glowTarget;

        private static List<GlowInfo> glows = new List<GlowInfo>();
        public static Color GlowColor = Color.White;
        public static float GlowIntensity = 1f;

        private static List<IScreenEffect> effects = new List<IScreenEffect>();
        private static Dictionary<string, bool> previousActiveStates = new Dictionary<string, bool>();
        private uint lastCheckFrame = 0;

        public struct GlowInfo {
            public Vector2 Position;
            public Color Color;
            public float Scale;
            public float Intensity;

            public GlowInfo(Vector2 position, Color color, float scale = 1f, float intensity = 1f) {
                Position = position;
                Color = color;
                Scale = scale;
                Intensity = intensity;
            }
        }

        public interface IScreenEffect {
            string Name { get; }
            float Intensity { get; set; }
            bool IsActive { get; }
            float FadeSpeed { get; set; }

            float TargetIntensity();
            void OnActivate();
            void OnUpdate();
            void OnDeactivate();
            void Initialize();
            void Unload();
        }

        public override void PreUpdateEntities() {
            if (!Main.gameMenu) {
                if (glowTarget == null) {
                    RecreateRenderTargets(Vector2.Zero);
                }

                if (Main.GameUpdateCount != lastCheckFrame) {
                    GlowColor = Color.White;
                    glows.Clear();
                }

                lastCheckFrame = Main.GameUpdateCount;
                UpdateAllEffects();
            }
        }

        private void UpdateAllEffects() {
            foreach (var effect in effects) {
                effect.OnUpdate();

                float targetIntensity = effect.TargetIntensity();
                bool wasActive = previousActiveStates.ContainsKey(effect.Name) &&
                                 previousActiveStates[effect.Name];

                float fadeSpeed = effect.FadeSpeed > 0f ? effect.FadeSpeed : 0.05f;

                if (Math.Abs(effect.Intensity - targetIntensity) > 0.01f) {
                    if (effect.Intensity < targetIntensity) {
                        effect.Intensity = Math.Min(effect.Intensity + fadeSpeed, targetIntensity);
                    } else {
                        effect.Intensity = Math.Max(effect.Intensity - fadeSpeed, targetIntensity);
                    }
                }

                bool isActive = effect.IsActive;
                if (isActive && !wasActive) {
                    effect.OnActivate();
                } else if (!isActive && wasActive) {
                    effect.OnDeactivate();
                }

                previousActiveStates[effect.Name] = isActive;
            }
        }

        public static void RegisterEffect(IScreenEffect effect) {
            if (!effects.Contains(effect)) {
                effects.Add(effect);
                effect.Initialize();
            }
        }

        public static T GetEffect<T>() where T : class, IScreenEffect {
            foreach (var effect in effects) {
                if (effect is T typedEffect) {
                    return typedEffect;
                }
            }

            return null;
        }

        public static bool IsEffectActive<T>() where T : class, IScreenEffect {
            var effect = GetEffect<T>();
            return effect?.IsActive ?? false;
        }

        public static float GetEffectIntensity<T>() where T : class, IScreenEffect {
            var effect = GetEffect<T>();
            return effect?.Intensity ?? 0f;
        }

        public static void SetEffectIntensity<T>(float intensity) where T : class, IScreenEffect {
            var effect = GetEffect<T>();
            if (effect != null) {
                effect.Intensity = MathHelper.Clamp(intensity, 0f, 1f);
            }
        }

        public static bool IsEffectActive(string effectName) {
            foreach (var effect in effects) {
                if (effect.Name == effectName) {
                    return effect.IsActive;
                }
            }

            return false;
        }

        public static float GetEffectIntensity(string effectName) {
            foreach (var effect in effects) {
                if (effect.Name == effectName) {
                    return effect.Intensity;
                }
            }

            return 0f;
        }

        public override void Load() {
            if (Main.dedServ) {
                return;
            }

            On_Main.InitTargets_int_int += On_Main_InitTargets_int_int;
            Main.OnResolutionChanged += RecreateRenderTargets;

            foreach (Type type in Mod.Code.GetTypes()) {
                if (typeof(IScreenEffect).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract) {
                    RegisterEffect((IScreenEffect)Activator.CreateInstance(type));
                }
            }

            Type filterManagerType = new FilterManager().GetType();
            MethodInfo detourMethod = filterManagerType.GetMethod("EndCapture", BindingFlags.Public | BindingFlags.Instance);
            if (detourMethod != null) {
                MonoModHooks.Add(detourMethod, On_Main_DrawDust);
            }
        }

        public override void Unload() {
            if (Main.dedServ) {
                return;
            }

            Main.OnResolutionChanged -= RecreateRenderTargets;

            foreach (var effect in effects) {
                effect.Unload();
            }

            effects.Clear();
            previousActiveStates.Clear();
            glows.Clear();

            RenderTarget2D target = glowTarget;
            glowTarget = null;
            if (target != null && !target.IsDisposed) {
                Main.RunOnMainThread(() => target.Dispose());
            }
        }

        private static void RecreateRenderTargets(Vector2 vector2) {
            if (Main.dedServ || Main.graphics?.GraphicsDevice == null) {
                return;
            }

            int width = Main.screenTarget.Width;
            int height = Main.screenTarget.Height;
            GraphicsDevice device = Main.graphics.GraphicsDevice;

            glowTarget?.Dispose();
            glowTarget = new RenderTarget2D(device, width / 2, height / 2, false,
                SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        }

        private void On_Main_InitTargets_int_int(On_Main.orig_InitTargets_int_int orig, Main self, int width, int height) {
            orig.Invoke(self, width, height);
            RecreateRenderTargets(Vector2.Zero);
            GuidaUtils.NewScreenTarget();
        }

        public delegate void orig_EndCapture(object self, RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Color clearColor);

        private void On_Main_DrawDust(orig_EndCapture orig, FilterManager self, RenderTarget2D finalTexture,
            RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Color clearColor) {
            if (!CaptureManager.Instance.IsCapturing) {
                if (Main.screenTarget.RenderTargetUsage == RenderTargetUsage.DiscardContents) {
                    GuidaUtils.NewScreenTarget();
                }

                if (glowTarget == null) {
                    RecreateRenderTargets(Vector2.Zero);
                }

                if (!GlowColor.Equals(Color.White)) {
                    DrawGlow(GlowColor, 2);
                }
            }

            orig.Invoke(self, finalTexture, screenTarget1, screenTarget2, clearColor);
        }

        public static void DrawGlow(Color color, float sizeRate) {
            SpriteBatch spriteBatch = Main.spriteBatch;
            GraphicsDevice device = Main.graphics.GraphicsDevice;
            int width = Main.screenTarget.Width;
            int height = Main.screenTarget.Height;
            Texture2D glowTexture = ModAsset.TexGlow.Value;

            device.SetRenderTarget(glowTarget);
            device.Clear(color);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            foreach (var glow in glows) {
                Vector2 screenPos = (glow.Position - Main.screenPosition) / 2;
                Vector2 rate = new Vector2(width / Main.screenWidth, height / Main.screenHeight);
                float size = 128f * glow.Scale * sizeRate;
                float opacity = glow.Intensity * GlowIntensity;
                Vector2 origin = glowTexture.Size() * 0.5f;

                spriteBatch.Draw(glowTexture, screenPos * rate, null,
                    glow.Color * opacity * 0.5f, 0f, origin,
                    rate * size / glowTexture.Width, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            device.SetRenderTarget(Main.screenTarget);

            var blendState = new BlendState {
                AlphaBlendFunction = BlendFunction.Add,
                AlphaDestinationBlend = Blend.One,
                AlphaSourceBlend = Blend.Zero,
                ColorBlendFunction = BlendFunction.Add,
                ColorDestinationBlend = Blend.SourceColor,
                ColorSourceBlend = Blend.Zero
            };

            spriteBatch.Begin(SpriteSortMode.Immediate, blendState);
            spriteBatch.Draw(glowTarget, new Rectangle(0, 0, width, height), Color.White);
            spriteBatch.End();
        }

        public static void AddGlow(Vector2 position, Color color, float scale = 1f, float intensity = 0.5f) {
            glows.Add(new GlowInfo(position, color, scale, intensity));
        }
    }
}
