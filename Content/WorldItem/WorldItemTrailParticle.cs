using System;
using GuidaSharedCode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TrailVertex = GuidaSharedCode.VertexPositionColorTexture;

namespace InventoryVisualTweaks.Content.WorldItem {
    public class WorldItemTrailParticle : Particle {
        private Vector2[] _points;
        private Vector2[] _smoothPoints;
        private readonly TrailVertex[] _beamVertices;
        private readonly TrailVertex[] _beamBackVertices;
        private int _pointCount;
        private int _smoothPointCount;
        private Color _trailColor = Color.White;
        private float _trailIntensity = 1f;
        private float _trailRarityScale = 1f;
        private float _attachedIntensity = 1f;
        private float _attachedRarityScale = 1f;
        private float _attachedMotionAlpha = 1f;
        private float _attachedMotionScale = 1f;
        private float _beamFadeAlpha;
        private bool _hasRippleHalo;
        private bool _hasFlare;
        private bool _useRarityStarTextures;
        private ItemGlowStyle _itemGlowStyle;
        private Texture2D _starTexture;
        private bool _trailFading;
        private bool _dissolving;
        private int _trailFadeTimer;
        private float _trailFadeAlpha = 1f;
        private int _trailDissolveTimer;
        private float _trailDissolveAlpha = 1f;
        private int _attachedDissolveTimer;
        private float _attachedDissolveAlpha = 1f;
        private float _appearAlpha;
        private int _spawnFlashTimer;
        private float _spawnFlashAlpha;
        private float _sparkSpawnAccumulator;
        private float _sparkGlintAccumulator;

        public bool IsDissolving => _dissolving;
        public bool IsTrailFading => _trailFading;
        public bool HasTrail => _pointCount >= 2;
        public float SpawnFlashAlpha => _spawnFlashAlpha;
        public bool ShouldDrawSpawnFlash => !_dissolving && _spawnFlashAlpha > 0.01f;

        private float AttachedEnvelopeAlpha => _dissolving ? _attachedDissolveAlpha : _appearAlpha;
        private float AttachedDrawAlpha => AttachedEnvelopeAlpha * _attachedMotionAlpha;
        private float BeamDrawAlpha => AttachedEnvelopeAlpha * _beamFadeAlpha;
        private float TrailDrawAlpha => _dissolving ? _trailDissolveAlpha : _trailFading ? _trailFadeAlpha : _appearAlpha;

        public WorldItemTrailParticle() {
            int beamVertexCapacity = (WorldItemVisualTuning.ItemBeamSegments + 1) * 2;
            _beamVertices = new TrailVertex[beamVertexCapacity];
            _beamBackVertices = new TrailVertex[beamVertexCapacity];
        }

        public override void SetDefaults() {
            EnsureTrailBuffers();
            drawLayer = ParticleLayer.BeforeItems;
            cutOffscreen = false;
            useLighting = false;
            base.SetDefaults();
            timeLeft = WorldItemVisualTuning.TrailFadeOutTime + 4;
        }

        public void Initialize(int itemIndex, Color trailColor, float trailIntensity, float attachedIntensity, float attachedRarityScale, float trailRarityScale, bool useRarityStarTextures, ItemGlowStyle itemGlowStyle) {
            EnsureTrailBuffers();
            owner = itemIndex;
            _trailFading = false;
            _dissolving = false;
            _trailFadeTimer = 0;
            _trailDissolveTimer = 0;
            _attachedDissolveTimer = 0;
            _trailFadeAlpha = 1f;
            _trailDissolveAlpha = 1f;
            _attachedDissolveAlpha = 1f;
            _attachedMotionAlpha = WorldItemVisualTuning.ItemAttachedMovingAlpha;
            _attachedMotionScale = WorldItemVisualTuning.ItemAttachedMovingScale;
            _beamFadeAlpha = 0f;
            _sparkSpawnAccumulator = 0f;
            _sparkGlintAccumulator = 0f;
            _appearAlpha = 0f;
            _spawnFlashTimer = WorldItemVisualTuning.ItemSpawnFlashTime;
            _spawnFlashAlpha = 1f;
            _trailColor = trailColor;
            color = trailColor;
            color2 = trailColor * 0.2f;
            _trailIntensity = trailIntensity;
            _trailRarityScale = trailRarityScale;
            _attachedIntensity = attachedIntensity;
            _attachedRarityScale = attachedRarityScale;
            _useRarityStarTextures = useRarityStarTextures;
            _itemGlowStyle = itemGlowStyle;
            _starTexture = ResolveStarTexture(Main.item[itemIndex]);
            RefreshRippleHalo(Main.item[itemIndex]);
            _pointCount = 0;
            _smoothPointCount = 0;
            active = true;
            timeLeft = WorldItemVisualTuning.TrailFadeOutTime + 4;
        }

        public void SyncToItem(int itemIndex, Vector2 worldPosition) {
            if (_dissolving)
                return;

            owner = itemIndex;
            position = worldPosition;
        }

