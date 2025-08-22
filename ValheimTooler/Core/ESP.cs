using System;
using System.Collections.Generic;
using UnityEngine;
using ValheimTooler.UI;
using ValheimTooler.Utils;

namespace ValheimTooler.Core
{
    public static class ESP
    {
        private static readonly Color s_playersColor = Color.magenta;
        private static readonly Color s_monstersAndOthersColor = Color.red;
        private static readonly Color s_tamedMonstersColor = new Color(1, 0.3f, 0, 1); // Orange
        private static readonly Color s_pickablesColor = new Color(0.13f, 0.58f, 0.89f, 1); // Light blue
        private static readonly Color s_dropsColor = new Color(0.13f, 0.72f, 0.11f, 1); // Light green
        private static readonly Color s_depositsColor = Color.yellow;

        private static readonly List<Character> s_characters = new List<Character>();
        private static readonly List<Pickable> s_pickables = new List<Pickable>();
        private static readonly List<PickableItem> s_pickableItems = new List<PickableItem>();
        private static readonly List<ItemDrop> s_drops = new List<ItemDrop>();
        private static readonly List<Destructible> s_depositsDestructible = new List<Destructible>();
        private static readonly List<MineRock5> s_mineRock5s = new List<MineRock5>();

        private static float s_updateTimer = 0f;
        private static readonly float s_updateTimerInterval = 1.5f;

        public static bool s_showPlayerESP = false;
        public static bool s_showMonsterESP = false;
        public static bool s_showDroppedESP = false;
        public static bool s_showDepositESP = false;
        public static bool s_showPickableESP = false;
        public static bool s_xray = false;

        private static readonly Dictionary<Renderer, GameObject> s_xrayOutlines = new Dictionary<Renderer, GameObject>();
        private static Material s_xrayMaterial;

        static ESP()
        {
            Shader shader = Shader.Find("Unlit/Color");
            if (shader == null)
            {
                shader = Shader.Find("Hidden/Internal-Colored");
            }
            if (shader != null)
            {
                s_xrayMaterial = new Material(shader);
                s_xrayMaterial.color = Color.cyan;
                s_xrayMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
                s_xrayMaterial.SetInt("_ZWrite", 0);
                s_xrayMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Front);
                s_xrayMaterial.EnableKeyword("_EMISSION");
                s_xrayMaterial.SetColor("_EmissionColor", Color.cyan);
                s_xrayMaterial.renderQueue = 5000;
                UnityEngine.Object.DontDestroyOnLoad(s_xrayMaterial);
            }
        }

        public static void Start()
        {
            return;
        }

