using HarmonyLib;
using UnityEngine;
using ValheimTooler.Core;

namespace ValheimTooler.Patches
{
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.GetAmmoItem))]
    class ArrowOverrideGetAmmo
    {
        private static void Postfix(Humanoid __instance, ref ItemDrop.ItemData __result)
        {
            if (__instance != Player.m_localPlayer)
            {
                return;
            }

            if (PlayerHacks.s_arrowTypeIdx <= 0)
            {
                return;
            }

            if (__result != null)
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

    [HarmonyPatch(typeof(Attack), nameof(Attack.HaveAmmo))]
    class ArrowOverrideHaveAmmo
    {
        private static bool Prefix(Humanoid character, ItemDrop.ItemData weapon, ref bool __result)
        {
            if (character != Player.m_localPlayer)
            {
                return true;
            }

            if (PlayerHacks.s_arrowTypeIdx <= 0)
            {
                return true;
            }

            if (weapon == null || weapon.m_shared.m_ammoType != "arrow")
            {
                return true;
            }

            __result = true;
            return false;
        }
    }
}
