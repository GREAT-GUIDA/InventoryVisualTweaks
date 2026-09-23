using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace InventoryVisualTweaks.Content.Inventory.Compatibility {
    /// <summary>
    /// Magic Storage reuses vanilla <see cref="Terraria.UI.ItemSlot.Context"/> values for slot tinting.
    /// Its UI also shares item references across zones. Suppress slot transfer motion there to avoid false matches.
    /// </summary>
    internal static class MagicStorageCompatibility {
        private static Func<bool> _hasOpenUi;

        internal static void Load() {
            _hasOpenUi = null;
            if (!ModLoader.HasMod("MagicStorage"))
                return;

            try {
                Mod mod = ModLoader.GetMod("MagicStorage");
                Type magicUi = mod?.Code?.GetType("MagicStorage.Common.Systems.MagicUI");
                MethodInfo method = magicUi?.GetMethod("HasOpenUI", BindingFlags.Public | BindingFlags.Static);
                if (method != null)
                    _hasOpenUi = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), method);
            } catch {
                _hasOpenUi = null;
            }
        }

        internal static void Unload() => _hasOpenUi = null;

        internal static bool IsMagicStorageUiOpen() => _hasOpenUi?.Invoke() ?? false;

        /// <summary>Legacy <c>UISlotZone</c> draws every slot through <c>DummyItems[10]</c>.</summary>
        internal static bool IsLegacySlotDummyInventory(Item[] inv) =>
            inv != null && inv.Length == 11;

        internal static bool IsPlayerItemInventory(Item[] inv) {
            Player player = Main.LocalPlayer;
            if (player == null || inv == null)
                return false;

            return ReferenceEquals(inv, player.inventory)
                || ReferenceEquals(inv, player.armor)
                || ReferenceEquals(inv, player.dye)
                || ReferenceEquals(inv, player.miscEquips)
                || ReferenceEquals(inv, player.miscDyes);
        }

        internal static bool ShouldSuppressTransferMotion(Item[] inv) {
            if (inv == null)
                return false;

            if (IsLegacySlotDummyInventory(inv))
                return true;

            return IsMagicStorageUiOpen() && !IsPlayerItemInventory(inv);
        }
    }
}
