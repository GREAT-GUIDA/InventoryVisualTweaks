namespace InventoryVisualTweaks.Content.WorldItem {

    internal static class WorldItemVisualTuning {

        public static float LusterSpeed = 1f;

        public static float LusterFrequency = 0.5f;

        public static float LusterRotation = 0.8f;

        public static float LusterWidth = 0.2f;

        public static float OutlineOffset = 2f;

        public static float HighlightBaseDarkness = 0.25f;

        public static float HighlightBaseAlpha = 0.45f;



        public static int TrailLength = 32;

        public static float TrailWidth = 11f;

        public static float MinTrailSpeed = 0.2f;

        public static float MinTrailSpeedSq => MinTrailSpeed * MinTrailSpeed;

        public static float TrailTailWidthScale = 0.12f;

        public static float TrailWidthFalloffPower = 1.35f;

        public static float TrailBrightnessFalloffPower = 1.75f;

        public static float TrailTailColorMix = 0.55f;

        public static int TrailHeadSoftnessPoints = 2;

        public static float TrailHeadAttachMinWidthScale = 0.4f;

        public static float TrailHeadAttachMinBrightness = 0.3f;

        public static float TrailBaseDarkness = 0.25f;

        public static float TrailBaseAlpha = 0.25f;

        public static int TrailFadeOutTime = 50;

        public static int ItemAttachedDissolveTime = 14;

        public static int ItemAppearFadeTime = 18;

        public static int ItemSpawnFlashTime = 8;

        public static float ItemSpawnFlashIntensity = 0.55f;

        public static int TrailSplineSamplesPerSegment = 3;

        public static float TrailBezierTension = 0.35f;

        public static int TrailMaxSmoothPoints => (TrailLength - 1) * TrailSplineSamplesPerSegment + 1;



        public static float ItemGlowScale = 64f;

        public static float ItemGlowPulseSpeed = 0.08f;

        public static float ItemGlowPulseAmplitude = 0.06f;

        public static float ItemGlowFlatScaleX = 0.98f;

        public static float ItemGlowFlatScaleY = 0.60f;

        public static float ItemGlowBaseDarkness = 0.45f;

        public static float ItemGlowBaseAlpha = 0.38f;

        public static float ItemGlowBaseScaleBoost = 0.75f;

        public static float ItemGlowHighlightAlpha = 0.20f;



        public static float ItemBeamWidth = 18f;

        public static float ItemBeamHeight = 88f;

        public static int ItemBeamSegments = 11;

        public static int ItemBeamRootSoftnessSegments = 3;

        public static float ItemBeamRootMinWidthScale = 0.35f;

        public static float ItemBeamRootMinBrightness = 0.28f;

        public static float ItemAttachedMotionLerp = 0.14f;

        public static float ItemAttachedMovingScale = 0.7f;

        public static float ItemAttachedMovingAlpha = 0.72f;

        public static float ItemBeamBaseDarkness = 0.35f;

        public static float ItemBeamBaseAlpha = 0.34f;

        public static float ItemBeamBaseWidthScale = 1.00f;

        public static float ItemBeamHighlightAlpha = 1.05f;

        public static float ItemBeamBackWidthScale = 0.828f;

        public static float ItemBeamSwipeHeightScale = 1.83f;

        public static float ItemBeamSwipeAlpha = 0.38f;

        public static float ItemBeamSwipeScrollSpeed = 0.02f;

        public static float ItemBeamSwipeSliceWidth = 0.08f;

        public static float ItemBeamSwipeEdgeSoftness = 0.264f;

        public static float ItemBeamSwipeScrollSpeed2 = 0.084f;

        public static float ItemBeamSwipeSliceWidth2 = 0.078f;

        public static float ItemBeamSwipeLayer2Strength = 0.72f;

        public static float ItemBeamSwipeRootSoftness = 0.11f;

        public static float ItemBeamSwipeRootMinBrightness = 0.52f;



        public static float ItemStarScale = 50f;

        public static float ItemStarFlatScaleX = 1.42f;

        public static float ItemStarFlatScaleY = 1.3f;

        public static float ItemStarPulseSpeed = 0.12f;

        public static float ItemStarWhiten = 0.35f;

        public static float ItemStarAlpha = 0.55f;



        public static float ItemEffectReferenceSize = 32f;

        public static float ItemEffectScaleConstant = 0.55f;

        public static float ItemEffectScalePerSize = 0.95f;



        public static float StackCountOffsetX = -5f;

        public static float StackCountOffsetY = -7f;

        public static float StackCountAlpha = 0.88f;



        public static float ItemGlowRarityMin = 0.78f;

        public static float ItemGlowRarityMax = 2f;

        public static float ItemGlowRarityCoin = 0.88f;

        public static float ItemGlowRarityQuest = 0.95f;

        public static float ItemGlowRarityBossTier = 2f;

        public static float ItemEffectColorDarken = 0.88f;

        public static float ItemEffectLowSaturationMin = 0.02f;

        public static float ItemEffectLowSaturationThreshold = 0.48f;

        public static float ItemEffectLowSaturationTarget = 0.65f;



        public static float ItemGlowSparkSpawnsPerFrame = 0.1f;

        public static float ItemGlowSparkReferenceGlowRadius = 20f;

        public static float ItemGlowSparkSpawnRadiusScale = 0.336f;

        public static int ItemGlowSparkLifetimeMin = 56;

        public static int ItemGlowSparkLifetimeMax = 88;

        public static float ItemGlowSparkLifetimeScale = 0.25f;

        public static int ItemGlowSparkLifetimeMinFrames = 12;

        public static float ItemGlowSparkHaloRadiusScale = 0.51f;

        public static float ItemGlowSparkCoreRadiusScale = 0.35f;

        public static float ItemGlowSparkSizeJitter = 0.15f;

        public static float ItemGlowSparkUpSpeedMin = 0.7f;

        public static float ItemGlowSparkUpSpeedMax = 1.7f;

        public static float ItemGlowSparkDrift = 0.22f;

        public static float ItemGlowSparkHaloAlpha = 0.24f;

        public static float ItemGlowSparkCoreWhiten = 0.92f;

        public static float ItemGlowSparkFadeInFrames = 3f;

        public static float ItemGlowSparkStreakReplaceChance = 0.45f;

        public static int ItemGlowSparkStreakLifetimeMin = 24;

        public static int ItemGlowSparkStreakLifetimeMax = 34;

        public static float ItemGlowSparkStreakLifetimeScale = 0.28f;

        public static int ItemGlowSparkStreakLifetimeMinFrames = 6;

        public static float ItemGlowSparkStreakUpSpeedMin = 2.0f;

        public static float ItemGlowSparkStreakUpSpeedMax = 3.0f;

        public static float ItemGlowSparkStreakHeightScale = 3.8f;

        public static int ItemGlowSparkStreakMinRarity = 3;

        public static float ItemGlowGlintSpawnsPerFrame = 0.05f;

        public static float ItemGlowGlintSpawnRadiusScale = 0.58f;

        public static int ItemGlowGlintLifetimeMin = 7;

        public static int ItemGlowGlintLifetimeMax = 12;

        public static float ItemGlowGlintScaleMin = 8f;

        public static float ItemGlowGlintScaleMax = 18f;

        public static float ItemGlowGlintAlpha = 0.95f;

        public static int ItemGlowGlintMinRarity = 5;

        public static int ItemGlowFlareMinRarity = 4;

        public static float ItemGlowFlareScale = 62f;

        public static float ItemGlowFlareAlpha = 0.09f;

        public static float ItemGlowFlareWhiten = 0.88f;

        public static float ItemGlowFlareRotationSpeed = 0.014f;

        public static int ItemBeamSwipeMinRarity = 1;

        public static int ItemGlowSparkMinRarity = 2;

        public static int ItemRippleHaloMinRarity = 3;

        /// <summary>Normal style: rarity thresholds are multiplied by this ratio (Fancy uses base values).</summary>
        public static float ItemGlowStyleNormalThresholdRatio = 1.5f;

        /// <summary>Normal style: rarity scale blends toward 1.0 by this factor (Fancy uses full range).</summary>
        public static float ItemGlowStyleNormalScaleBlend = 0.35f;

        /// <summary>Normal style: extra particle spawn rate scale.</summary>
        public static float ItemGlowStyleNormalExtraSpawnScale = 0.72f;

        /// <summary>Normal style: glint alpha scale.</summary>
        public static float ItemGlowStyleNormalGlintAlphaScale = 0.78f;

        /// <summary>Normal style: glint size scale.</summary>
        public static float ItemGlowStyleNormalGlintSizeScale = 0.82f;

        public static float ItemRippleBloomScale = 59.4f;

        public static float ItemRippleBloomSpeed = 0.42f;

        public static float ItemRippleBloomRingSpeed = 6f;

        public static float ItemRippleBloomRingCount = 3.2f;

        public static float ItemRippleBloomRingWidth = 0.96f;

        public static float ItemRippleBloomBaseDarkness = 0.28f;

        public static float ItemRippleBloomBaseAlpha = 0.2f;

        public static float ItemRippleBloomHighlightAlpha = 0.48f;

        public static float ItemRippleBloomColorMix = 0.88f;

        public static float ItemEffectHueTrailBase = -0.1f;

        public static float ItemEffectHueTrailHighlight = 0.01f;

        public static float ItemEffectHueTrailTail = -0.13f;

        public static float ItemEffectHueGlowBase = -0.14f;

        public static float ItemEffectHueGlowHighlight = -0.06f;

        public static float ItemEffectHueBeamBase = -0.11f;

        public static float ItemEffectHueBeamHighlight = 0.012f;

        public static float ItemEffectHueBeamTail = -0.15f;

        public static float ItemEffectHueBeamSwipe = -0.18f;

        public static float ItemEffectHueStar = 0.015f;

        public static float ItemEffectHueRippleBase = -0.12f;

        public static float ItemEffectHueRippleHighlight = -0.1f;

        public static float ItemEffectHueOutlineBase = -0.09f;

        public static float ItemEffectHueOutlineHighlight = 0.012f;

        public static float ItemEffectHueLuster = 0.01f;

        public static float ItemEffectSaturationBase = 0.1f;

        public static float ItemEffectSaturationGlowBase = 0.14f;

        public static float ItemEffectSaturationHighlight = 0.035f;
    }

}