        public static void Update()
        {
            if (Time.time >= s_updateTimer)
            {
                if (s_xray)
                {
                    ClearAllXRay();
                }

                s_characters.Clear();
                s_pickables.Clear();
                s_pickableItems.Clear();
                s_drops.Clear();
                s_depositsDestructible.Clear();
                s_mineRock5s.Clear();

                Camera mainCamera = global::Utils.GetMainCamera();

                if (ESP.s_showPlayerESP || ESP.s_showMonsterESP)
                {
                    List<Character> characters = Character.GetAllCharacters();

                    if (characters != null && mainCamera != null && Player.m_localPlayer != null)
                    {
                        foreach (Character character in characters)
                        {
                            var distance = Vector3.Distance(mainCamera.transform.position, character.transform.position);

                            if (character.IsPlayer() && ((Player)character).GetPlayerID() == Player.m_localPlayer.GetPlayerID())
                            {
                                continue;
                            }

                            if (distance > 2 && (!ConfigManager.s_espRadiusEnabled.Value || distance < ConfigManager.s_espRadius.Value))
                            {
                                s_characters.Add(character);
                            }
                        }
                    }
                }

                if (ESP.s_showPickableESP)
                {
                    var pickables = UnityEngine.Object.FindObjectsOfType<Pickable>();

                    if (pickables != null && mainCamera != null)
                    {
                        foreach (Pickable pickable in pickables)
                        {
                            var distance = Vector3.Distance(mainCamera.transform.position, pickable.transform.position);

                            if (distance > 2 && (!ConfigManager.s_espRadiusEnabled.Value || distance < ConfigManager.s_espRadius.Value))
                            {
                                s_pickables.Add(pickable);
                            }
                        }
                    }

                    var pickableItems = UnityEngine.Object.FindObjectsOfType<PickableItem>();

                    if (pickableItems != null && mainCamera != null)
                    {
                        foreach (PickableItem pickableItem in pickableItems)
                        {
                            var distance = Vector3.Distance(mainCamera.transform.position, pickableItem.transform.position);

                            if (distance > 2 && (!ConfigManager.s_espRadiusEnabled.Value || distance < ConfigManager.s_espRadius.Value))
                            {
                                s_pickableItems.Add(pickableItem);
                            }
                        }
                    }
                }

                if (ESP.s_showDroppedESP)
                {
                    var itemDrops = UnityEngine.Object.FindObjectsOfType<ItemDrop>();

                    if (itemDrops != null && mainCamera != null)
                    {
                        foreach (ItemDrop itemDrop in itemDrops)
                        {
                            var distance = Vector3.Distance(mainCamera.transform.position, itemDrop.transform.position);

                            if (distance > 2 && (!ConfigManager.s_espRadiusEnabled.Value || distance < ConfigManager.s_espRadius.Value))
                            {
                                s_drops.Add(itemDrop);
                            }
                        }
                    }
                }

                if (ESP.s_showDepositESP)
                {
                    var mineRock5s = UnityEngine.Object.FindObjectsOfType<MineRock5>();

                    if (mineRock5s != null && mainCamera != null)
                    {
                        foreach (MineRock5 mineRock5 in mineRock5s)
                        {
                            string name = mineRock5.GetHoverText().ToLower();

                            if (name.Contains("rock") || name.Length == 0)
                                continue;

                            var distance = Vector3.Distance(mainCamera.transform.position, mineRock5.transform.position);

                            if (distance > 2 && (!ConfigManager.s_espRadiusEnabled.Value || distance < ConfigManager.s_espRadius.Value))
                            {
                                s_mineRock5s.Add(mineRock5);
                            }
                        }
                    }


                    var destructibles = UnityEngine.Object.FindObjectsOfType<Destructible>();

                    if (destructibles != null && mainCamera != null)
                    {
                        foreach (Destructible destructible in destructibles)
                        {
                            HoverText component = destructible.GetComponent<HoverText>();
                            if (component == null)
                                continue;
                            string text = component.m_text.ToLower();

                            if (!text.Contains("deposit") && !text.Contains("piece_mudpile"))
                            {
                                continue;
                            }

                            var distance = Vector3.Distance(mainCamera.transform.position, destructible.transform.position);

                            if (distance > 2 && (!ConfigManager.s_espRadiusEnabled.Value || distance < ConfigManager.s_espRadius.Value))
                            {
                                s_depositsDestructible.Add(destructible);
                            }
                        }
                    }
                }

                s_updateTimer = Time.time + s_updateTimerInterval;
            }
        }

        public static void SetXRay(bool enabled)
        {
            s_xray = enabled;

            if (!s_xray)
            {
                ClearAllXRay();
            }
        }

