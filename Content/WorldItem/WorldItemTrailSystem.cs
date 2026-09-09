using GuidaSharedCode;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InventoryVisualTweaks.Content.WorldItem {
    public class WorldItemTrailSystem : ModSystem {
        private WorldItemTrailParticle[] _trails;

        public override void Load() {
            if (Main.dedServ)
                return;

            _trails = new WorldItemTrailParticle[Main.maxItems];
        }

        public override void PostUpdateEverything() {
            if (Main.gameMenu || Main.dedServ)
                return;

            SyncTrails();
        }

        public override void Unload() {
            _trails = null;
        }

        private void SyncTrails() {
            var config = ModContent.GetInstance<DrawItemInWorldConfig>();
            if (!config.EnableTrail && !config.EnableItemGlow) {
                ClearAllTrails();
                return;
            }

            if (ParticleManager.Instance == null)
                return;

            float trailIntensity = config.EnableTrail ? config.TrailIntensity / 255f : 0f;
            float attachedIntensity = config.EnableItemGlow ? config.ItemGlowIntensity / 255f : 0f;

            for (int i = 0; i < Main.maxItems; i++) {
                Item item = Main.item[i];
                WorldItemTrailParticle trail = _trails[i];

                if (trail != null && !trail.active)
                    _trails[i] = trail = null;

                if (!item.active || item.IsAir) {
                    if (trail != null && !trail.IsDissolving)
                        trail.BeginDissolve();
                    _trails[i] = null;
                    continue;
                }

                float attachedRarityMultiplier = 1f;
                float trailRarityMultiplier = 1f;
                ItemGlowStyle glowStyle = config.ItemGlowStyle;
                if (config.ItemGlowScaleWithRarity || config.TrailScaleWithRarity) {
                    float multiplier;
                    if (item.IsACoin)
                        multiplier = WorldItemVisualTuning.ItemGlowRarityCoin;
                    else if (item.master || item.rare == ItemRarityID.Master || item.expert || item.rare == ItemRarityID.Expert)
                        multiplier = WorldItemVisualTuning.ItemGlowRarityBossTier;
                    else if (item.questItem || item.rare == ItemRarityID.Quest)
                        multiplier = WorldItemVisualTuning.ItemGlowRarityQuest;
                    else {
                        float t = (item.rare - ItemRarityID.Gray) / (float)(ItemRarityID.Purple - ItemRarityID.Gray);
                        if (t < 0f)
                            t = 0f;
                        multiplier = MathHelper.Lerp(
                            WorldItemVisualTuning.ItemGlowRarityMin,
                            WorldItemVisualTuning.ItemGlowRarityMax,
                            t);
                    }

                    if (glowStyle != ItemGlowStyle.Fancy)
                        multiplier = MathHelper.Lerp(1f, multiplier, WorldItemVisualTuning.ItemGlowStyleNormalScaleBlend);

                    if (config.EnableItemGlow && config.ItemGlowScaleWithRarity)
                        attachedRarityMultiplier = multiplier;
                    if (config.EnableTrail && config.TrailScaleWithRarity)
                        trailRarityMultiplier = multiplier;
                }

                float itemAttachedIntensity = attachedIntensity * attachedRarityMultiplier;
                Color trailColor = WorldItemEffectColorHelper.PrepareBaseColor(
                    ColorSolver.ResolveUseColor(config.TrailColorBased, item));
                if (item.rare == ItemRarityID.Gray
                    || item.rare == ItemRarityID.White
                    || item.rare == ItemRarityID.Orange
                    || item.rare == ItemRarityID.Yellow)
                    trailColor *= WorldItemVisualTuning.ItemEffectColorDarken;
                bool moving = item.velocity.LengthSquared() >= WorldItemVisualTuning.MinTrailSpeedSq;

                Vector2 visualCenter = WorldItemVisualScaleHelper.GetItemVisualCenter(item);

                if (trail == null) {
                    trail = ParticleManager.Instance.NewParticle<WorldItemTrailParticle>(visualCenter, item.velocity);
                    trail.Initialize(i, trailColor, trailIntensity, itemAttachedIntensity, attachedRarityMultiplier, trailRarityMultiplier, config.ItemGlowScaleWithRarity, glowStyle);
                    _trails[i] = trail;
                } else {
                    trail.UpdateAppearance(item, trailColor, trailIntensity, itemAttachedIntensity, attachedRarityMultiplier, trailRarityMultiplier, config.ItemGlowScaleWithRarity, glowStyle);
                    trail.SyncToItem(i, visualCenter);
                    trail.RefreshLifetime();
                }

                if (config.EnableTrail && moving) {
                    trail.RecordPoint(visualCenter);
                } else if (config.EnableTrail && trail.HasTrail && !trail.IsTrailFading) {
                    trail.BeginTrailFadeOut();
                }
            }
        }

        public WorldItemTrailParticle GetTrailForItem(int itemIndex) {
            if (_trails == null || itemIndex < 0 || itemIndex >= _trails.Length)
                return null;

            return _trails[itemIndex];
        }

        private void ClearAllTrails() {
            if (_trails == null)
                return;

            for (int i = 0; i < _trails.Length; i++) {
                if (_trails[i] != null && !_trails[i].IsDissolving)
                    _trails[i].BeginDissolve();
                _trails[i] = null;
            }
        }
    }
}
