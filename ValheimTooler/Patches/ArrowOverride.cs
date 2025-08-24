using HarmonyLib;
using UnityEngine;
using ValheimTooler.Core;

namespace ValheimTooler.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.GetAmmoItem))]
    class ArrowOverride
    {
        private static void Postfix(Player __instance, ItemDrop.ItemData weapon, ref ItemDrop.ItemData __result)
        {
            if (__instance != Player.m_localPlayer)
            {
                return;
            }

            if (PlayerHacks.s_arrowTypeIdx <= 0)
            {
                return;
            }

            if (weapon == null || weapon.m_shared.m_ammoType != "arrow")
            {
                return;
            }

            var arrowItem = PlayerHacks.GetSelectedArrowItem();
            if (arrowItem != null)
            {
                __result = arrowItem;
            }
        }
    }
}
