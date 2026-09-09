using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.WorldBuilding;

namespace GuidaSharedCode {
    public class CameraModifier {
        public Vector2 EndpointCenter;
        public float CurrentMultiplier = 0f;
        public float TargetMultiplier = 0f;
        public float TargetZoom = 1f;
        public object Owner;

        public float MaxDistance = 1000f;           // 最大作用距离
        public float LerpMultiplier = 0.05f;        // lerp乘数
        public float MaxSpeed = 0.02f;              // 最大速度限制

        public void UpdateMultiplier() {
            var NewMultiplier = MathHelper.Lerp(CurrentMultiplier, TargetMultiplier, LerpMultiplier);
            float difference = NewMultiplier - CurrentMultiplier;
            if (Math.Abs(difference) > MaxSpeed) {
                CurrentMultiplier += Math.Sign(difference) * MaxSpeed;
            } else {
                CurrentMultiplier = NewMultiplier;
            }
        }

        public bool ShouldRemove() {
            return TargetMultiplier == 0f && Math.Abs(CurrentMultiplier) < 0.001f;
        }

        public bool IsInRange(Vector2 playerCenter) {
            return Vector2.Distance(playerCenter, EndpointCenter) <= MaxDistance;
        }

        public void SetParameters(float maxDistance = 1000f, float lerpMultiplier = 0.05f, float maxSpeed = 0.02f) {
            MaxDistance = maxDistance;
            LerpMultiplier = lerpMultiplier;
            MaxSpeed = maxSpeed;
        }
    }

    public class CameraModifySystem : ModSystem {
        private List<CameraModifier> modifiers = new List<CameraModifier>();
        private float subZoom = 0;

        public override void ModifyScreenPosition() {
            if (modifiers.Count == 0) return;

            Player localPlayer = Main.LocalPlayer;
            if (localPlayer == null) return;

            Vector2 playerScreenCenter = localPlayer.Center;
            Vector2 currentScreenPosition = playerScreenCenter - new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;
            currentScreenPosition = Main.screenPosition;
            for (int i = modifiers.Count - 1; i >= 0; i--) {
                if (!modifiers[i].IsInRange(playerScreenCenter)) modifiers[i].TargetMultiplier = 0f;
                modifiers[i].UpdateMultiplier();
                if (modifiers[i].ShouldRemove()) {
                    modifiers.RemoveAt(i);
                }
            }
            float sumValue = 1;
            Main.GameZoomTarget += subZoom;
            float originZoom = Main.GameZoomTarget;
            float currentZoom = Main.GameZoomTarget;
            foreach (var modifier in modifiers) {
                if (Math.Abs(modifier.CurrentMultiplier) > 0.001f) {
                    Vector2 endpointScreenPosition = modifier.EndpointCenter - new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;
                    float newValue = modifier.CurrentMultiplier / (1 - modifier.CurrentMultiplier);
                    sumValue += newValue;
                    currentScreenPosition = Vector2.Lerp(currentScreenPosition, endpointScreenPosition, newValue / sumValue);
                    currentZoom = MathHelper.Lerp(currentZoom, originZoom * modifier.TargetZoom, newValue / sumValue);
                }
            }
            foreach (var modifier in modifiers) {
                modifier.TargetMultiplier = 0f;
            }
            subZoom = originZoom - currentZoom;
            Main.GameZoomTarget = currentZoom;
            currentScreenPosition.X = MathHelper.Clamp(currentScreenPosition.X, 0, Main.maxTilesX * 16 - Main.screenWidth);
            currentScreenPosition.Y = MathHelper.Clamp(currentScreenPosition.Y, 0, Main.maxTilesY * 16 - Main.screenHeight);
            Main.screenPosition = currentScreenPosition;
        }

        public CameraModifier AddOrGetModifier(object owner) {
            var existing = modifiers.FirstOrDefault(m => m.Owner == owner);
            if (existing != null) return existing;
            var newModifier = new CameraModifier { Owner = owner };
            modifiers.Add(newModifier);
            return newModifier;
        }

        public CameraModifier AddOrGetModifier(object owner, float maxDistance = 1800f, float lerpMultiplier = 0.05f, float maxSpeed = 0.02f) {
            var modifier = AddOrGetModifier(owner);
            modifier.SetParameters(maxDistance, lerpMultiplier, maxSpeed);
            return modifier;
        }

        public void RemoveModifier(object owner) {
            for (int i = modifiers.Count - 1; i >= 0; i--) {
                if (modifiers[i].Owner == owner) {
                    modifiers.RemoveAt(i);
                    break;
                }
            }
        }
    }
}