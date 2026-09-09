
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace GuidaSharedCode {


    public static class GuidaUtils {
        public static void DrawBorderStringEightWay(SpriteBatch sb, DynamicSpriteFont font, string text, Vector2 baseDrawPosition, Color main, Color border, float scale = 1f) {
            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    Vector2 drawPosition = baseDrawPosition + new Vector2(x, y);
                    if (x == 0 && y == 0)
                        continue;
                    DynamicSpriteFontExtensionMethods.DrawString(sb, font, text, drawPosition, border, 0f, default, scale, SpriteEffects.None, 0f);
                }
            }
            DynamicSpriteFontExtensionMethods.DrawString(sb, font, text, baseDrawPosition, main, 0f, default, scale, SpriteEffects.None, 0f);
        }

        public static float Smoothstep(float t1, float t2, float x) {
            x = MathHelper.Clamp((x - t1) / (t2 - t1), 0, 1);
            return x * x * (3 - 2 * x);
        }
        public static float Cross(this Vector2 vec, Vector2 vec2) {
            return vec.X * vec2.Y - vec.Y * vec2.X;
        }

        public static float PackVec2(Vector2 value, float min = -1000, float max = 1000) {
            float range = max - min;
            if (range <= 0) {
                return 0;
            }

            Vector2 clampedValue = Vector2.Clamp(value, new Vector2(min, min), new Vector2(max, max));

            float normalizedX = (clampedValue.X - min) / range;
            float normalizedY = (clampedValue.Y - min) / range;

            uint shortX = (uint)(normalizedX * 65535.0f);
            uint shortY = (uint)(normalizedY * 65535.0f);

            uint packedUInt = (shortY << 16) | shortX;

            return BitConverter.ToSingle(BitConverter.GetBytes(packedUInt), 0);
        }

        public static Vector2 UnpackVec2(float packed, float min = -1000, float max = 1000) {
            uint packedUInt = BitConverter.ToUInt32(BitConverter.GetBytes(packed), 0);

            uint shortX = packedUInt & 0xFFFF;
            uint shortY = packedUInt >> 16;

            float normalizedX = shortX / 65535.0f;
            float normalizedY = shortY / 65535.0f;

            float range = max - min;
            float x = normalizedX * range + min;
            float y = normalizedY * range + min;

            return new Vector2(x, y);
        }

        public static Vector2 GetScreenCenter() {
            return Main.screenPosition + new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;
        }

        public static void NewScreenTarget() {
            GraphicsDevice device = Main.graphics.GraphicsDevice;
            int width = Main.screenTarget.Width;
            int height = Main.screenTarget.Height;

            Main.screenTarget.Dispose();
            Main.screenTarget = new RenderTarget2D(device, width, height, false,
                device.PresentationParameters.BackBufferFormat,
                DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        }
    }

    public static class HashUtils {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float fract(float x) => x - (float)Math.Floor(x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2 fract(Vector2 v) => new Vector2(fract(v.X), fract(v.Y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 fract(Vector3 v) => new Vector3(fract(v.X), fract(v.Y), fract(v.Z));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector4 fract(Vector4 v) => new Vector4(fract(v.X), fract(v.Y), fract(v.Z), fract(v.W));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float dot(Vector3 a, Vector3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float dot(Vector4 a, Vector4 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;

        // --- Hash º¯ÊýÊµÏÖ ---

        public static float Hash11(float p) {
            p = fract(p * .1031f);
            p *= p + 33.33f;
            p *= p + p;
            return fract(p);
        }

        public static float Hash12(Vector2 p) {
            Vector3 p3 = fract(new Vector3(p.X, p.Y, p.X) * .1031f);
            p3 += Vector3.One * dot(p3, new Vector3(p3.Y, p3.Z, p3.X) + Vector3.One * 33.33f);
            return fract((p3.X + p3.Y) * p3.Z);
        }

        public static float Hash13(Vector3 p3) {
            p3 = fract(p3 * .1031f);
            p3 += Vector3.One * dot(p3, new Vector3(p3.Z, p3.Y, p3.X) + Vector3.One * 33.33f);
            return fract((p3.X + p3.Y) * p3.Z);
        }

        public static Vector2 Hash21(float p) {
            Vector3 p3 = fract(new Vector3(p) * new Vector3(.1031f, .1030f, .0973f));
            p3 += Vector3.One * dot(p3, new Vector3(p3.Y, p3.Z, p3.X) + Vector3.One * 33.33f);
            return fract(new Vector2(p3.X + p3.Y, p3.X + p3.Z) * new Vector2(p3.Z, p3.Y));
        }

        public static Vector2 Hash22(Vector2 p) {
            Vector3 p3 = fract(new Vector3(p.X, p.Y, p.X) * new Vector3(.1031f, .1030f, .0973f));
            p3 += Vector3.One * dot(p3, new Vector3(p3.Y, p3.Z, p3.X) + Vector3.One * 33.33f);
            return fract(new Vector2(p3.X + p3.Y, p3.X + p3.Z) * new Vector2(p3.Z, p3.Y));
        }

        public static Vector3 Hash31(float p) {
            Vector3 p3 = fract(new Vector3(p) * new Vector3(.1031f, .1030f, .0973f));
            p3 += Vector3.One * dot(p3, new Vector3(p3.Y, p3.Z, p3.X) + Vector3.One * 33.33f);
            return fract(new Vector3(p3.X + p3.Y, p3.X + p3.Z, p3.Y + p3.Z) * new Vector3(p3.Z, p3.Y, p3.X));
        }

        public static Vector3 Hash33(Vector3 p3) {
            p3 = fract(p3 * new Vector3(.1031f, .1030f, .0973f));
            p3 += Vector3.One * dot(p3, new Vector3(p3.Y, p3.X, p3.Z) + Vector3.One * 33.33f);
            return fract(new Vector3(p3.X + p3.Y, p3.X + p3.X, p3.Y + p3.X) * new Vector3(p3.Z, p3.Y, p3.X));
        }

        public static Vector4 Hash44(Vector4 p4) {
            p4 = fract(p4 * new Vector4(.1031f, .1030f, .0973f, .1099f));
            float d = dot(p4, new Vector4(p4.W, p4.Z, p4.X, p4.Y) + Vector4.One * 33.33f);
            p4 += new Vector4(d);
            return fract(new Vector4(p4.X + p4.X, p4.X + p4.Y, p4.Y + p4.Z, p4.Z + p4.W)
                         * new Vector4(p4.Z, p4.Y, p4.W, p4.X));
        }
    }
    public static class NoiseUtils {
        public static float GradientNoise(Vector2 p) {
            Vector2 i = new Vector2((float)Math.Floor(p.X), (float)Math.Floor(p.Y));
            Vector2 f = p - i;

            Vector2 u = new Vector2(f.X * f.X * (3.0f - 2.0f * f.X), f.Y * f.Y * (3.0f - 2.0f * f.Y));

            float a = Vector2.Dot(HashUtils.Hash22(i + new Vector2(0, 0)) * 2f - Vector2.One, f - new Vector2(0, 0));
            float b = Vector2.Dot(HashUtils.Hash22(i + new Vector2(1, 0)) * 2f - Vector2.One, f - new Vector2(1, 0));
            float c = Vector2.Dot(HashUtils.Hash22(i + new Vector2(0, 1)) * 2f - Vector2.One, f - new Vector2(0, 1));
            float d = Vector2.Dot(HashUtils.Hash22(i + new Vector2(1, 1)) * 2f - Vector2.One, f - new Vector2(1, 1));

            return MathHelper.Lerp(MathHelper.Lerp(a, b, u.X), MathHelper.Lerp(c, d, u.X), u.Y);
        }

        public static float SimplexNoise(Vector2 p) {
            const float K1 = 0.366025404f;
            const float K2 = 0.211324865f;

            float s = (p.X + p.Y) * K1;
            Vector2 i = new Vector2((float)Math.Floor(p.X + s), (float)Math.Floor(p.Y + s));

            float t = (i.X + i.Y) * K2;
            Vector2 a = p - (i - new Vector2(t));

            Vector2 o = a.X > a.Y ? new Vector2(1, 0) : new Vector2(0, 1);
            Vector2 b = a - o + new Vector2(K2);
            Vector2 c = a - Vector2.One + new Vector2(2.0f * K2);

            Vector3 h = new Vector3(
                Math.Max(0.5f - Vector2.Dot(a, a), 0.0f),
                Math.Max(0.5f - Vector2.Dot(b, b), 0.0f),
                Math.Max(0.5f - Vector2.Dot(c, c), 0.0f)
            );

            Vector3 n = new Vector3(
                h.X * h.X * h.X * h.X * Vector2.Dot(a, HashUtils.Hash22(i) * 2f - Vector2.One),
                h.Y * h.Y * h.Y * h.Y * Vector2.Dot(b, HashUtils.Hash22(i + o) * 2f - Vector2.One),
                h.Z * h.Z * h.Z * h.Z * Vector2.Dot(c, HashUtils.Hash22(i + Vector2.One) * 2f - Vector2.One)
            );

            return Vector3.Dot(n, new Vector3(70.0f));
        }

        public static float FBM(Vector2 uv, int octaves = 4) {
            float f = 0.0f;
            float amp = 0.5f;
            for (int i = 0; i < octaves; i++) {
                f += amp * SimplexNoise(uv);
                uv *= 2.01f;
                amp *= 0.5f;
            }
            return f;
        }
    }
    public static class Easing {
        public static float Linear(float t) => t;

        public static float QuadIn(float t) => t * t;

        public static float QuadOut(float t) => 1f - (1f - t) * (1f - t);

        public static float QuadInOut(float t) =>
            t < 0.5f ? 2f * t * t : 1f - 2f * (1f - t) * (1f - t);

        public static float CubicIn(float t) => t * t * t;

        public static float CubicOut(float t) => 1f - (1f - t) * (1f - t) * (1f - t);

        public static float CubicInOut(float t) =>
            t < 0.5f ? 4f * t * t * t : 1f - 4f * (1f - t) * (1f - t) * (1f - t);

        public static float QuartIn(float t) => t * t * t * t;

        public static float QuartOut(float t) => 1f - (1f - t) * (1f - t) * (1f - t) * (1f - t);

        public static float QuartInOut(float t) =>
            t < 0.5f ? 8f * t * t * t * t : 1f - 8f * (1f - t) * (1f - t) * (1f - t) * (1f - t);

        public static float SineIn(float t) => 1f - (float)Math.Cos(t * Math.PI * 0.5);

        public static float SineOut(float t) => (float)Math.Sin(t * Math.PI * 0.5);

        public static float SineInOut(float t) => 0.5f * (1f - (float)Math.Cos(t * Math.PI));

        public static float ExpoIn(float t) => t == 0f ? 0f : (float)Math.Pow(2, 10 * (t - 1));

        public static float ExpoOut(float t) => t == 1f ? 1f : 1f - (float)Math.Pow(2, -10 * t);

        public static float ExpoInOut(float t) {
            if (t == 0f) return 0f;
            if (t == 1f) return 1f;
            return t < 0.5f ?
                0.5f * (float)Math.Pow(2, 20 * t - 10) :
                0.5f * (2f - (float)Math.Pow(2, -20 * t + 10));
        }

        public static float CircIn(float t) => 1f - (float)Math.Sqrt(1 - t * t);

        public static float CircOut(float t) => (float)Math.Sqrt(1 - (t - 1) * (t - 1));

        public static float CircInOut(float t) =>
            t < 0.5f ?
                0.5f * (1f - (float)Math.Sqrt(1 - 4 * t * t)) :
                0.5f * ((float)Math.Sqrt(1 - 4 * (t - 1) * (t - 1)) + 1);

        public static float BackIn(float t, float s = 1.70158f) => t * t * ((s + 1) * t - s);

        public static float BackOut(float t, float s = 1.70158f) =>
            1f + (t - 1) * (t - 1) * ((s + 1) * (t - 1) + s);

        public static float BackInOut(float t, float s = 1.70158f) {
            s *= 1.525f;
            return t < 0.5f ?
                2f * t * t * ((s + 1) * 2f * t - s) :
                1f + 2f * (t - 1) * (t - 1) * ((s + 1) * 2f * (t - 1) + s);
        }

        public static float ElasticIn(float t, float amplitude = 1f, float period = 0.3f) {
            if (t == 0f || t == 1f) return t;
            float s = period / 4f;
            return -(amplitude * (float)Math.Pow(2, 10 * (t - 1)) *
                     (float)Math.Sin((t - 1 - s) * (2 * Math.PI) / period));
        }

        public static float ElasticOut(float t, float amplitude = 1f, float period = 0.3f) {
            if (t == 0f || t == 1f) return t;
            float s = period / 4f;
            return amplitude * (float)Math.Pow(2, -10 * t) *
                   (float)Math.Sin((t - s) * (2 * Math.PI) / period) + 1f;
        }

        public static float ElasticInOut(float t, float amplitude = 1f, float period = 0.3f) {
            if (t == 0f || t == 1f) return t;
            float s = period / 4f;
            return t < 0.5f ?
                -0.5f * amplitude * (float)Math.Pow(2, 20 * t - 10) *
                (float)Math.Sin((2 * t - 1 - s) * Math.PI / period) :
                0.5f * amplitude * (float)Math.Pow(2, -20 * t + 10) *
                (float)Math.Sin((2 * t - 1 - s) * Math.PI / period) + 1f;
        }

        public static float BounceOut(float t) {
            if (t < 1f / 2.75f) {
                return 7.5625f * t * t;
            } else if (t < 2f / 2.75f) {
                t -= 1.5f / 2.75f;
                return 7.5625f * t * t + 0.75f;
            } else if (t < 2.5f / 2.75f) {
                t -= 2.25f / 2.75f;
                return 7.5625f * t * t + 0.9375f;
            } else {
                t -= 2.625f / 2.75f;
                return 7.5625f * t * t + 0.984375f;
            }
        }

        public static float BounceIn(float t) => 1f - BounceOut(1f - t);

        public static float BounceInOut(float t) =>
            t < 0.5f ? 0.5f * BounceIn(t * 2f) : 0.5f * BounceOut(t * 2f - 1f) + 0.5f;

        public static float SmoothStep(float t) => t * t * (3f - 2f * t);

        public static float SmootherStep(float t) => t * t * t * (t * (t * 6f - 15f) + 10f);

        public static float Pulse(float t, float center = 0.5f, float width = 0.5f) {
            float distance = Math.Abs(t - center);
            return distance < width ? 1f - distance / width : 0f;
        }

        public static float Spike(float t) => t <= 0.5f ? 2f * t : 2f * (1f - t);

        public static float Wave(float t, float frequency = 1f) =>
            0.5f + 0.5f * (float)Math.Sin(t * frequency * 2f * Math.PI);

        public static float Sawtooth(float t, float frequency = 1f) =>
            2f * (t * frequency - (float)Math.Floor(t * frequency + 0.5f));

        public static float Square(float t, float frequency = 1f, float dutyCycle = 0.5f) =>
            t * frequency % 1f < dutyCycle ? 1f : 0f;
    }
}