        public void UpdateAppearance(Item item, Color trailColor, float trailIntensity, float attachedIntensity, float attachedRarityScale, float trailRarityScale, bool useRarityStarTextures, ItemGlowStyle itemGlowStyle) {
            if (_dissolving)
                return;

            _trailColor = trailColor;
            color = trailColor;
            color2 = trailColor * 0.2f;
            _trailIntensity = trailIntensity;
            _trailRarityScale = trailRarityScale;
            _attachedIntensity = attachedIntensity;
            _attachedRarityScale = attachedRarityScale;
            _useRarityStarTextures = useRarityStarTextures;
            _itemGlowStyle = itemGlowStyle;
            if (item != null && item.active && !item.IsAir) {
                _starTexture = ResolveStarTexture(item);
                RefreshRippleHalo(item);
            }
            _pointCount = Math.Min(_pointCount, GetMaxTrailPoints());
        }

        public void RefreshLifetime() {
            if (!_dissolving)
                timeLeft = WorldItemVisualTuning.TrailFadeOutTime + 4;
        }

        public void BeginTrailFadeOut() {
            if (_trailFading || _dissolving || _pointCount < 2)
                return;

            _trailFading = true;
            _trailFadeTimer = WorldItemVisualTuning.TrailFadeOutTime;
            _trailFadeAlpha = 1f;
        }

        public void BeginDissolve() {
            if (_dissolving)
                return;

            _dissolving = true;
            _trailFading = true;
            owner = -1;
            _trailDissolveTimer = WorldItemVisualTuning.TrailFadeOutTime;
            _attachedDissolveTimer = WorldItemVisualTuning.ItemAttachedDissolveTime;
            _trailDissolveAlpha = 1f;
            _attachedDissolveAlpha = 1f;
            _spawnFlashAlpha = 0f;
            timeLeft = WorldItemVisualTuning.TrailFadeOutTime + 4;
        }

        public void RecordPoint(Vector2 worldPosition) {
            if (_dissolving)
                return;

            _trailFading = false;

            for (int i = _points.Length - 1; i > 0; i--)
                _points[i] = _points[i - 1];

            position = worldPosition;
            _points[0] = worldPosition;
            _pointCount = Math.Min(_pointCount + 1, GetMaxTrailPoints());
        }

        private int GetMaxTrailPoints() {
            int maxPoints = (int)Math.Round(WorldItemVisualTuning.TrailLength * _trailRarityScale);
            return Math.Clamp(maxPoints, 2, WorldItemVisualTuning.TrailLength);
        }

        public override void AI() {
            if (_dissolving) {
                _trailDissolveTimer--;
                _attachedDissolveTimer--;
                _trailDissolveAlpha = MathHelper.Clamp(_trailDissolveTimer / (float)WorldItemVisualTuning.TrailFadeOutTime, 0f, 1f);
                _attachedDissolveAlpha = MathHelper.Clamp(_attachedDissolveTimer / (float)WorldItemVisualTuning.ItemAttachedDissolveTime, 0f, 1f);

                if (Main.GameUpdateCount % 2 == 0)
                    RetractTail();

                if (_pointCount > 0)
                    _points[0] = position;

                timeLeft = _trailDissolveTimer + 2;
                if (_trailDissolveTimer <= 0)
                    Kill();
                return;
            }

            if (_appearAlpha < 1f)
                _appearAlpha = MathHelper.Clamp(
                    _appearAlpha + 1f / WorldItemVisualTuning.ItemAppearFadeTime,
                    0f,
                    1f);

            if (_spawnFlashTimer > 0) {
                _spawnFlashTimer--;
                _spawnFlashAlpha = _spawnFlashTimer / (float)WorldItemVisualTuning.ItemSpawnFlashTime;
            } else {
                _spawnFlashAlpha = 0f;
            }

            if (_trailFading) {
                _trailFadeTimer--;
                _trailFadeAlpha = MathHelper.Clamp(_trailFadeTimer / (float)WorldItemVisualTuning.TrailFadeOutTime, 0f, 1f);

                PinTrailHeadToItem();

                if (Main.GameUpdateCount % 2 == 0)
                    RetractTail();

                if (_trailFadeTimer <= 0 || _pointCount < 2)
                    _trailFading = false;
            }

            if (owner >= 0 && owner < Main.maxItems) {
                Item item = Main.item[owner];
                if (!item.active || item.IsAir)
                    BeginDissolve();
                else
                    position = WorldItemVisualScaleHelper.GetItemVisualCenter(item);
            }

            UpdateAttachedMotion();
            TrySpawnGlowSparks();
            TrySpawnGlowGlints();
            timeLeft = 3;
        }