        private static void ApplyXRayOutline(GameObject obj)
        {
            if (obj == null || s_xrayMaterial == null)
            {
                return;
            }

            foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>())
            {
                if (s_xrayOutlines.ContainsKey(renderer))
                {
                    continue;
                }

                GameObject outline = new GameObject("ESP_XRAY");
                outline.transform.SetParent(renderer.transform, false);
                outline.transform.localScale = Vector3.one * 1.03f;

                MeshFilter mf = renderer.GetComponent<MeshFilter>();
                MeshRenderer mr = renderer as MeshRenderer;
                SkinnedMeshRenderer smr = renderer as SkinnedMeshRenderer;

                if (mf != null && mr != null)
                {
                    var cFilter = outline.AddComponent<MeshFilter>();
                    cFilter.sharedMesh = mf.sharedMesh;
                    var cRenderer = outline.AddComponent<MeshRenderer>();
                    cRenderer.material = s_xrayMaterial;
                    s_xrayOutlines[renderer] = outline;
                }
                else if (smr != null)
                {
                    var cRenderer = outline.AddComponent<SkinnedMeshRenderer>();
                    cRenderer.sharedMesh = smr.sharedMesh;
                    cRenderer.bones = smr.bones;
                    cRenderer.rootBone = smr.rootBone;
                    cRenderer.material = s_xrayMaterial;
                    s_xrayOutlines[renderer] = outline;
                }
                else
                {
                    UnityEngine.Object.Destroy(outline);
                }
            }
        }

        private static void RemoveXRayOutline(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>())
            {
                if (s_xrayOutlines.TryGetValue(renderer, out GameObject outline))
                {
                    if (outline != null)
                    {
                        UnityEngine.Object.Destroy(outline);
                    }
                    s_xrayOutlines.Remove(renderer);
                }
            }
        }

        private static void ClearAllXRay()
        {
            foreach (var kvp in s_xrayOutlines)
            {
                if (kvp.Value != null)
                {
                    UnityEngine.Object.Destroy(kvp.Value);
                }
            }
            s_xrayOutlines.Clear();
        }

        public static void DisplayGUI()
        {
            Camera mainCamera = global::Utils.GetMainCamera();

            if (mainCamera != null && Player.m_localPlayer != null)
            {
                var main = mainCamera;
                var labelSkin = new GUIStyle(InterfaceMaker.CustomSkin.label);

                if (ESP.s_showPlayerESP || ESP.s_showMonsterESP)
                {
                    foreach (Character character in s_characters)
                    {
                        if (character == null || (!ESP.s_showMonsterESP && !character.IsPlayer()))
                        {
                            continue;
                        }
                        if (!ESP.s_xray && !HasLineOfSight(main, character.transform))
                        {
                            continue;
                        }
                        if (ESP.s_xray)
                        {
                            ApplyXRayOutline(character.gameObject);
                        }
                        else
                        {
                            RemoveXRayOutline(character.gameObject);
                        }
                        Vector3 vector = main.WorldToScreenPointScaled(character.transform.position);

                        if (vector.z > -1)
                        {
                            if (character.IsPlayer() && ESP.s_showPlayerESP)
                            {
                                string espLabel = ((Player)character).GetPlayerName() + $" [{(int)vector.z}]";
                                labelSkin.normal.textColor = s_playersColor;
                                GUI.Label(new Rect((int)vector.x - 10, Screen.height - vector.y - 5, 150, 40), espLabel, labelSkin);
                            }
                            else if (!character.IsPlayer() && ESP.s_showMonsterESP)
                            {
                                string espLabel = character.GetHoverName() + $" [{(int)vector.z}]";
                                labelSkin.normal.textColor = character.IsTamed() ? s_tamedMonstersColor : s_monstersAndOthersColor;
                                GUI.Label(new Rect((int)vector.x - 10, Screen.height - vector.y - 5, 150, 40), espLabel, labelSkin);
                            }
                        }
                    }
                }

                if (ESP.s_showPickableESP)
                {
                    labelSkin.normal.textColor = s_pickablesColor;
                    foreach (Pickable pickable in s_pickables)
                    {
                        if (pickable == null)
                        {
                            continue;
                        }
                        if (!ESP.s_xray && !HasLineOfSight(main, pickable.transform))
                        {
                            continue;
                        }
                        if (ESP.s_xray)
                        {
                            ApplyXRayOutline(pickable.gameObject);
                        }
                        else
                        {
                            RemoveXRayOutline(pickable.gameObject);
                        }
                        Vector3 vector = main.WorldToScreenPointScaled(pickable.transform.position);

                        if (vector.z > -1)
                        {
                            string espLabel = $"{Localization.instance.Localize(pickable.GetHoverName())} [{(int)vector.z}]";

                            GUI.Label(new Rect((int)vector.x - 5, Screen.height - vector.y - 5, 150, 40), espLabel, labelSkin);
                        }
                    }
                    foreach (PickableItem pickableItem in s_pickableItems)
                    {
                        if (pickableItem == null)
                        {
                            continue;
                        }
                        if (!ESP.s_xray && !HasLineOfSight(main, pickableItem.transform))
                        {
                            continue;
                        }
                        if (ESP.s_xray)
                        {
                            ApplyXRayOutline(pickableItem.gameObject);
                        }
                        else
                        {
                            RemoveXRayOutline(pickableItem.gameObject);
                        }
                        Vector3 vector = main.WorldToScreenPointScaled(pickableItem.transform.position);

                        if (vector.z > -1)
                        {
                            string espLabel = $"{Localization.instance.Localize(pickableItem.GetHoverName())} [{(int)vector.z}]";

                            GUI.Label(new Rect((int)vector.x - 5, Screen.height - vector.y - 5, 150, 40), espLabel, labelSkin);
                        }
                    }
                }

                if (ESP.s_showDroppedESP)
                {
                    labelSkin.normal.textColor = s_dropsColor;
                    foreach (ItemDrop itemDrop in s_drops)
                    {
                        if (itemDrop == null)
                        {
                            continue;
                        }
                        if (!ESP.s_xray && !HasLineOfSight(main, itemDrop.transform))
                        {
                            continue;
                        }
                        if (ESP.s_xray)
                        {
                            ApplyXRayOutline(itemDrop.gameObject);
                        }
                        else
                        {
                            RemoveXRayOutline(itemDrop.gameObject);
                        }
                        Vector3 vector = main.WorldToScreenPointScaled(itemDrop.transform.position);

                        if (vector.z > -1)
                        {
                            string espLabel = $"{Localization.instance.Localize(itemDrop.GetHoverName())} [{(int)vector.z}]";

                            GUI.Label(new Rect((int)vector.x - 5, Screen.height - vector.y - 5, 150, 40), espLabel, labelSkin);
                        }
                    }
                }

                if (ESP.s_showDepositESP)
                {
                    labelSkin.normal.textColor = s_depositsColor;

                    foreach (Destructible depositDestructible in s_depositsDestructible)
                    {
                        if (depositDestructible == null)
                        {
                            continue;
                        }
                        if (!ESP.s_xray && !HasLineOfSight(main, depositDestructible.transform))
                        {
                            continue;
                        }
                        if (ESP.s_xray)
                        {
                            ApplyXRayOutline(depositDestructible.gameObject);
                        }
                        else
                        {
                            RemoveXRayOutline(depositDestructible.gameObject);
                        }
                        Vector3 vector = main.WorldToScreenPointScaled(depositDestructible.transform.position);

                        if (vector.z > -1)
                        {
                            string name = depositDestructible.GetComponent<HoverText>().GetHoverName();
                            string espLabel = $"{name} [{(int)vector.z}]";

                            GUI.Label(new Rect((int)vector.x - 5, Screen.height - vector.y - 5, 150, 40), espLabel, labelSkin);
                        }
                    }

                    foreach (MineRock5 mineRock5 in s_mineRock5s)
                    {
                        if (mineRock5 == null)
                        {
                            continue;
                        }
                        if (!ESP.s_xray && !HasLineOfSight(main, mineRock5.transform))
                        {
                            continue;
                        }
                        if (ESP.s_xray)
                        {
                            ApplyXRayOutline(mineRock5.gameObject);
                        }
                        else
                        {
                            RemoveXRayOutline(mineRock5.gameObject);
                        }
                        Vector3 vector = main.WorldToScreenPointScaled(mineRock5.transform.position);

                        if (vector.z > -1)
                        {
                            string espLabel = $"{mineRock5.GetHoverText()} [{(int)vector.z}]";

                            GUI.Label(new Rect((int)vector.x - 5, Screen.height - vector.y - 5, 150, 40), espLabel, labelSkin);
                        }
                    }
                }
            }
        }

        private static bool HasLineOfSight(Camera camera, Transform target)
        {
            Vector3 origin = camera.transform.position;
            Vector3 targetPos = target.position;

            if (UnityEngine.Physics.Linecast(origin, targetPos, out UnityEngine.RaycastHit hit, UnityEngine.Physics.DefaultRaycastLayers, UnityEngine.QueryTriggerInteraction.Ignore))
            {
                return hit.transform == target || hit.transform.IsChildOf(target);
            }

            return true;
        }
    }
}
