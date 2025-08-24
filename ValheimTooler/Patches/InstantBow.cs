using HarmonyLib;
using ValheimTooler.Core;
using ValheimTooler.Utils;

namespace ValheimTooler.Patches
{
    [HarmonyPatch(typeof(Player), "UpdateAttack")]
    class InstantBow
    {
        private static void Prefix(Player __instance)
        {
            if (!PlayerHacks.s_instantBow || __instance != Player.m_localPlayer)
            {
                return;
            }

            Attack currentAttack = __instance.GetFieldValue<Attack>("m_currentAttack");
            if (currentAttack != null && currentAttack.m_bowDraw)
            {
                __instance.SetFieldValue("m_attackDrawPercentage", 1f);
                __instance.SetFieldValue("m_attackDrawTime", currentAttack.m_drawDurationMin);
            }
        }
    }
}