        private void UpdateAttachedMotion() {
            if (_dissolving) {
                _attachedMotionAlpha = MathHelper.Lerp(_attachedMotionAlpha, 0f, WorldItemVisualTuning.ItemAttachedMotionLerp);
                _attachedMotionScale = MathHelper.Lerp(_attachedMotionScale, 0f, WorldItemVisualTuning.ItemAttachedMotionLerp);
                _beamFadeAlpha = MathHelper.Lerp(_beamFadeAlpha, 0f, WorldItemVisualTuning.ItemAttachedMotionLerp);
                return;
            }

            bool stationary = IsItemStationary();
            float targetAlpha = stationary ? 1f : WorldItemVisualTuning.ItemAttachedMovingAlpha;
            float targetScale = stationary ? 1f : WorldItemVisualTuning.ItemAttachedMovingScale;
            float targetBeamAlpha = stationary ? 1f : 0f;
            _attachedMotionAlpha = MathHelper.Lerp(_attachedMotionAlpha, targetAlpha, WorldItemVisualTuning.ItemAttachedMotionLerp);
            _attachedMotionScale = MathHelper.Lerp(_attachedMotionScale, targetScale, WorldItemVisualTuning.ItemAttachedMotionLerp);
            _beamFadeAlpha = MathHelper.Lerp(_beamFadeAlpha, targetBeamAlpha, WorldItemVisualTuning.ItemAttachedMotionLerp);
        }

        private void TrySpawnGlowSparks() {
            Item item = GetOwnedItem();
            if (item == null)
                return;

            bool canSpawnStreak = MeetsRarityThreshold(
                item,
                WorldItemVisualTuning.ItemGlowSparkStreakMinRarity,
                _useRarityStarTextures,
                whenScaleDisabled: false,
                _itemGlowStyle);

            TrySpawnRadialGlowParticles(
                ref _sparkSpawnAccumulator,
                WorldItemVisualTuning.ItemGlowSparkSpawnsPerFrame,
                WorldItemVisualTuning.ItemGlowSparkSpawnRadiusScale,
                ownedItem => MeetsRarityThreshold(
                    ownedItem,
                    WorldItemVisualTuning.ItemGlowSparkMinRarity,
                    _useRarityStarTextures,
                    whenScaleDisabled: true,
                    _itemGlowStyle),
                (spawnPos, glowScale) => {
                    GlowSparkVariant variant = canSpawnStreak
                        && Main.rand.NextFloat() < WorldItemVisualTuning.ItemGlowSparkStreakReplaceChance
                        ? GlowSparkVariant.Streak
                        : GlowSparkVariant.Normal;

                    WorldItemGlowSparkParticle spark = ParticleManager.Instance.NewParticle<WorldItemGlowSparkParticle>(spawnPos, Vector2.Zero);
                    spark?.Configure(
                        _trailColor,
                        glowScale,
                        _attachedIntensity * AttachedDrawAlpha,
                        variant);
                });
        }

        private void TrySpawnGlowGlints() {
            TrySpawnRadialGlowParticles(
                ref _sparkGlintAccumulator,
                WorldItemVisualTuning.ItemGlowGlintSpawnsPerFrame,
                WorldItemVisualTuning.ItemGlowGlintSpawnRadiusScale,
                ownedItem => MeetsRarityThreshold(
                    ownedItem,
                    WorldItemVisualTuning.ItemGlowGlintMinRarity,
                    _useRarityStarTextures,
                    whenScaleDisabled: false,
                    _itemGlowStyle),
                (spawnPos, _) => {
                    WorldItemGlowGlintParticle glint = ParticleManager.Instance.NewParticle<WorldItemGlowGlintParticle>(spawnPos, Vector2.Zero);
                    glint?.Configure(
                        _trailColor,
                        _attachedIntensity * AttachedDrawAlpha,
                        _itemGlowStyle);
                });
        }

        private void TrySpawnRadialGlowParticles(
            ref float accumulator,
            float spawnsPerFrame,
            float spawnRadiusScale,
            Func<Item, bool> rarityGate,
            Action<Vector2, float> spawnAt) {
            if (_dissolving || _attachedIntensity < 0.01f || _appearAlpha < 0.5f || !IsItemStationary())
                return;

            var config = ModContent.GetInstance<DrawItemInWorldConfig>();
            if (!config.EnableItemGlow || !config.EnableExtraParticles)
                return;

            Item item = GetOwnedItem();
            if (item == null || !rarityGate(item))
                return;

            float itemEffectScale = GetAttachedEffectScale();
            float glowScale = WorldItemVisualScaleHelper.GetGlowWorldRadius(itemEffectScale)
                / WorldItemVisualTuning.ItemGlowSparkReferenceGlowRadius;

            accumulator += spawnsPerFrame
                * glowScale
                * (_itemGlowStyle == ItemGlowStyle.Fancy
                    ? 1f
                    : WorldItemVisualTuning.ItemGlowStyleNormalExtraSpawnScale);

            int spawnCount = (int)accumulator;
            if (spawnCount <= 0)
                return;

            accumulator -= spawnCount;
            if (ParticleManager.Instance == null)
                return;

            WorldItemVisualScaleHelper.GetGlowWorldRadii(itemEffectScale, out float radiusX, out float radiusY);
            radiusX *= spawnRadiusScale;
            radiusY *= spawnRadiusScale;

            Vector2 center = GetHeadWorldPosition();
            for (int i = 0; i < spawnCount; i++)
                spawnAt(GetRadialSpawnPosition(center, radiusX, radiusY), glowScale);
        }

        private static Vector2 GetRadialSpawnPosition(Vector2 center, float radiusX, float radiusY) {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            float radial = MathF.Sqrt(Main.rand.NextFloat());
            return center + new Vector2(
                MathF.Cos(angle) * radial * radiusX,
                MathF.Sin(angle) * radial * radiusY);
        }

