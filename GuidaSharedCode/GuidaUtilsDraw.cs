using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace GuidaSharedCode;
public static class SpriteBatchUtils {
    //public static BlendState originalBlendState = Main.spriteBatch.GraphicsDevice.BlendState;
    //public static SamplerState originalSamplerState = Main.spriteBatch.GraphicsDevice.SamplerStates[0];
    //public static DepthStencilState originalDepthStencilState = Main.spriteBatch.GraphicsDevice.DepthStencilState;
    //public static RasterizerState originalRasterizerState = Main.spriteBatch.GraphicsDevice.RasterizerState;
    /*public static void SaveGraphicsDeviceParameters(this SpriteBatch spriteBatch) {
        originalBlendState = Main.spriteBatch.GraphicsDevice.BlendState;
        originalSamplerState = Main.spriteBatch.GraphicsDevice.SamplerStates[0];
        originalDepthStencilState = Main.spriteBatch.GraphicsDevice.DepthStencilState;
        originalRasterizerState = Main.spriteBatch.GraphicsDevice.RasterizerState;
    }*/
    public static void EndAndBeginShader(this SpriteBatch spriteBatch, Effect shader, BlendState bs = null) {
        spriteBatch.End();
        if (bs == null) {
            bs = BlendState.NonPremultiplied;
        }
        spriteBatch.Begin(default, bs, SamplerState.PointClamp, default, Main.Rasterizer, shader, Main.GameViewMatrix.TransformationMatrix);
    }
    public static void EndAndBeginShaderAdd(this SpriteBatch spriteBatch, Effect shader) {
        spriteBatch.End();
        spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointClamp, default, Main.Rasterizer, shader, Main.GameViewMatrix.TransformationMatrix);
    }
    public static void EndAndBeginAlpha(this SpriteBatch spriteBatch) {
        spriteBatch.End();
        spriteBatch.Begin(default, BlendState.NonPremultiplied, SamplerState.PointClamp, default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
    }
    public static void EndAndBeginAdd(this SpriteBatch spriteBatch) {
        spriteBatch.End();
        spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointClamp, default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
    }
    public static void EndAndBeginDefault(this SpriteBatch spriteBatch) {
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
    }
    public static void EndAndBegin(this SpriteBatch spriteBatch, BlendState bs, SamplerState ss, Effect shader, Matrix mr) {
        spriteBatch.End();
        spriteBatch.Begin(default, bs, ss, default, Main.Rasterizer, shader, mr);
    }
    public static void EndAndBegin(this SpriteBatch spriteBatch, BlendState bs, SamplerState ss) {
        spriteBatch.End();
        spriteBatch.Begin(default, bs, ss, default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
    }
    public static void EndAndBegin(this SpriteBatch spriteBatch, BlendState bs, SamplerState ss, Effect shader) {
        spriteBatch.End();
        spriteBatch.Begin(default, bs, ss, default, Main.Rasterizer, shader, Main.GameViewMatrix.TransformationMatrix);
    }
    public static void EndAndBegin(this SpriteBatch spriteBatch, BlendState bs, Effect shader) {
        spriteBatch.End();
        spriteBatch.Begin(default, bs, SamplerState.PointClamp, default, Main.Rasterizer, shader, Main.GameViewMatrix.TransformationMatrix);
    }
    public static void EndAndBegin(this SpriteBatch spriteBatch, SpriteSortMode ssm, BlendState bs) {
        spriteBatch.End();
        spriteBatch.Begin(ssm, bs, SamplerState.PointClamp, default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
    }
    public static void EndAndBegin(this SpriteBatch spriteBatch, BlendState bs) {
        spriteBatch.End();
        spriteBatch.Begin(default, bs, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
    }

    public static void EndAndBeginImmediate(this SpriteBatch spriteBatch, BlendState bs) {
        spriteBatch.End();
        spriteBatch.Begin(
            SpriteSortMode.Immediate,
            bs,
            SamplerState.LinearClamp,
            DepthStencilState.None,
            RasterizerState.CullNone,
            null,
            Main.GameViewMatrix.TransformationMatrix);
    }

    public static void EndAndBeginWorld(this SpriteBatch spriteBatch) {
        spriteBatch.End();
        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            Main.DefaultSamplerState,
            DepthStencilState.None,
            Main.Rasterizer,
            null,
            Main.GameViewMatrix.TransformationMatrix);
    }

    public static void DrawCentered(this SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Color color, float scale = 1f) {
        spriteBatch.Draw(texture, position, null, color, 0f, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
    }

    public static void DrawCentered(this SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Color color, Vector2 scale) {
        spriteBatch.Draw(texture, position, null, color, 0f, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
    }

    public static void DrawCentered(this SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Color color, float rotation, float scale) {
        spriteBatch.Draw(texture, position, null, color, rotation, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
    }
    public static void DrawWithEffect(
        this SpriteBatch spriteBatch,
        BlendState blendState,
        Effect effect,
        Matrix transform,
        Action<Effect> draw) {
        var device = spriteBatch.GraphicsDevice;
        var originalBlendState = device.BlendState;
        var originalSamplerState = device.SamplerStates[0];

        spriteBatch.EndAndBegin(blendState, originalSamplerState, effect, transform);
        draw(effect);
        spriteBatch.EndAndBegin(originalBlendState, originalSamplerState, null, transform);
    }
}

public static class ShaderUtils {
    public static void Apply(this Effect effect) {
        effect.CurrentTechnique.Passes[0].Apply();
    }

    public static void Apply(this Effect effect, Action<Effect> setup) {
        setup(effect);
        effect.Apply();
    }

    public static Effect SetTime(this Effect effect, float value) {
        effect.Parameters["uTime"].SetValue(value);
        return effect;
    }

    public static Effect SetFrequency(this Effect effect, float value) {
        effect.Parameters["uFrequency"].SetValue(value);
        return effect;
    }

    public static Effect SetRotation(this Effect effect, float value) {
        effect.Parameters["uRotation"].SetValue(value);
        return effect;
    }

    public static Effect SetColor(this Effect effect, Color color) {
        effect.Parameters["uColor"].SetValue(color.ToVector3());
        return effect;
    }

    public static Effect SetColor(this Effect effect, Vector3 color) {
        effect.Parameters["uColor"].SetValue(color);
        return effect;
    }

    public static Effect SetOpacity(this Effect effect, float value) {
        effect.Parameters["uOpacity"].SetValue(value);
        return effect;
    }

    public static Effect SetWidth(this Effect effect, float value) {
        effect.Parameters["uWidth"].SetValue(value);
        return effect;
    }

    public static Effect SetIntensity(this Effect effect, float value) {
        effect.Parameters["uIntensity"].SetValue(value);
        return effect;
    }

    public static Effect SetProgress(this Effect effect, float value) {
        effect.Parameters["uProgress"].SetValue(value);
        return effect;
    }

    public static Effect SetSaturation(this Effect effect, float value) {
        effect.Parameters["uSaturation"].SetValue(value);
        return effect;
    }

    public static Effect SetImageSize0(this Effect effect, Vector2 value) {
        effect.Parameters["uImageSize0"].SetValue(value);
        return effect;
    }

    public static Effect SetImageSize0(this Effect effect, Texture2D texture) {
        return effect.SetImageSize0(texture.Size());
    }

    public static Effect SetSourceRect(this Effect effect, Vector4 value) {
        effect.Parameters["uSourceRect"].SetValue(value);
        return effect;
    }

    public static Effect SetSourceRect(this Effect effect, Texture2D texture) {
        return effect.SetSourceRect(new Vector4(0, 0, texture.Width, texture.Height));
    }

    public static Effect Set(this Effect effect, string name, float value) {
        effect.Parameters[UniformName(name)].SetValue(value);
        return effect;
    }

    public static Effect Set(this Effect effect, string name, Vector2 value) {
        effect.Parameters[UniformName(name)].SetValue(value);
        return effect;
    }

    public static Effect Set(this Effect effect, string name, Vector3 value) {
        effect.Parameters[UniformName(name)].SetValue(value);
        return effect;
    }

    public static Effect Set(this Effect effect, string name, Vector4 value) {
        effect.Parameters[UniformName(name)].SetValue(value);
        return effect;
    }

    public static Effect Set(this Effect effect, string name, Color value) {
        return effect.Set(name, value.ToVector3());
    }

    static string UniformName(string name) =>
        name.StartsWith("u", StringComparison.Ordinal) ? name : "u" + name;
}
public struct VertexPositionColorTexture : IVertexType {
    public Vector2 Position;
    public Vector3 TexCoord;
    public Color Color;
    public VertexPositionColorTexture(Vector2 position, Vector3 texCoord, Color color) {
        Position = position;
        TexCoord = texCoord;
        Color = color;
    }
    public VertexDeclaration VertexDeclaration => _vertexDeclaration;
    private static readonly VertexDeclaration _vertexDeclaration = new VertexDeclaration(
        new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
        new VertexElement(8, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0),
        new VertexElement(20, VertexElementFormat.Color, VertexElementUsage.Color, 0)
    );
}
public struct VertexPositionColor : IVertexType {
    public Vector2 Position;
    public Color Color;
    public VertexPositionColor(Vector2 position, Color color) {
        Position = position;
        Color = color;
    }
    public VertexDeclaration VertexDeclaration => _vertexDeclaration;
    private static readonly VertexDeclaration _vertexDeclaration = new VertexDeclaration(
        new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
        new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0)
    );
}


public static class ColorUtils {
    /// <param name="hOffset">色相偏移 (0-1)</param>
    /// <param name="sOffset">饱和度偏移 (-1 到 1)</param>
    /// <param name="lOffset">亮度偏移 (-1 到 1)</param>
    public static Color OffsetHSL(this Color color, float hOffset, float sOffset, float lOffset) {
        // 1. 转为 Vector3 (RGB 0-1)
        Vector3 rgb = color.ToVector3();

        // 2. RGB 转 HSL
        Vector3 hsl = RgbToHsl(rgb);

        // 3. 应用偏移
        hsl.X = (hsl.X + hOffset) % 1f; // 色相是环形的
        if (hsl.X < 0) hsl.X += 1f;

        hsl.Y = MathHelper.Clamp(hsl.Y + sOffset, 0f, 1f);
        hsl.Z = MathHelper.Clamp(hsl.Z + lOffset, 0f, 1f);

        // 4. 转回 RGB 并保持原有的 Alpha
        Vector3 finalRgb = HslToRgb(hsl);
        return new Color(finalRgb.X, finalRgb.Y, finalRgb.Z) * (color.A / 255f);
    }

    public static Color ScaleSaturation(this Color color, float saturation) {
        if (saturation == 1f)
            return color;

        Vector3 hsl = RgbToHsl(color.ToVector3());
        hsl.Y = MathHelper.Clamp(hsl.Y * saturation, 0f, 1f);
        Vector3 finalRgb = HslToRgb(hsl);
        return new Color(finalRgb.X, finalRgb.Y, finalRgb.Z) * (color.A / 255f);
    }

    public static Color BoostLowSaturation(this Color color, float minSaturation, float threshold, float targetSaturation) {
        if (threshold <= minSaturation)
            return color;

        Vector3 hsl = RgbToHsl(color.ToVector3());
        if (hsl.Y <= minSaturation || hsl.Y >= threshold)
            return color;

        float t = 1f - (hsl.Y - minSaturation) / (threshold - minSaturation);
        hsl.Y = MathHelper.Lerp(hsl.Y, targetSaturation, t);

        Vector3 finalRgb = HslToRgb(hsl);
        return new Color(finalRgb.X, finalRgb.Y, finalRgb.Z) * (color.A / 255f);
    }

    public static Color WithRgbScale(this Color color, float scale) {
        if (scale == 1f)
            return color;

        byte alpha = color.A;
        color *= scale;
        color.A = alpha;
        return color;
    }

    public static Color WithAlpha(this Color color, params float[] factors) {
        float alpha = 1f;
        for (int i = 0; i < factors.Length; i++)
            alpha *= factors[i];

        color.A = (byte)(255f * alpha);
        return color;
    }

    public static Color MultiplyAlpha(this Color color, float factor) {
        color.A = (byte)(color.A * factor);
        return color;
    }

    // 辅助计算：RGB -> HSL
    private static Vector3 RgbToHsl(Vector3 rgb) {
        float max = Math.Max(rgb.X, Math.Max(rgb.Y, rgb.Z));
        float min = Math.Min(rgb.X, Math.Min(rgb.Y, rgb.Z));
        float h, s, l = (max + min) / 2f;

        if (max == min) {
            h = s = 0f; // 灰色
        } else {
            float d = max - min;
            s = l > 0.5f ? d / (2f - max - min) : d / (max + min);
            if (max == rgb.X) h = (rgb.Y - rgb.Z) / d + (rgb.Y < rgb.Z ? 6f : 0f);
            else if (max == rgb.Y) h = (rgb.Z - rgb.X) / d + 2f;
            else h = (rgb.X - rgb.Y) / d + 4f;
            h /= 6f;
        }
        return new Vector3(h, s, l);
    }

    // 辅助计算：HSL -> RGB
    private static Vector3 HslToRgb(Vector3 hsl) {
        float h = hsl.X, s = hsl.Y, l = hsl.Z;
        if (s == 0) return new Vector3(l, l, l);

        float q = l < 0.5f ? l * (1f + s) : l + s - l * s;
        float p = 2f * l - q;

        return new Vector3(
            HueToRgb(p, q, h + 1f / 3f),
            HueToRgb(p, q, h),
            HueToRgb(p, q, h - 1f / 3f)
        );
    }

    private static float HueToRgb(float p, float q, float t) {
        if (t < 0f) t += 1f;
        if (t > 1f) t -= 1f;
        if (t < 1f / 6f) return p + (q - p) * 6f * t;
        if (t < 1f / 2f) return q;
        if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
        return p;
    }
}