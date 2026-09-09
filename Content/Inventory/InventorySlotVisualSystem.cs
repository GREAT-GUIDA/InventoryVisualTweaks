using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace InventoryVisualTweaks.Content.Inventory {
    internal sealed class InventorySlotVisualState {
        public bool Seeded;
        public int LastType;
        public int LastStack;
        public int PickupFlashTimer;
        public int PickupSquashTimer;
        public ulong LastPickupEffectUpdate;
        public float ScaleX = 1f;
        public float ScaleY = 1f;
        public float HoverScale = 1f;
    }

    internal readonly struct ItemLocation : IEquatable<ItemLocation> {
        public readonly int InvId;
        public readonly int Slot;

        public ItemLocation(int invId, int slot) {
            InvId = invId;
            Slot = slot;
        }

        public bool Equals(ItemLocation other) => InvId == other.InvId && Slot == other.Slot;

        public override bool Equals(object obj) => obj is ItemLocation other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(InvId, Slot);
    }

    internal struct ItemMotionTrack {
        public Vector2 LastVisualPos;
        public bool HasPosition;
    }

    internal struct ItemMotionState {
        public Vector2 StartOffset;
        public float Elapsed;
        public float Duration;
    }

    internal struct MouseItemPickupMotionState {
        public Vector2 SourceCenter;
        public float Elapsed;
        public float Duration;
    }

    internal readonly struct InventorySlotKey : IEquatable<InventorySlotKey> {
        private const int CraftingInventoryId = -2001;

        public readonly int InvId;
        public readonly int Slot;

        public InventorySlotKey(int invId, int slot) {
            InvId = invId;
            Slot = slot;
        }

        public InventorySlotKey(Item[] inv, int slot) {
            InvId = RuntimeHelpers.GetHashCode(inv);
            Slot = slot;
        }

        public static InventorySlotKey ForSlot(Item[] inv, int context, int slot, Vector2 screenPosition = default) {
            if (InventorySlotContextRules.IsTransientDisplayContext(context))
                return new InventorySlotKey(CraftingInventoryId, HashScreenPosition(screenPosition));

            if (InventorySlotContextRules.UsesSharedSingleSlotArray(context))
                return new InventorySlotKey(InventorySlotContextRules.GetVirtualInventoryId(context), 0);

            return new InventorySlotKey(inv, slot);
        }

        public static bool IsLogicalInventory(int invId) =>
            invId != CraftingInventoryId && !InventorySlotContextRules.IsVirtualInventoryId(invId);

        private static int HashScreenPosition(Vector2 screenPosition) =>
            HashCode.Combine((int)screenPosition.X, (int)screenPosition.Y);

        public bool Equals(InventorySlotKey other) => InvId == other.InvId && Slot == other.Slot;

        public override bool Equals(object obj) => obj is InventorySlotKey other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(InvId, Slot);
    }

    internal static class InventorySlotDrawScope {
        public static bool Active { get; private set; }
        public static Item[] Inv { get; private set; }
        public static int Context { get; private set; }
        public static int Slot { get; private set; }
        public static Vector2 SlotPosition { get; private set; }
        public static bool Hovered { get; private set; }
        public static float HoverProgress { get; private set; }
        public static float HoverScaleAmount { get; private set; }
        public static Vector2 VisualOffset { get; private set; }
        public static bool HasActiveMotion { get; private set; }
        public static InventorySlotKey SlotKey { get; private set; }

        public static void Begin(Item[] inv, int context, int slot, Vector2 slotPosition, bool hovered, float hoverProgress, float hoverScaleAmount, Vector2 visualOffset, bool hasActiveMotion, InventorySlotKey slotKey) {
            Active = true;
            Inv = inv;
            Context = context;
            Slot = slot;
            SlotPosition = slotPosition;
            Hovered = hovered;
            HoverProgress = hoverProgress;
            HoverScaleAmount = hoverScaleAmount;
            VisualOffset = visualOffset;
            HasActiveMotion = hasActiveMotion;
            SlotKey = slotKey;
        }

        public static void End() {
            Active = false;
            Inv = null;
            Context = 0;
            Slot = 0;
            SlotPosition = Vector2.Zero;
            Hovered = false;
            HoverProgress = 0f;
            HoverScaleAmount = 1f;
            VisualOffset = Vector2.Zero;
            HasActiveMotion = false;
            SlotKey = default;
        }
    }

    public class InventorySlotVisualSystem : ModSystem {
        private static MouseItemPickupMotionState? MouseItemPickupMotion;
        private static ulong LastMousePickupMotionStartUpdate;

        private static readonly Dictionary<InventorySlotKey, InventorySlotVisualState> States = new();
        private static readonly Dictionary<Item, ItemLocation> ItemLocations = new();
        private static readonly Dictionary<Item, ItemLocation> ItemPreviousLocations = new();
        private static readonly Dictionary<Item, ItemMotionState> ItemMotionStates = new();
        private static readonly Dictionary<Item, ItemMotionTrack> ItemMotionTracks = new();
        private static readonly HashSet<Item> SuppressMotionForItem = new();
        private static readonly HashSet<Item> MouseItemsThisFrame = new();
        private static readonly Dictionary<InventorySlotKey, Vector2> SlotVisualPositions = new();
        private static readonly Dictionary<InventorySlotKey, Vector2> PreviousSlotVisualPositions = new();
        private static readonly Dictionary<InventorySlotKey, Vector2> SlotItemVisualCenters = new();
        private static readonly Dictionary<InventorySlotKey, Vector2> PreviousSlotItemVisualCenters = new();
        private static readonly Dictionary<InventorySlotKey, Vector2> SlotItemIconCenters = new();
        private static readonly Dictionary<InventorySlotKey, Vector2> PreviousSlotItemIconCenters = new();
        private static readonly Dictionary<Item, Vector2> PendingMousePlaceOrigins = new();
        private static readonly Dictionary<InventorySlotKey, InventorySlotKey> PendingTransferTargets = new();
        private static readonly HashSet<Item[]> DrawnInventoriesThisFrame = new();
        private static readonly HashSet<Item[]> LastDrawnInventories = new();
        private static readonly HashSet<InventorySlotKey> DrawnSlotsThisFrame = new();
        private static readonly HashSet<InventorySlotKey> LastDrawnSlots = new();
        private static readonly List<InventorySlotKey> StaleKeys = new();
        private static readonly List<Item> StaleTrackedItems = new();
        private static readonly List<Item> FinishedMotionItems = new();
        private static readonly List<Item[]> TrackedInventories = new();
        private static readonly Dictionary<InventorySlotKey, (int Type, int Stack)> SlotStateSnapshot = new();
        private static Item LastMouseItem;
        private static Vector2 MouseItemDrawCenterThisFrame;
        private static Vector2 PreviousMouseItemDrawCenter;

        public override void Load() {
            IL_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += PatchItemSlotDrawStackOffset;
            On_ItemSlot.PickupItemIntoMouse += OnPickupItemIntoMouse;
            On_ItemSlot.LeftClick_ItemArray_int_int += OnLeftClick;
        }

        public override void OnWorldLoad() {
            States.Clear();
            ItemLocations.Clear();
            ItemPreviousLocations.Clear();
            ItemMotionStates.Clear();
            ItemMotionTracks.Clear();
            SuppressMotionForItem.Clear();
            MouseItemsThisFrame.Clear();
            SlotVisualPositions.Clear();
            PreviousSlotVisualPositions.Clear();
            SlotItemVisualCenters.Clear();
            PreviousSlotItemVisualCenters.Clear();
            SlotItemIconCenters.Clear();
            PreviousSlotItemIconCenters.Clear();
            PendingMousePlaceOrigins.Clear();
            PendingTransferTargets.Clear();
            DrawnInventoriesThisFrame.Clear();
            LastDrawnInventories.Clear();
            DrawnSlotsThisFrame.Clear();
            LastDrawnSlots.Clear();
            SlotStateSnapshot.Clear();
            MouseItemPickupMotion = null;
            LastMousePickupMotionStartUpdate = 0;
            LastMouseItem = null;
            MouseItemDrawCenterThisFrame = Vector2.Zero;
            PreviousMouseItemDrawCenter = Vector2.Zero;
        }

        internal static void RecordSlotItemDrawCenter(InventorySlotKey slotKey, Vector2 center) {
            SlotItemVisualCenters[slotKey] = center;
        }

        internal static void RecordSlotItemIconCenter(InventorySlotKey slotKey, Vector2 iconCenter) {
            SlotItemIconCenters[slotKey] = iconCenter;
        }

        internal static void RecordMouseItemDrawCenter(Vector2 center) {
            MouseItemDrawCenterThisFrame = center;
        }

        private static Vector2 GetSlotItemCenterOffset() {
            float half = 26f * Main.inventoryScale;
            return new Vector2(half, half);
        }

        public static bool ShouldSkipContext(int context) =>
            InventorySlotContextRules.ShouldSkipContext(context);

        public static bool SupportsTransferEffects(int context) =>
            InventorySlotContextRules.SupportsTransferEffects(context);

        internal static Vector2 GetHoverItemOffset(float hoverProgress, float hoverScaleAmount) {
            if (hoverProgress <= 0.001f || hoverScaleAmount <= 1.001f)
                return Vector2.Zero;

            float referenceExtraScale = InventorySlotVisualTuning.ReferenceHoverScaleAmount - 1f;
            float amount = (hoverScaleAmount - 1f) / referenceExtraScale;
            return InventorySlotVisualTuning.HoverItemOffset * amount * Main.inventoryScale * hoverProgress;
        }

        public static Vector2 GetMouseItemPickupOffset(Item item, Vector2 currentMouseItemDrawCenter) {
            if (item == null || !ReferenceEquals(item, Main.mouseItem)
                || !ModContent.GetInstance<InventoryInteractionConfig>().EnableMouseItemMotion
                || !MouseItemPickupMotion.HasValue || Main.mouseItem.IsAir)
                return Vector2.Zero;

            MouseItemPickupMotionState state = MouseItemPickupMotion.Value;
            float progress = MathHelper.Clamp(state.Elapsed / state.Duration, 0f, 1f);
            float eased = EaseOutCubic(progress);
            return (state.SourceCenter - currentMouseItemDrawCenter) * (1f - eased);
        }

        private static void OnLeftClick(On_ItemSlot.orig_LeftClick_ItemArray_int_int orig, Item[] inv, int context, int slot) {
            if (inv == null || slot < 0 || slot >= inv.Length || ShouldSkipContext(context) || !SupportsTransferEffects(context)) {
                orig(inv, context, slot);
                return;
            }

            Item mouseBefore = Main.mouseItem != null && !Main.mouseItem.IsAir ? Main.mouseItem : null;
            int mouseStackBefore = mouseBefore?.stack ?? 0;
            Item slotItemBefore = inv[slot] != null && !inv[slot].IsAir ? inv[slot] : null;
            Vector2 mouseOrigin = PreviousMouseItemDrawCenter;
            InventorySlotKey slotKey = InventorySlotKey.ForSlot(inv, context, slot);

            orig(inv, context, slot);

            if (slotItemBefore != null && !slotItemBefore.IsAir
                && Main.mouseItem != null && !Main.mouseItem.IsAir
                && ReferenceEquals(Main.mouseItem, slotItemBefore)) {
                TryStartMouseItemPickupMotionFromSlot(slotKey);
                return;
            }

            if (mouseBefore == null || mouseBefore.IsAir)
                return;

            Item slotItem = inv[slot];
            if (slotItem == null || slotItem.IsAir)
                return;

            bool placedItem = ReferenceEquals(slotItem, mouseBefore);
            bool stackedFromMouse = !placedItem
                && slotItem.type == mouseBefore.type
                && mouseStackBefore > 0
                && (Main.mouseItem.IsAir || Main.mouseItem.stack < mouseStackBefore);

            if (!placedItem && !stackedFromMouse)
                return;

            if (mouseOrigin == Vector2.Zero)
                mouseOrigin = MouseItemDrawCenterThisFrame;

            if (mouseOrigin == Vector2.Zero)
                return;

            InventorySlotVisualState state = GetOrCreateState(slotKey);
            if (!TryStartMousePlaceMotion(slotItem, slotKey, null, mouseOrigin, state))
                PendingMousePlaceOrigins[slotItem] = mouseOrigin;
        }

        private static void OnPickupItemIntoMouse(On_ItemSlot.orig_PickupItemIntoMouse orig, Item[] inv, int context, int slot, Player player) {
            InventorySlotKey? slotKey = null;
            if (!ShouldSkipContext(context) && inv != null && slot >= 0 && slot < inv.Length && SupportsTransferEffects(context))
                slotKey = InventorySlotKey.ForSlot(inv, context, slot);

            orig(inv, context, slot, player);

            if (slotKey.HasValue)
                TryStartMouseItemPickupMotionFromSlot(slotKey.Value);
        }

        private static void PatchItemSlotDrawStackOffset(ILContext il) {
            ILCursor c = new ILCursor(il);
            MethodInfo vanilla = typeof(ChatManager).GetMethod(
                nameof(ChatManager.DrawColorCodedStringWithShadow),
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] {
                    typeof(SpriteBatch), typeof(DynamicSpriteFont), typeof(string), typeof(Vector2), typeof(Color),
                    typeof(float), typeof(Vector2), typeof(Vector2), typeof(float), typeof(float)
                },
                null);
            MethodInfo replacement = typeof(InventorySlotVisualSystem).GetMethod(
                nameof(DrawItemStackWithHoverOffset),
                BindingFlags.NonPublic | BindingFlags.Static);

            if (vanilla == null || replacement == null)
                return;

            while (c.TryGotoNext(MoveType.Before, i => i.MatchCall(vanilla))) {
                bool isMainStack = false;
                for (int j = c.Index - 1; j >= c.Index - 50 && j >= 0; j--) {
                    if (c.Instrs[j].OpCode != OpCodes.Ldc_R4 || (float)c.Instrs[j].Operand != 10f)
                        continue;

                    if (j + 1 < c.Instrs.Count
                        && c.Instrs[j + 1].OpCode == OpCodes.Ldc_R4
                        && (float)c.Instrs[j + 1].Operand == 26f) {
                        isMainStack = true;
                        break;
                    }
                }

                if (isMainStack) {
                    c.Remove();
                    c.Emit(OpCodes.Call, replacement);
                }
            }
        }

        private static Vector2 DrawItemStackWithHoverOffset(
            SpriteBatch spriteBatch,
            DynamicSpriteFont font,
            string text,
            Vector2 position,
            Color baseColor,
            float rotation,
            Vector2 origin,
            Vector2 baseScale,
            float maxWidth,
            float spread) {
            if (!InventorySlotDrawScope.Active) {
                return ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, text, position, baseColor, rotation, origin, baseScale, maxWidth, spread);
            }

            position += InventorySlotDrawScope.VisualOffset;
            if (InventorySlotDrawScope.HoverProgress > 0.001f)
                position += InventorySlotVisualTuning.HoverStackOffset * Main.inventoryScale * InventorySlotDrawScope.HoverProgress;

            return ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, text, position, baseColor, rotation, origin, baseScale, maxWidth, spread);
        }

        public override void PostDrawInterface(SpriteBatch spriteBatch) {
            PreviousSlotVisualPositions.Clear();
            foreach (KeyValuePair<InventorySlotKey, Vector2> pair in SlotVisualPositions)
                PreviousSlotVisualPositions[pair.Key] = pair.Value;

            PreviousSlotItemVisualCenters.Clear();
            foreach (KeyValuePair<InventorySlotKey, Vector2> pair in SlotItemVisualCenters)
                PreviousSlotItemVisualCenters[pair.Key] = pair.Value;

            PreviousSlotItemIconCenters.Clear();
            foreach (KeyValuePair<InventorySlotKey, Vector2> pair in SlotItemIconCenters)
                PreviousSlotItemIconCenters[pair.Key] = pair.Value;

            SlotItemVisualCenters.Clear();
            SlotItemIconCenters.Clear();

            PreviousMouseItemDrawCenter = MouseItemDrawCenterThisFrame;
            MouseItemDrawCenterThisFrame = Vector2.Zero;

            LastDrawnInventories.Clear();
            foreach (Item[] inv in DrawnInventoriesThisFrame)
                LastDrawnInventories.Add(inv);

            LastDrawnSlots.Clear();
            foreach (InventorySlotKey slotKey in DrawnSlotsThisFrame)
                LastDrawnSlots.Add(slotKey);

            DrawnInventoriesThisFrame.Clear();
            DrawnSlotsThisFrame.Clear();

            AdvanceMotionStates();
        }

        public override void PostUpdateEverything() {
            if (Main.gameMenu || Main.dedServ)
                return;

            TrackedInventories.Clear();
            Player player = Main.LocalPlayer;
            if (player == null)
                return;

            Item previousFrameMouseItem = LastMouseItem;

            MouseItemsThisFrame.Clear();
            if (!Main.mouseItem.IsAir)
                MouseItemsThisFrame.Add(Main.mouseItem);

            if (Main.mouseItem.IsAir)
                MouseItemPickupMotion = null;

            foreach (Item[] inv in LastDrawnInventories)
                TrackedInventories.Add(inv);

            PendingTransferTargets.Clear();
            var nextItemLocations = new Dictionary<Item, ItemLocation>();
            var newlyEmptied = new List<(InventorySlotKey Key, int Type, int Stack)>();
            var incomingCandidates = new List<(InventorySlotKey DestKey, InventorySlotVisualState State, Item Item, int Type, int Stack)>();
            var replaceCandidates = new List<(InventorySlotKey DestKey, int Type, int Stack)>();

            CaptureSlotSnapshot();

            ItemPreviousLocations.Clear();
            foreach (KeyValuePair<Item, ItemLocation> pair in ItemLocations)
                ItemPreviousLocations[pair.Key] = pair.Value;

            UpdatePickupAnimationStates();

            foreach (Item[] inv in TrackedInventories)
                ScanInventory(inv, nextItemLocations, newlyEmptied, incomingCandidates, replaceCandidates, previousFrameMouseItem);

            ResolveIncomingTransfers(newlyEmptied, incomingCandidates);
            ResolveReplaceTransfers(replaceCandidates);

            ItemLocations.Clear();
            foreach (KeyValuePair<Item, ItemLocation> pair in nextItemLocations)
                ItemLocations[pair.Key] = pair.Value;

            StaleKeys.Clear();
            foreach (InventorySlotKey key in States.Keys) {
                bool alive = false;
                if (!InventorySlotKey.IsLogicalInventory(key.InvId)) {
                    alive = LastDrawnSlots.Contains(key);
                } else {
                    foreach (Item[] inv in TrackedInventories) {
                        if (RuntimeHelpers.GetHashCode(inv) == key.InvId && key.Slot >= 0 && key.Slot < inv.Length) {
                            alive = true;
                            break;
                        }
                    }
                }

                if (!alive)
                    StaleKeys.Add(key);
            }

            for (int i = 0; i < StaleKeys.Count; i++) {
                States.Remove(StaleKeys[i]);
                SlotVisualPositions.Remove(StaleKeys[i]);
                PreviousSlotVisualPositions.Remove(StaleKeys[i]);
                SlotItemVisualCenters.Remove(StaleKeys[i]);
                PreviousSlotItemVisualCenters.Remove(StaleKeys[i]);
                SlotItemIconCenters.Remove(StaleKeys[i]);
                PreviousSlotItemIconCenters.Remove(StaleKeys[i]);
            }

            StaleTrackedItems.Clear();
            foreach (Item item in ItemMotionStates.Keys) {
                if (!nextItemLocations.ContainsKey(item))
                    StaleTrackedItems.Add(item);
            }

            for (int i = 0; i < StaleTrackedItems.Count; i++) {
                Item item = StaleTrackedItems[i];
                ItemMotionStates.Remove(item);
                ItemMotionTracks.Remove(item);
                SuppressMotionForItem.Remove(item);
                PendingMousePlaceOrigins.Remove(item);
            }

            if (!Main.mouseItem.IsAir)
                LastMouseItem = Main.mouseItem;
            else
                LastMouseItem = null;
        }

        public static bool IsSlotHovered(Vector2 slotPosition) {
            if (Main.LocalPlayer == null)
                return false;

            float size = InventorySlotVisualTuning.SlotCellSize * Main.inventoryScale;
            return Main.MouseScreen.Between(slotPosition, slotPosition + new Vector2(size));
        }

        internal static bool TryGetActiveState(out InventorySlotVisualState state) {
            if (!InventorySlotDrawScope.Active) {
                state = null;
                return false;
            }

            return States.TryGetValue(InventorySlotDrawScope.SlotKey, out state);
        }

        public static void BeginSlotDraw(Item[] inv, int context, int slot, ref Vector2 slotPosition) {
            if (ShouldSkipContext(context) || inv == null || slot < 0 || slot >= inv.Length) {
                InventorySlotDrawScope.End();
                return;
            }

            InventorySlotKey slotKey = InventorySlotKey.ForSlot(inv, context, slot, slotPosition);
            InventorySlotVisualState state = GetOrCreateState(slotKey);

            if (SupportsTransferEffects(context) && !InventorySlotContextRules.UsesSharedSingleSlotArray(context))
                DrawnInventoriesThisFrame.Add(inv);

            DrawnSlotsThisFrame.Add(slotKey);
            if (!Main.mouseItem.IsAir)
                MouseItemsThisFrame.Add(Main.mouseItem);

            bool hovered = IsSlotHovered(slotPosition);
            var interactionConfig = ModContent.GetInstance<InventoryInteractionConfig>();
            float targetHoverScale = interactionConfig.EnableHoverScale ? interactionConfig.HoverScaleAmount : 1f;
            state.HoverScale = MathHelper.Lerp(
                state.HoverScale,
                hovered ? targetHoverScale : 1f,
                InventorySlotVisualTuning.HoverLerp);
            float hoverRange = targetHoverScale - 1f;
            float hoverProgress = hoverRange > 0.001f
                ? MathHelper.Clamp((state.HoverScale - 1f) / hoverRange, 0f, 1f)
                : 0f;

            Item item = inv[slot];
            Vector2 visualOffset = Vector2.Zero;
            bool hasActiveMotion = false;
            if (item != null && !item.IsAir) {
                ItemLocation currentLocation = new(slotKey.InvId, slotKey.Slot);

                if (!ItemMotionStates.ContainsKey(item)) {
                    if (PendingMousePlaceOrigins.TryGetValue(item, out Vector2 mouseItemCenter)) {
                        Vector2 slotIconCenter = slotPosition + GetSlotItemCenterOffset();
                        if (TryStartMousePlaceMotion(item, slotKey, slotPosition, mouseItemCenter, state))
                            PendingMousePlaceOrigins.Remove(item);
                    } else if (SupportsTransferEffects(context)) {
                        if (TryGetTransferSourcePosition(slotKey, out Vector2 sourcePos)) {
                            TryStartItemMotion(item, sourcePos - slotPosition);
                        } else {
                            bool wasTracked = ItemPreviousLocations.TryGetValue(item, out ItemLocation previousLocation);
                            bool relocated = wasTracked && !previousLocation.Equals(currentLocation);

                            if (relocated
                                && ItemMotionTracks.TryGetValue(item, out ItemMotionTrack track)
                                && track.HasPosition) {
                                TryStartItemMotion(item, track.LastVisualPos - slotPosition);
                            } else if (!wasTracked
                                && LastDrawnSlots.Contains(slotKey)
                                && TryFindTransferSourceBySlotState(item, slotKey, out Vector2 drawSourcePos)) {
                                TryStartItemMotion(item, drawSourcePos - slotPosition);
                            }
                        }
                    }
                }

                visualOffset = GetMotionOffset(item);
                hasActiveMotion = ItemMotionStates.ContainsKey(item);

                ItemMotionTracks[item] = new ItemMotionTrack {
                    LastVisualPos = slotPosition + visualOffset,
                    HasPosition = true
                };

                SlotVisualPositions[slotKey] = slotPosition + visualOffset;
                state.Seeded = true;
                state.LastType = item.type;
                state.LastStack = item.stack;
            } else {
                if (!state.Seeded) {
                    state.Seeded = true;
                    state.LastType = 0;
                    state.LastStack = 0;
                }
            }

            InventorySlotDrawScope.Begin(inv, context, slot, slotPosition, hovered, hoverProgress, targetHoverScale, visualOffset, hasActiveMotion, slotKey);
        }

        public static void EndSlotDraw() {
            InventorySlotDrawScope.End();
        }

        private static void StartMouseItemPickupMotion(Vector2 slotItemCenter) {
            if (!ModContent.GetInstance<InventoryInteractionConfig>().EnableMouseItemMotion)
                return;

            ulong now = Main.GameUpdateCount;
            if (MouseItemPickupMotion.HasValue
                && now - LastMousePickupMotionStartUpdate < (ulong)InventorySlotVisualTuning.MousePickupMotionMinInterval)
                return;

            LastMousePickupMotionStartUpdate = now;
            MouseItemPickupMotion = new MouseItemPickupMotionState {
                SourceCenter = slotItemCenter,
                Elapsed = 0f,
                Duration = InventorySlotVisualTuning.MotionDuration
            };
        }

        private static void TryStartMouseItemPickupMotionFromSlot(InventorySlotKey slotKey) {
            if (Main.mouseItem.IsAir || !TryGetRecordedSlotItemIconCenter(slotKey, out Vector2 slotIconCenter))
                return;

            StartMouseItemPickupMotion(slotIconCenter);
        }

        private static void TryStartMouseItemPickupMotion(InventorySlotKey slotKey, int emptiedType) {
            if (Main.mouseItem.IsAir || Main.mouseItem.type != emptiedType)
                return;

            TryStartMouseItemPickupMotionFromSlot(slotKey);
        }

        private static float EaseOutCubic(float t) => 1f - MathF.Pow(1f - t, 3f);

        private static void ClearPickupFlash(InventorySlotVisualState state) {
            if (state == null)
                return;

            state.PickupFlashTimer = 0;
            state.PickupSquashTimer = 0;
            state.ScaleX = 1f;
            state.ScaleY = 1f;
        }

        private static bool TryGetTransferSourcePosition(InventorySlotKey destKey, out Vector2 sourcePos) {
            if (PendingTransferTargets.TryGetValue(destKey, out InventorySlotKey sourceKey)
                && TryGetRecordedSlotPosition(sourceKey, out sourcePos)) {
                return true;
            }

            sourcePos = Vector2.Zero;
            return false;
        }

        private static bool TryGetRecordedSlotPosition(InventorySlotKey slotKey, out Vector2 position) {
            if (PreviousSlotVisualPositions.TryGetValue(slotKey, out position))
                return true;

            return SlotVisualPositions.TryGetValue(slotKey, out position);
        }

        private static bool TryGetRecordedSlotItemIconCenter(InventorySlotKey slotKey, out Vector2 center) {
            if (PreviousSlotItemIconCenters.TryGetValue(slotKey, out center))
                return true;

            if (SlotItemIconCenters.TryGetValue(slotKey, out center))
                return true;

            if (TryGetRecordedSlotItemCenter(slotKey, out center))
                return true;

            if (TryGetRecordedSlotPosition(slotKey, out Vector2 topLeft)) {
                center = topLeft + GetSlotItemCenterOffset();
                return true;
            }

            center = Vector2.Zero;
            return false;
        }

        private static bool TryGetRecordedSlotItemCenter(InventorySlotKey slotKey, out Vector2 center) {
            if (PreviousSlotItemVisualCenters.TryGetValue(slotKey, out center))
                return true;

            if (SlotItemVisualCenters.TryGetValue(slotKey, out center))
                return true;

            if (TryGetRecordedSlotPosition(slotKey, out Vector2 topLeft)) {
                center = topLeft + GetSlotItemCenterOffset();
                return true;
            }

            center = Vector2.Zero;
            return false;
        }

        private static void CaptureSlotSnapshot() {
            SlotStateSnapshot.Clear();
            foreach (KeyValuePair<InventorySlotKey, InventorySlotVisualState> pair in States)
                SlotStateSnapshot[pair.Key] = (pair.Value.LastType, pair.Value.LastStack);
        }

        private static bool IsPickupToMouse(int type) =>
            !Main.mouseItem.IsAir && Main.mouseItem.type == type;

        private static bool IsEmptiedPickupSource(InventorySlotKey sourceKey, int type) {
            if (!IsPickupToMouse(type))
                return false;

            if (!TryResolveSlot(sourceKey, out Item[] inv, out int slot))
                return false;

            Item sourceItem = inv[slot];
            return sourceItem == null || sourceItem.IsAir;
        }

        private static bool SourceMatchesOutgoingTransfer(InventorySlotKey sourceKey, int type, int stack) {
            if (IsEmptiedPickupSource(sourceKey, type))
                return false;

            if (!TryResolveSlot(sourceKey, out Item[] inv, out int slot))
                return false;

            Item sourceItem = inv[slot];
            if (sourceItem == null || sourceItem.IsAir)
                return true;

            return sourceItem.type != type || sourceItem.stack != stack;
        }

        private static bool TryFindTransferSourceBySlotState(Item item, InventorySlotKey destKey, out Vector2 sourcePos) {
            sourcePos = Vector2.Zero;

            if (!InventorySlotKey.IsLogicalInventory(destKey.InvId))
                return false;

            foreach (KeyValuePair<InventorySlotKey, (int Type, int Stack)> pair in SlotStateSnapshot) {
                InventorySlotKey sourceKey = pair.Key;
                if (sourceKey.Equals(destKey) || !InventorySlotKey.IsLogicalInventory(sourceKey.InvId))
                    continue;

                (int type, int stack) = pair.Value;
                if (type != item.type || stack != item.stack)
                    continue;

                if (!SourceMatchesOutgoingTransfer(sourceKey, type, stack))
                    continue;

                if (!TryGetRecordedSlotPosition(sourceKey, out sourcePos))
                    continue;

                PendingTransferTargets[destKey] = sourceKey;
                return true;
            }

            return false;
        }

        private static void ResolveReplaceTransfers(List<(InventorySlotKey DestKey, int Type, int Stack)> replaceCandidates) {
            for (int i = 0; i < replaceCandidates.Count; i++) {
                (InventorySlotKey destKey, int type, int stack) = replaceCandidates[i];
                if (PendingTransferTargets.ContainsKey(destKey))
                    continue;

                foreach (KeyValuePair<InventorySlotKey, (int Type, int Stack)> pair in SlotStateSnapshot) {
                    InventorySlotKey sourceKey = pair.Key;
                    if (sourceKey.Equals(destKey))
                        continue;

                    if (pair.Value.Type != type || pair.Value.Stack != stack)
                        continue;

                    if (!SourceMatchesOutgoingTransfer(sourceKey, type, stack))
                        continue;

                    if (!TryGetRecordedSlotPosition(sourceKey, out _))
                        continue;

                    PendingTransferTargets[destKey] = sourceKey;
                    break;
                }
            }
        }

        private static bool TryResolveSlot(InventorySlotKey key, out Item[] inv, out int slot) {
            slot = key.Slot;

            if (!InventorySlotKey.IsLogicalInventory(key.InvId)) {
                inv = null;
                return false;
            }

            foreach (Item[] candidate in LastDrawnInventories) {
                if (RuntimeHelpers.GetHashCode(candidate) == key.InvId) {
                    inv = candidate;
                    return slot >= 0 && slot < inv.Length;
                }
            }

            foreach (Item[] candidate in DrawnInventoriesThisFrame) {
                if (RuntimeHelpers.GetHashCode(candidate) == key.InvId) {
                    inv = candidate;
                    return slot >= 0 && slot < inv.Length;
                }
            }

            inv = null;
            return false;
        }

        private static bool TryStartMousePlaceMotion(Item item, InventorySlotKey slotKey, Vector2? slotPosition, Vector2 mouseOrigin, InventorySlotVisualState flashState) {
            if (mouseOrigin == Vector2.Zero)
                return false;

            Vector2 slotIconCenter;
            if (slotPosition.HasValue)
                slotIconCenter = slotPosition.Value + GetSlotItemCenterOffset();
            else if (!TryResolveSlotIconCenter(slotKey, out slotIconCenter))
                return false;

            return TryStartItemMotion(item, mouseOrigin - slotIconCenter, flashState);
        }

        private static bool TryResolveSlotIconCenter(InventorySlotKey slotKey, out Vector2 slotIconCenter) {
            if (TryGetRecordedSlotItemIconCenter(slotKey, out slotIconCenter))
                return true;

            if (PreviousSlotVisualPositions.TryGetValue(slotKey, out Vector2 topLeft)
                || SlotVisualPositions.TryGetValue(slotKey, out topLeft)) {
                slotIconCenter = topLeft + GetSlotItemCenterOffset();
                return true;
            }

            slotIconCenter = Vector2.Zero;
            return false;
        }

        private static bool TryStartItemMotion(Item item, Vector2 startOffset, InventorySlotVisualState flashState = null) {
            if (!ModContent.GetInstance<InventoryInteractionConfig>().EnableSlotTransferMotion)
                return false;

            if (startOffset.LengthSquared() < 0.25f)
                return false;

            if (SuppressMotionForItem.Remove(item))
                return false;

            ClearPickupFlash(flashState);

            ItemMotionStates[item] = new ItemMotionState {
                StartOffset = startOffset,
                Elapsed = 0f,
                Duration = InventorySlotVisualTuning.MotionDuration
            };
            return true;
        }

        private static Vector2 GetMotionOffset(Item item) {
            if (!ItemMotionStates.TryGetValue(item, out ItemMotionState state))
                return Vector2.Zero;

            float progress = MathHelper.Clamp(state.Elapsed / state.Duration, 0f, 1f);
            float eased = EaseOutCubic(progress);
            return state.StartOffset * (1f - eased);
        }

        private static void AdvanceMotionStates() {
            if (MouseItemPickupMotion.HasValue) {
                MouseItemPickupMotionState mouseState = MouseItemPickupMotion.Value;
                mouseState.Elapsed += 1f;
                if (mouseState.Elapsed >= mouseState.Duration || Main.mouseItem.IsAir)
                    MouseItemPickupMotion = null;
                else
                    MouseItemPickupMotion = mouseState;
            }

            FinishedMotionItems.Clear();
            foreach (KeyValuePair<Item, ItemMotionState> pair in ItemMotionStates) {
                ItemMotionState state = pair.Value;
                state.Elapsed += 1f;
                if (state.Elapsed >= state.Duration)
                    FinishedMotionItems.Add(pair.Key);
                else
                    ItemMotionStates[pair.Key] = state;
            }

            for (int i = 0; i < FinishedMotionItems.Count; i++)
                ItemMotionStates.Remove(FinishedMotionItems[i]);
        }

        private static void UpdatePickupAnimationStates() {
            foreach (InventorySlotVisualState state in States.Values) {
                if (state.PickupSquashTimer > 0) {
                    state.PickupSquashTimer--;
                    float progress = 1f - state.PickupSquashTimer / (float)InventorySlotVisualTuning.PickupSquashTime;
                    state.ScaleX = MathHelper.Lerp(InventorySlotVisualTuning.PickupSquashScaleX, 1f, progress);
                    state.ScaleY = MathHelper.Lerp(InventorySlotVisualTuning.PickupSquashScaleY, 1f, progress);
                } else {
                    state.ScaleX = 1f;
                    state.ScaleY = 1f;
                }

                if (state.PickupFlashTimer > 0)
                    state.PickupFlashTimer--;
            }
        }

        private static void ResolveIncomingTransfers(
            List<(InventorySlotKey Key, int Type, int Stack)> newlyEmptied,
            List<(InventorySlotKey DestKey, InventorySlotVisualState State, Item Item, int Type, int Stack)> incomingCandidates) {
            var usedEmptied = new bool[newlyEmptied.Count];

            for (int i = 0; i < incomingCandidates.Count; i++) {
                (InventorySlotKey destKey, InventorySlotVisualState state, Item item, int type, int stack) = incomingCandidates[i];
                int matchIndex = -1;

                for (int j = 0; j < newlyEmptied.Count; j++) {
                    if (usedEmptied[j])
                        continue;

                    (InventorySlotKey key, int emptiedType, int emptiedStack) = newlyEmptied[j];
                    if (emptiedType == type && emptiedStack == stack) {
                        matchIndex = j;
                        break;
                    }
                }

                if (matchIndex >= 0) {
                    (InventorySlotKey sourceKey, int emptiedType, _) = newlyEmptied[matchIndex];
                    if (!IsEmptiedPickupSource(sourceKey, emptiedType)) {
                        usedEmptied[matchIndex] = true;
                        PendingTransferTargets[destKey] = sourceKey;
                    }
                } else if (!PendingMousePlaceOrigins.ContainsKey(item)) {
                    TriggerPickupFlash(state);
                }
            }
        }

        private static void ScanInventory(
            Item[] inv,
            Dictionary<Item, ItemLocation> nextItemLocations,
            List<(InventorySlotKey Key, int Type, int Stack)> newlyEmptied,
            List<(InventorySlotKey DestKey, InventorySlotVisualState State, Item Item, int Type, int Stack)> incomingCandidates,
            List<(InventorySlotKey DestKey, int Type, int Stack)> replaceCandidates,
            Item previousFrameMouseItem) {
            int invId = RuntimeHelpers.GetHashCode(inv);

            for (int slot = 0; slot < inv.Length; slot++) {
                Item item = inv[slot];
                InventorySlotVisualState state = GetOrCreateState(new InventorySlotKey(invId, slot));
                InventorySlotKey slotKey = new InventorySlotKey(invId, slot);

                if (item == null || item.IsAir) {
                    if (!state.Seeded)
                        continue;

                    if (state.LastType > 0) {
                        newlyEmptied.Add((slotKey, state.LastType, state.LastStack));
                        TryStartMouseItemPickupMotion(slotKey, state.LastType);
                    }

                    state.LastType = 0;
                    state.LastStack = 0;
                    continue;
                }

                if (!state.Seeded)
                    continue;

                ItemLocation currentLocation = new(invId, slot);
                bool wasTracked = ItemLocations.TryGetValue(item, out ItemLocation previousLocation);
                bool relocated = wasTracked && !previousLocation.Equals(currentLocation);
                bool slotWasVisible = LastDrawnSlots.Contains(slotKey);

                if (!relocated) {
                    if (item.type != state.LastType) {
                        if (item.type > 0 && state.LastType == 0 && !wasTracked) {
                            if (slotWasVisible) {
                                incomingCandidates.Add((slotKey, state, item, item.type, item.stack));
                                if (previousFrameMouseItem != null && ReferenceEquals(previousFrameMouseItem, item))
                                    PendingMousePlaceOrigins[item] = PreviousMouseItemDrawCenter;
                            }
                        } else if (item.type > 0 && state.LastType > 0 && !wasTracked && slotWasVisible) {
                            replaceCandidates.Add((slotKey, item.type, item.stack));
                        } else if (item.type > 0 && state.LastType > 0 && wasTracked && previousLocation.Equals(currentLocation) && slotWasVisible) {
                            TriggerPickupFlash(state);
                        }
                    } else if (item.stack > state.LastStack && state.LastStack > 0 && slotWasVisible) {
                        if (!PendingMousePlaceOrigins.ContainsKey(item))
                            TriggerPickupFlash(state);
                    }
                }

                state.LastType = item.type;
                state.LastStack = item.stack;
                nextItemLocations[item] = currentLocation;
            }
        }

        private static void TriggerPickupFlash(InventorySlotVisualState state) {
            if (ModContent.GetInstance<InventoryInteractionConfig>().PickupFlashIntensity <= 0f)
                return;

            ulong now = Main.GameUpdateCount;
            if (now - state.LastPickupEffectUpdate < (ulong)InventorySlotVisualTuning.PickupFlashMinInterval)
                return;

            state.LastPickupEffectUpdate = now;
            state.PickupFlashTimer = InventorySlotVisualTuning.PickupFlashTime;
            state.PickupSquashTimer = InventorySlotVisualTuning.PickupSquashTime;
            state.ScaleX = InventorySlotVisualTuning.PickupSquashScaleX;
            state.ScaleY = InventorySlotVisualTuning.PickupSquashScaleY;
        }

        private static InventorySlotVisualState GetOrCreateState(InventorySlotKey key) {
            if (!States.TryGetValue(key, out InventorySlotVisualState state)) {
                state = new InventorySlotVisualState();
                States[key] = state;
            }

            return state;
        }
    }
}