        private bool IsItemStationary() {
            if (owner < 0 || owner >= Main.maxItems)
                return false;

            Item item = Main.item[owner];
            return item.active && !item.IsAir && item.velocity.LengthSquared() < WorldItemVisualTuning.MinTrailSpeedSq;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            float attachedAlpha = AttachedDrawAlpha;
            float beamAlpha = BeamDrawAlpha;
            bool drawAttached = _attachedIntensity > 0.01f && attachedAlpha > 0.01f;
            bool drawBeam = drawAttached && beamAlpha > 0.01f;
            bool drawTrail = _trailIntensity > 0.01f && HasTrail && (!_trailFading || _trailFadeAlpha > 0.01f);

            if (!drawAttached && !drawTrail)
                return false;

            EnsureTrailBuffers();

            Vector2 headWorld = GetHeadWorldPosition();
            TrailVertex[] baseVertices = null;
            TrailVertex[] highlightVertices = null;
            int vertexCount = 0;

            if (drawTrail) {
                BuildTrailSmoothPoints();

                if (_smoothPointCount >= 2) {
                    vertexCount = _smoothPointCount * 2;
                    baseVertices = new TrailVertex[vertexCount];
                    highlightVertices = new TrailVertex[vertexCount];
                    float trailAlpha = TrailDrawAlpha;

                    BuildTrailVertices(baseVertices, trailAlpha, 1f, WorldItemVisualTuning.TrailBaseDarkness, WorldItemVisualTuning.TrailBaseAlpha, false);
                    BuildTrailVertices(highlightVertices, trailAlpha, 1f, 1f, 1f, true);
                } else {
                    drawTrail = false;
                }
            }

            int beamVertexCount = 0;
            int beamSwipeVertexCount = 0;
            Item ownedItem = GetOwnedItem();
            bool drawBeamSwipe = ownedItem != null
                && MeetsRarityThreshold(
                    ownedItem,
                    WorldItemVisualTuning.ItemBeamSwipeMinRarity,
                    _useRarityStarTextures,
                    whenScaleDisabled: true,
                    _itemGlowStyle);
            if (drawBeam) {
                if (drawBeamSwipe)
                    beamSwipeVertexCount = BuildBeamSwipeVertices(headWorld, beamAlpha);
                beamVertexCount = BuildBeamVertices(headWorld, beamAlpha, false);
            }

            spriteBatch.EndAndBeginImmediate(BlendState.NonPremultiplied);
            if (drawAttached) {
                DrawItemGlow(spriteBatch, headWorld, attachedAlpha, false);
                if (_hasRippleHalo)
                    DrawItemRippleHalo(spriteBatch, headWorld, attachedAlpha, false, BlendState.NonPremultiplied);
            }
            if (drawBeam && beamVertexCount >= 4)
                DrawTrailPrimitives(ModAsset.TexItemTrail.Value, _beamVertices, beamVertexCount);
            if (drawTrail && baseVertices != null)
                DrawTrailPrimitives(ModAsset.TexItemTrail.Value, baseVertices, vertexCount);

            spriteBatch.EndAndBeginImmediate(BlendState.Additive);
            if (drawAttached) {
                DrawItemGlow(spriteBatch, headWorld, attachedAlpha, true);
                if (_hasRippleHalo)
                    DrawItemRippleHalo(spriteBatch, headWorld, attachedAlpha, true, BlendState.Additive);
                if (_hasFlare)
                    DrawItemFlare(spriteBatch, headWorld, attachedAlpha);
                if (drawBeam) {
                    if (beamSwipeVertexCount >= 4)
                        DrawBeamSwipe(spriteBatch, ModAsset.TexItemBeamDisturb.Value, _beamBackVertices, beamSwipeVertexCount);
                    beamVertexCount = BuildBeamVertices(headWorld, beamAlpha, true);
                    if (beamVertexCount >= 4)
                        DrawTrailPrimitives(ModAsset.TexItemTrail.Value, _beamVertices, beamVertexCount);
                }
                DrawItemStar(spriteBatch, headWorld, attachedAlpha);
            }
            
            if (drawTrail && highlightVertices != null)
                DrawTrailPrimitives(ModAsset.TexItemTrail.Value, highlightVertices, vertexCount);

            spriteBatch.EndAndBeginWorld();

            return false;
        }

        private Item GetOwnedItem() {
            if (_dissolving || owner < 0 || owner >= Main.maxItems)
                return null;

            Item item = Main.item[owner];
            if (!item.active || item.IsAir)
                return null;

            return item;
        }

        private Vector2 GetHeadWorldPosition() {
            if (!_dissolving && owner >= 0 && owner < Main.maxItems) {
                Item item = Main.item[owner];
                if (item.active && !item.IsAir)
                    return WorldItemVisualScaleHelper.GetItemVisualCenter(item);
            }

            return position;
        }

        private float GetItemEffectScale() {
            if (_dissolving || owner < 0 || owner >= Main.maxItems)
                return 1f;

            Item item = Main.item[owner];
            if (!item.active || item.IsAir)
                return 1f;

            return WorldItemVisualScaleHelper.GetEffectScale(item);
        }

