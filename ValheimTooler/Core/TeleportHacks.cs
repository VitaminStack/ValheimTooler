using System;
using UnityEngine;
using ValheimTooler.Utils;

namespace ValheimTooler.Core
{
    public static class TeleportHacks
    {
        private static string s_radiusText = "10";
        private static string s_filterText = string.Empty;

        public static void Start()
        {
        }

        public static void Update()
        {
        }

        public static void DisplayGUI()
        {
            GUILayout.BeginVertical(VTLocalization.instance.Localize("$vt_teleport_title"), GUI.skin.box, GUILayout.ExpandWidth(false));
            {
                GUILayout.Space(EntryPoint.s_boxSpacing);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(VTLocalization.instance.Localize("$vt_teleport_radius") + " :", GUILayout.ExpandWidth(false));
                    s_radiusText = GUILayout.TextField(s_radiusText, GUILayout.ExpandWidth(true));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(VTLocalization.instance.Localize("$vt_teleport_filter") + " :", GUILayout.ExpandWidth(false));
                    s_filterText = GUILayout.TextField(s_filterText, GUILayout.ExpandWidth(true));
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button(VTLocalization.instance.Localize("$vt_teleport_button")))
                {
                    if (float.TryParse(s_radiusText, out float radius) && radius > 0f)
                    {
                        TeleportEntities(radius, s_filterText);
                    }
                }
            }
            GUILayout.EndVertical();
        }

        private static void TeleportEntities(float radius, string filter)
        {
            Player player = Player.m_localPlayer;
            if (player == null)
            {
                return;
            }

            Vector3 targetPosition = player.transform.position;
            string filterLower = filter.ToLowerInvariant();

            foreach (Character character in UnityEngine.Object.FindObjectsOfType<Character>())
            {
                if (character == null || character.IsPlayer())
                {
                    continue;
                }

                if (Vector3.Distance(character.transform.position, targetPosition) > radius)
                {
                    continue;
                }

                string name = character.gameObject.name.Replace("(Clone)", string.Empty);
                if (!string.IsNullOrEmpty(filterLower) && !name.ToLowerInvariant().Contains(filterLower))
                {
                    continue;
                }

                character.transform.position = targetPosition + UnityEngine.Random.insideUnitSphere;
            }

            foreach (ItemDrop item in UnityEngine.Object.FindObjectsOfType<ItemDrop>())
            {
                if (item == null)
                {
                    continue;
                }

                if (Vector3.Distance(item.transform.position, targetPosition) > radius)
                {
                    continue;
                }

                string name = item.gameObject.name.Replace("(Clone)", string.Empty);
                if (!string.IsNullOrEmpty(filterLower) && !name.ToLowerInvariant().Contains(filterLower))
                {
                    continue;
                }

                item.transform.position = targetPosition + UnityEngine.Random.insideUnitSphere;
            }
        }
    }
}
