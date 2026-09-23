using InventoryVisualTweaks.Content.Inventory.Compatibility;
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
            context is ItemSlot.Context.CraftingMaterial
            or ItemSlot.Context.CreativeInfinite;

        public static bool SupportsTransferEffects(int context) =>
            !IsTransientDisplayContext(context);

        public static bool SupportsTransferEffects(int context, Item[] inv) {
            if (!SupportsTransferEffects(context))
                return false;

            return !MagicStorageCompatibility.ShouldSuppressTransferMotion(inv);
        }

        public static bool SupportsInventoryVisuals(int context) =>
            !ShouldSkipContext(context);

        public static bool UsesSharedSingleSlotArray(int context, Item[] inv) {
            if (context != ItemSlot.Context.TrashItem || inv == null || inv.Length != 1)
                return false;

            Player player = Main.LocalPlayer;
            return player != null && ReferenceEquals(inv[0], player.trashItem);
        }

        public static int GetVirtualInventoryId(int context) =>
            VirtualInventoryIdBase - context;

        public static bool IsVirtualInventoryId(int invId) =>
            invId <= VirtualInventoryIdBase && invId > VirtualInventoryIdBase - 1_000;

        private const int VirtualInventoryIdBase = -100_000;
    }
}