        private float GetAttachedEffectScale() => GetItemEffectScale() * _attachedRarityScale * _attachedMotionScale;

        private void DrawItemGlow(SpriteBatch spriteBatch, Vector2 headWorld, float globalAlpha, bool highlight) {
            float itemEffectScale = GetAttachedEffectScale();
            float scalePulse = highlight
                ? 1f - WorldItemVisualTuning.ItemGlowPulseAmplitude
                    + WorldItemVisualTuning.ItemGlowPulseAmplitude * MathF.Sin((float)Main.timeForVisualEffects * WorldItemVisualTuning.ItemGlowPulseSpeed + owner * 0.37f)
                : 1f;
            Vector2 scale = new Vector2(
                WorldItemVisualTuning.ItemGlowScale * itemEffectScale * scalePulse * WorldItemVisualTuning.ItemGlowFlatScaleX / ModAsset.TexItemGlow.Value.Width,
                WorldItemVisualTuning.ItemGlowScale * itemEffectScale * scalePulse * WorldItemVisualTuning.ItemGlowFlatScaleY / ModAsset.TexItemGlow.Value.Height);
            if (!highlight)
                scale *= WorldItemVisualTuning.ItemGlowBaseScaleBoost;

            Color drawColor = WorldItemEffectColorHelper.ForGlow(_trailColor, highlight);
            if (highlight)
                drawColor = drawColor.WithAlpha(_attachedIntensity, globalAlpha, WorldItemVisualTuning.ItemGlowHighlightAlpha);
            else
                drawColor = drawColor
                    .WithRgbScale(WorldItemVisualTuning.ItemGlowBaseDarkness)
                    .WithAlpha(_attachedIntensity, globalAlpha, WorldItemVisualTuning.ItemGlowBaseAlpha);

            spriteBatch.DrawCentered(ModAsset.TexItemGlow.Value, headWorld - Main.screenPosition, drawColor, scale);
        }

        private void RefreshRippleHalo(Item item) {
            _hasRippleHalo = item != null && item.active && !item.IsAir && MeetsRarityThreshold(
                item,
                WorldItemVisualTuning.ItemRippleHaloMinRarity,
                scaleWithRarity: true,
                whenScaleDisabled: true,
                _itemGlowStyle);
            _hasFlare = item != null && item.active && !item.IsAir && MeetsRarityThreshold(
                item,
                WorldItemVisualTuning.ItemGlowFlareMinRarity,
                _useRarityStarTextures,
                whenScaleDisabled: false,
                _itemGlowStyle);
        }

        private void DrawItemFlare(SpriteBatch spriteBatch, Vector2 headWorld, float globalAlpha) {
            float drawScale = WorldItemVisualTuning.ItemGlowFlareScale * GetAttachedEffectScale() / ModAsset.TexItemFlare.Value.Width;
            Vector2 screen = headWorld - Main.screenPosition;

            Color drawColor = Color.Lerp(
                WorldItemEffectColorHelper.ForGlow(_trailColor, true),
                Color.White,
                WorldItemVisualTuning.ItemGlowFlareWhiten)
                .WithAlpha(_attachedIntensity, globalAlpha, WorldItemVisualTuning.ItemGlowFlareAlpha);

            float rotation1 = (float)Main.timeForVisualEffects * WorldItemVisualTuning.ItemGlowFlareRotationSpeed;
            spriteBatch.DrawCentered(ModAsset.TexItemFlare.Value, screen, drawColor, rotation1, drawScale);
            spriteBatch.DrawCentered(ModAsset.TexItemFlare.Value, screen, drawColor, -rotation1, drawScale);
        }

