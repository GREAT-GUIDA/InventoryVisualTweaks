using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Reflection;
using Terraria.Audio;
using Terraria.ModLoader;

namespace GuidaSharedCode {
    // 自定义属性
    [AttributeUsage(AttributeTargets.Field)]
    public class AssetAttribute : Attribute {
        public string Path { get; }
        public AssetAttribute(string path) {
            Path = path;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class SoundAssetAttribute : AssetAttribute {
        public float Volume { get; set; } = 1f;
        public float PitchVariance { get; set; } = 0f;
        public int MaxInstances { get; set; } = 1;

        public SoundAssetAttribute(string path) : base(path) { }
    }
}
