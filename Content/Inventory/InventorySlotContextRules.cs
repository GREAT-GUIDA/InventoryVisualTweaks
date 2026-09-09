using Terraria;
using Terraria.UI;

namespace InventoryVisualTweaks.Content.Inventory {
    internal static class InventorySlotContextRules {
        public static bool ShouldSkipContext(int context) =>
            context is ItemSlot.Context.ChatItem
            or ItemSlot.Context.MouseItem
            or ItemSlot.Context.CreativeSacrifice
            or ItemSlot.Context.InWorld;

        public static bool ShouldUseVanillaItemIconOnly(int context) =>
            context is ItemSlot.Context.ChatItem
            or ItemSlot.Context.CreativeSacrifice
            or ItemSlot.Context.InWorld;

        public static bool IsMouseItemContext(int context) =>
            context == ItemSlot.Context.MouseItem;

        public static bool IsHotbarSlot(int context, int slot) =>
            context == ItemSlot.Context.HotbarItem
            || (context == ItemSlot.Context.InventoryItem && slot < 10);

        public static bool IsSelectedHotbarSlotWhenInventoryOpen(int context, int slot) =>
            Main.playerInventory
            && context == ItemSlot.Context.InventoryItem
            && slot < 10
            && slot == Main.LocalPlayer.selectedItem;

        public static bool IsPrimaryMouseItemDraw(Item item, int context) =>
            IsMouseItemContext(context) && item != null && ReferenceEquals(item, Main.mouseItem);

        public static bool IsTransientDisplayContext(int context) =>
            context == ItemSlot.Context.CraftingMaterial;

        public static bool SupportsTransferEffects(int context) =>
            !IsTransientDisplayContext(context);

        public static bool SupportsInventoryVisuals(int context) =>
            !ShouldSkipContext(context);

        public static bool UsesSharedSingleSlotArray(int context) =>
            context == ItemSlot.Context.TrashItem;

        public static int GetVirtualInventoryId(int context) =>
            VirtualInventoryIdBase - context;

        public static bool IsVirtualInventoryId(int invId) =>
            invId <= VirtualInventoryIdBase && invId > VirtualInventoryIdBase - 1_000;

        private const int VirtualInventoryIdBase = -100_000;
    }
}