        private void DrawItemRippleHalo(SpriteBatch spriteBatch, Vector2 headWorld, float globalAlpha, bool highlight, BlendState blendState) {
            Effect effect = ModAsset.ItemRippleBloom.Value;

            float scaleValue = WorldItemVisualTuning.ItemRippleBloomScale * GetAttachedEffectScale() / ModAsset.TexItemSolidBloom.Value.Width;
            Vector2 scale = new Vector2(scaleValue, scaleValue);

            Color tint = WorldItemEffectColorHelper.ForRipple(_trailColor, highlight);
            if (!highlight)
                tint = tint.WithRgbScale(WorldItemVisualTuning.ItemRippleBloomBaseDarkness);

            float alphaMultiplier = highlight
                ? WorldItemVisualTuning.ItemRippleBloomHighlightAlpha
                : WorldItemVisualTuning.ItemRippleBloomBaseAlpha;
            float colorMix = highlight ? 1f : WorldItemVisualTuning.ItemRippleBloomColorMix;
            float time = (float)Main.timeForVisualEffects / 60f * WorldItemVisualTuning.ItemRippleBloomSpeed + owner * 0.17f;

            spriteBatch.End();
            spriteBatch.Begin(
                SpriteSortMode.Immediate,
                blendState,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                effect,
                Main.GameViewMatrix.TransformationMatrix);

            effect
                .SetTime(time)
                .Set("RingSpeed", WorldItemVisualTuning.ItemRippleBloomRingSpeed)
                .Set("RingCount", WorldItemVisualTuning.ItemRippleBloomRingCount)
                .Set("RingWidth", WorldItemVisualTuning.ItemRippleBloomRingWidth)
                .SetColor(tint)
                .Set("ColorMix", colorMix)
                .SetOpacity(_attachedIntensity * globalAlpha * alphaMultiplier)
                .Apply();

            Vector2 screen = headWorld - Main.screenPosition;
            spriteBatch.DrawCentered(ModAsset.TexItemSolidBloom.Value, screen, Color.White, scale);

            spriteBatch.End();
            spriteBatch.Begin(
                SpriteSortMode.Immediate,
                blendState,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        private int BuildBeamVertices(Vector2 headWorld, float globalAlpha, bool highlight) {
            int segmentCount = WorldItemVisualTuning.ItemBeamSegments;
            float itemEffectScale = GetAttachedEffectScale();
            float widthMultiplier = highlight ? 1f : WorldItemVisualTuning.ItemBeamBaseWidthScale;
            float colorMultiplier = highlight ? 1f : WorldItemVisualTuning.ItemBeamBaseDarkness;
            float alphaMultiplier = highlight ? WorldItemVisualTuning.ItemBeamHighlightAlpha : WorldItemVisualTuning.ItemBeamBaseAlpha;

            float halfWidth = WorldItemVisualTuning.ItemBeamWidth * itemEffectScale * widthMultiplier * 0.5f;
            float beamHeight = WorldItemVisualTuning.ItemBeamHeight * itemEffectScale * globalAlpha;
            Vector2 screenBase = headWorld - Main.screenPosition;

            for (int i = 0; i <= segmentCount; i++) {
                float tailness = i / (float)segmentCount;
                float headness = 1f - tailness;
                float width = halfWidth * MathHelper.Lerp(WorldItemVisualTuning.TrailTailWidthScale, 1f, headness);
                float brightness = MathF.Pow(headness, WorldItemVisualTuning.TrailBrightnessFalloffPower);

                float rootFactor = GetBeamRootFactor(i);
                width *= MathHelper.Lerp(WorldItemVisualTuning.ItemBeamRootMinWidthScale, 1f, rootFactor);
                brightness *= MathHelper.Lerp(WorldItemVisualTuning.ItemBeamRootMinBrightness, 1f, rootFactor);

                Color drawColor = Color.Lerp(
                    WorldItemEffectColorHelper.ForBeam(_trailColor, highlight),
                    WorldItemEffectColorHelper.ForBeamTail(color2, highlight),
                    tailness * WorldItemVisualTuning.TrailTailColorMix)
                    .WithRgbScale(colorMultiplier)
                    .WithAlpha(_attachedIntensity, brightness, globalAlpha, alphaMultiplier);

                Vector2 center = screenBase - new Vector2(0f, beamHeight * tailness);
                _beamVertices[i * 2] = new TrailVertex(
                    center + new Vector2(-width, 0f),
                    new Vector3(0.5f, 0.15f, 1f),
                    drawColor);
                _beamVertices[i * 2 + 1] = new TrailVertex(
                    center + new Vector2(width, 0f),
                    new Vector3(0.5f, 0.85f, 1f),
                    drawColor);
            }

            return (segmentCount + 1) * 2;
        }

        private int BuildBeamSwipeVertices(Vector2 headWorld, float globalAlpha) {
            int segmentCount = WorldItemVisualTuning.ItemBeamSegments;
            float itemEffectScale = GetAttachedEffectScale();
            float halfWidth = WorldItemVisualTuning.ItemBeamWidth * itemEffectScale * WorldItemVisualTuning.ItemBeamBackWidthScale * 0.5f;
            float beamHeight = WorldItemVisualTuning.ItemBeamHeight * itemEffectScale * WorldItemVisualTuning.ItemBeamSwipeHeightScale * globalAlpha;
            Vector2 screenBase = headWorld - Main.screenPosition;

            Color drawColor = WorldItemEffectColorHelper.ForBeamSwipe(_trailColor)
                .WithAlpha(_attachedIntensity, globalAlpha, WorldItemVisualTuning.ItemBeamSwipeAlpha);

            for (int i = 0; i <= segmentCount; i++) {
                float tailness = i / (float)segmentCount;

                float textureV = 1f - tailness;

                Vector2 center = screenBase - new Vector2(0f, beamHeight * tailness);
                _beamBackVertices[i * 2] = new TrailVertex(
                    center + new Vector2(-halfWidth, 0f),
                    new Vector3(0f, textureV, 1f),
                    drawColor);
                _beamBackVertices[i * 2 + 1] = new TrailVertex(
                    center + new Vector2(halfWidth, 0f),
                    new Vector3(1f, textureV, 1f),
                    drawColor);
            }

            return (segmentCount + 1) * 2;
        }

        private void DrawBeamSwipe(SpriteBatch spriteBatch, Texture2D swipeTexture, TrailVertex[] vertices, int vertexCount) {
            Effect effect = ModAsset.ItemBeamSwipe.Value;
            float time = (float)Main.timeForVisualEffects / 60f;
            float phase = owner * WorldItemVisualTuning.ItemBeamSwipeSliceWidth;

            spriteBatch.End();
            spriteBatch.Begin(
                SpriteSortMode.Immediate,
                BlendState.Additive,
                SamplerState.LinearWrap,
                DepthStencilState.None,
                RasterizerState.CullNone,
                effect,
                Main.GameViewMatrix.TransformationMatrix);
            effect
                .SetTime(time)
                .Set("ScrollSpeed", WorldItemVisualTuning.ItemBeamSwipeScrollSpeed)
                .Set("ScrollSpeed2", -WorldItemVisualTuning.ItemBeamSwipeScrollSpeed2)
                .Set("ScrollPhase", phase)
                .Set("SliceWidth", WorldItemVisualTuning.ItemBeamSwipeSliceWidth)
                .Set("SliceWidth2", WorldItemVisualTuning.ItemBeamSwipeSliceWidth2)
                .Set("EdgeSoftness", WorldItemVisualTuning.ItemBeamSwipeEdgeSoftness)
                .Set("RootSoftness", WorldItemVisualTuning.ItemBeamSwipeRootSoftness)
                .Set("RootMinBrightness", WorldItemVisualTuning.ItemBeamSwipeRootMinBrightness)
                .Set("Layer2Strength", WorldItemVisualTuning.ItemBeamSwipeLayer2Strength)
                .Apply();

            GraphicsDevice graphicsDevice = Main.graphics.GraphicsDevice;
            graphicsDevice.Textures[0] = swipeTexture;
            graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertexCount - 2);

            spriteBatch.End();
            spriteBatch.Begin(
                SpriteSortMode.Immediate,
                BlendState.Additive,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawItemStar(SpriteBatch spriteBatch, Vector2 headWorld, float globalAlpha) {
            float itemEffectScale = GetAttachedEffectScale();
            float pulse = 0.95f + 0.05f * MathF.Sin((float)Main.timeForVisualEffects * WorldItemVisualTuning.ItemStarPulseSpeed + owner * 0.53f);
            Vector2 scale = new Vector2(
                WorldItemVisualTuning.ItemStarScale * itemEffectScale * pulse * WorldItemVisualTuning.ItemStarFlatScaleX / _starTexture.Width,
                WorldItemVisualTuning.ItemStarScale * itemEffectScale * pulse * WorldItemVisualTuning.ItemStarFlatScaleY / _starTexture.Height);

            Color drawColor = Color.Lerp(
                WorldItemEffectColorHelper.ForStar(_trailColor),
                Color.White,
                WorldItemVisualTuning.ItemStarWhiten)
                .WithAlpha(_attachedIntensity, globalAlpha, WorldItemVisualTuning.ItemStarAlpha);

            spriteBatch.DrawCentered(_starTexture, headWorld - Main.screenPosition, drawColor, scale);
        }

        private void BuildTrailVertices(
            TrailVertex[] vertices,
            float globalAlpha,
            float widthMultiplier,
            float colorMultiplier,
            float alphaMultiplier,
            bool highlight) {
            float itemEffectScale = GetItemEffectScale() * _trailRarityScale;

            for (int i = 0; i < _smoothPointCount; i++) {
                float tailness = _smoothPointCount <= 1 ? 0f : i / (_smoothPointCount - 1f);
                float headness = 1f - tailness;

                float widthScale = MathHelper.Lerp(
                    WorldItemVisualTuning.TrailTailWidthScale,
                    1f,
                    MathF.Pow(headness, WorldItemVisualTuning.TrailWidthFalloffPower));
                float brightness = MathF.Pow(headness, WorldItemVisualTuning.TrailBrightnessFalloffPower);

                float headAttach = GetHeadAttachFactor(i);
                widthScale *= MathHelper.Lerp(WorldItemVisualTuning.TrailHeadAttachMinWidthScale, 1f, headAttach);
                brightness *= MathHelper.Lerp(WorldItemVisualTuning.TrailHeadAttachMinBrightness, 1f, headAttach);

                float width = WorldItemVisualTuning.TrailWidth * itemEffectScale * widthScale * widthMultiplier;

                Vector2 tangent = GetSmoothTangent(i);
                Vector2 normal = tangent.RotatedBy(MathHelper.PiOver2);

                Color drawColor = Color.Lerp(
                    WorldItemEffectColorHelper.ForTrail(_trailColor, highlight),
                    WorldItemEffectColorHelper.ForTrailTail(color2),
                    tailness * WorldItemVisualTuning.TrailTailColorMix)
                    .WithRgbScale(colorMultiplier)
                    .WithAlpha(_trailIntensity, brightness, globalAlpha, alphaMultiplier);

                Vector2 screen = _smoothPoints[i] - Main.screenPosition;
                vertices[i * 2] = new TrailVertex(
                    screen + normal * width,
                    new Vector3(0.5f, 0.15f, 1f),
                    drawColor);
                vertices[i * 2 + 1] = new TrailVertex(
                    screen - normal * width,
                    new Vector3(0.5f, 0.85f, 1f),
                    drawColor);
            }
        }

        private static void DrawTrailPrimitives(Texture2D trailTexture, TrailVertex[] vertices, int vertexCount, SamplerState sampler = null) {
            GraphicsDevice graphicsDevice = Main.graphics.GraphicsDevice;
            graphicsDevice.Textures[0] = trailTexture;
            graphicsDevice.SamplerStates[0] = sampler ?? SamplerState.LinearClamp;
            graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertexCount - 2);
        }

        private void PinTrailHeadToItem() {
            if (_dissolving || owner < 0 || owner >= Main.maxItems)
                return;

            Item item = Main.item[owner];
            if (!item.active || item.IsAir || _pointCount <= 0)
                return;

            _points[0] = WorldItemVisualScaleHelper.GetItemVisualCenter(item);
        }

        private void BuildTrailSmoothPoints() {
            if (ShouldUseRawTrailPoints()) {
                _smoothPointCount = _pointCount;
                for (int i = 0; i < _pointCount; i++)
                    _smoothPoints[i] = _points[i];
                return;
            }

            _smoothPointCount = TrailSplineHelper.BuildSmoothPoints(
                _points,
                _pointCount,
                WorldItemVisualTuning.TrailSplineSamplesPerSegment,
                WorldItemVisualTuning.TrailBezierTension,
                _smoothPoints);
        }

        private bool ShouldUseRawTrailPoints() => _trailFading || _dissolving;

        private void EnsureTrailBuffers() {
            int length = WorldItemVisualTuning.TrailLength;
            int smoothMax = WorldItemVisualTuning.TrailMaxSmoothPoints;
            if (_points == null || _points.Length != length)
                _points = new Vector2[length];
            if (_smoothPoints == null || _smoothPoints.Length != smoothMax)
                _smoothPoints = new Vector2[smoothMax];
        }

        private void RetractTail() {
            if (_pointCount <= 1)
                return;

            _pointCount--;
        }

        private float GetHeadAttachFactor(int index) {
            int affectedPoints = WorldItemVisualTuning.TrailHeadSoftnessPoints;
            if (index >= affectedPoints)
                return 1f;

            float t = (index + 1f) / (affectedPoints + 1f);
            return t * t * (3f - 2f * t);
        }

        private static float GetBeamRootFactor(int index) {
            int affectedSegments = WorldItemVisualTuning.ItemBeamRootSoftnessSegments;
            if (index >= affectedSegments)
                return 1f;

            float t = (index + 1f) / (affectedSegments + 1f);
            return t * t * (3f - 2f * t);
        }

        private Vector2 GetSmoothTangent(int index) {
            Vector2 tangent;
            if (index < _smoothPointCount - 1)
                tangent = _smoothPoints[index] - _smoothPoints[index + 1];
            else
                tangent = _smoothPoints[index - 1] - _smoothPoints[index];

            if (tangent.LengthSquared() < 0.01f && !_dissolving && owner >= 0 && owner < Main.maxItems)
                tangent = Main.item[owner].velocity;

            if (tangent.LengthSquared() < 0.0001f)
                tangent = Vector2.UnitY;

            return Vector2.Normalize(tangent);
        }

        private Texture2D ResolveStarTexture(Item item) {
            if (!_useRarityStarTextures)
                return ModAsset.TexItemLightStar1.Value;

            if (item.IsACoin)
                return ModAsset.TexItemLightStar1.Value;

            if (item.master || item.rare == ItemRarityID.Master)
                return ModAsset.TexItemLightStar4.Value;

            if (item.expert || item.rare == ItemRarityID.Expert)
                return ModAsset.TexItemLightStar4.Value;

            if (item.questItem || item.rare == ItemRarityID.Quest)
                return ModAsset.TexItemLightStar3.Value;

            if (item.rare <= ItemRarityID.White)
                return ModAsset.TexItemLightStar1.Value;

            if (item.rare == ItemRarityID.Blue)
                return ModAsset.TexItemLightStar2.Value;

            if (item.rare == ItemRarityID.Green || item.rare == ItemRarityID.Orange)
                return ModAsset.TexItemLightStar3.Value;

            return ModAsset.TexItemLightStar4.Value;
        }

        private bool MeetsRarityThreshold(Item item, int threshold, bool scaleWithRarity, bool whenScaleDisabled, ItemGlowStyle style) {
            if (!scaleWithRarity)
                return whenScaleDisabled;

            return GetEffectRarityTier(item) >= GetStyleAdjustedRarityThreshold(threshold, style);
        }

        private static int GetEffectRarityTier(Item item) {
            if (item.master || item.rare == ItemRarityID.Master)
                return ItemRarityID.Purple;

            if (item.expert || item.rare == ItemRarityID.Expert)
                return ItemRarityID.Purple;

            if (item.questItem || item.rare == ItemRarityID.Quest)
                return ItemRarityID.Green;

            return item.rare;
        }

        private static int GetStyleAdjustedRarityThreshold(int fancyThreshold, ItemGlowStyle style) {
            if (style == ItemGlowStyle.Fancy)
                return fancyThreshold;

            int adjusted = (int)MathF.Ceiling(fancyThreshold * WorldItemVisualTuning.ItemGlowStyleNormalThresholdRatio);
            return Math.Min(ItemRarityID.Purple, adjusted);
        }
    }
}
