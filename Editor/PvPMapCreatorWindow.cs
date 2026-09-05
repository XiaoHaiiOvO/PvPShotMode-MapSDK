#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace PvPShotMode.MapSDK.Editor
{
    /// <summary>
    /// PvP 地图创建编辑器窗口。
    /// 菜单: PvPShotMode → 创建新地图模版
    /// </summary>
    public class PvPMapCreatorWindow : EditorWindow
    {
        private string _mapId = "my_map";
        private string _displayName = "My Custom Map";
        private string _author = "MapMaker";
        private string _description = "自定义 PvP 地图";
        private bool _supportDemolition = true;
        private bool _supportTDM = true;
        private bool _supportFFA = false;
        private int _ctSpawnCount = 5;
        private int _tSpawnCount = 5;
        private int _ffaSpawnCount = 8;

        [MenuItem("PvPShotMode/创建新地图模版", false, 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<PvPMapCreatorWindow>("PvP 地图创建器");
            window.minSize = new Vector2(400, 520);
        }

        private void OnGUI()
        {
            GUILayout.Label("★ PvP Shot Mode 地图模版创建器 ★", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("地图基本信息", EditorStyles.boldLabel);
            _mapId = EditorGUILayout.TextField("地图 ID (英文)", _mapId);
            _displayName = EditorGUILayout.TextField("显示名称", _displayName);
            _author = EditorGUILayout.TextField("作者", _author);
            _description = EditorGUILayout.TextField("简介", _description);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("支持的游戏模式", EditorStyles.boldLabel);
            _supportDemolition = EditorGUILayout.Toggle("爆破模式 (demolition)", _supportDemolition);
            _supportTDM = EditorGUILayout.Toggle("团队竞技 (team_deathmatch)", _supportTDM);
            _supportFFA = EditorGUILayout.Toggle("个人死斗 (free_for_all)", _supportFFA);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("重生点数量", EditorStyles.boldLabel);
            _ctSpawnCount = EditorGUILayout.IntSlider("CT 重生点", _ctSpawnCount, 1, 16);
            _tSpawnCount = EditorGUILayout.IntSlider("T 重生点", _tSpawnCount, 1, 16);
            if (_supportFFA)
                _ffaSpawnCount = EditorGUILayout.IntSlider("FFA 重生点", _ffaSpawnCount, 2, 32);

            EditorGUILayout.Space(12);

            EditorGUILayout.HelpBox(
                "点击下方按钮后，将在当前场景中自动创建完整的地图模版层级结构：\n" +
                "• CT/T/FFA 重生点（带半透明彩色 Gizmo 球）\n" +
                "• 空气墙组（白色半透明方块）\n" +
                "• 武器购买墙锚点\n" +
                "• 包点区域（黄色半透明球+BoxCollider）\n" +
                "• MapInfo TextAsset（JSON 元信息）\n\n" +
                "创建后您只需添加场景模型即可。",
                MessageType.Info);

            EditorGUILayout.Space(6);

            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.4f);
            if (GUILayout.Button("★ 一键生成地图模版 ★", GUILayout.Height(40)))
            {
                CreateMapTemplate();
            }
            GUI.backgroundColor = Color.white;
        }

        private void CreateMapTemplate()
        {
            // 根节点
            GameObject mapRoot = new GameObject($"MapRoot_{_mapId}");
            Undo.RegisterCreatedObjectUndo(mapRoot, "Create PvP Map Template");

            // GamePlay 节点
            GameObject gameplay = CreateChild(mapRoot, "GamePlay");

            // CT 重生点
            GameObject ctSpawns = CreateChild(gameplay, "TeamCT_respawn_points");
            for (int i = 0; i < _ctSpawnCount; i++)
            {
                GameObject spawn = CreateChild(ctSpawns, $"Spawn_CT_{(i + 1):D2}");
                spawn.transform.localPosition = new Vector3(i * 2f, 0, 0);
                var marker = spawn.AddComponent<SpawnPointMarker>();
                SerializedObject so = new SerializedObject(marker);
                so.FindProperty("team").enumValueIndex = 0; // CT
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // T 重生点
            GameObject tSpawns = CreateChild(gameplay, "TeamT_respawn_points");
            for (int i = 0; i < _tSpawnCount; i++)
            {
                GameObject spawn = CreateChild(tSpawns, $"Spawn_T_{(i + 1):D2}");
                spawn.transform.localPosition = new Vector3(i * 2f, 0, 20f);
                var marker = spawn.AddComponent<SpawnPointMarker>();
                SerializedObject so = new SerializedObject(marker);
                so.FindProperty("team").enumValueIndex = 1; // T
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // FFA 重生点
            if (_supportFFA)
            {
                GameObject ffaSpawns = CreateChild(gameplay, "FFA_respawn_points");
                for (int i = 0; i < _ffaSpawnCount; i++)
                {
                    float angle = (360f / _ffaSpawnCount) * i * Mathf.Deg2Rad;
                    GameObject spawn = CreateChild(ffaSpawns, $"Spawn_FFA_{(i + 1):D2}");
                    spawn.transform.localPosition = new Vector3(Mathf.Cos(angle) * 10f, 0, Mathf.Sin(angle) * 10f);
                    var marker = spawn.AddComponent<SpawnPointMarker>();
                    SerializedObject so = new SerializedObject(marker);
                    so.FindProperty("team").enumValueIndex = 2; // FFA
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            // 空气墙组
            GameObject barriers = CreateChild(gameplay, "回合开始空气墙组");
            for (int i = 0; i < 3; i++)
            {
                GameObject barrier = CreateChild(barriers, $"Barrier_{(i + 1):D2}");
                barrier.transform.localPosition = new Vector3(i * 4f, 1f, 10f);
                barrier.AddComponent<BarrierGroupMarker>();
            }

            // 武器墙
            GameObject weaponShops = CreateChild(gameplay, "武器墙位置");
            GameObject ctShop = CreateChild(weaponShops, "警");
            ctShop.transform.localPosition = new Vector3(-5f, 1f, 2f);
            var ctShopMarker = ctShop.AddComponent<WeaponShopMarker>();
            SerializedObject ctSo = new SerializedObject(ctShopMarker);
            ctSo.FindProperty("team").enumValueIndex = 0; // CT
            ctSo.ApplyModifiedPropertiesWithoutUndo();

            GameObject tShop = CreateChild(weaponShops, "匪");
            tShop.transform.localPosition = new Vector3(-5f, 1f, 18f);
            var tShopMarker = tShop.AddComponent<WeaponShopMarker>();
            SerializedObject tSo = new SerializedObject(tShopMarker);
            tSo.FindProperty("team").enumValueIndex = 1; // T
            tSo.ApplyModifiedPropertiesWithoutUndo();

            // 包点（爆破模式）
            if (_supportDemolition)
            {
                GameObject bombsites = CreateChild(gameplay, "包点");

                GameObject siteA = CreateChild(bombsites, "A");
                siteA.transform.localPosition = new Vector3(10f, 0, 5f);
                siteA.AddComponent<BombsiteMarker>();
                var colA = siteA.GetComponent<BoxCollider>();
                colA.size = new Vector3(4f, 2f, 4f);
                colA.isTrigger = true;

                GameObject siteB = CreateChild(bombsites, "B");
                siteB.transform.localPosition = new Vector3(-10f, 0, 5f);
                siteB.AddComponent<BombsiteMarker>();
                var colB = siteB.GetComponent<BoxCollider>();
                colB.size = new Vector3(4f, 2f, 4f);
                colB.isTrigger = true;
            }

            // MapInfo TextAsset
            MapInfo info = new MapInfo
            {
                mapId = _mapId,
                displayName = _displayName,
                author = _author,
                description = _description,
                supportedModes = BuildSupportedModes(),
                bombsiteNames = _supportDemolition ? new[] { "A", "B" } : new string[0]
            };

            string json = JsonUtility.ToJson(info, true);
            string assetPath = $"Assets/{_mapId}_MapInfo.asset";
            TextAsset textAsset = new TextAsset(json);
            AssetDatabase.CreateAsset(textAsset, assetPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"[PvP MapSDK] 地图模版 '{_displayName}' 已创建！\n" +
                     $"MapInfo 保存于: {assetPath}\n" +
                     $"请将场景模型添加到 MapRoot_{_mapId} 节点下。");

            Selection.activeGameObject = mapRoot;
            EditorGUIUtility.PingObject(mapRoot);
        }

        private string[] BuildSupportedModes()
        {
            var modes = new System.Collections.Generic.List<string>();
            if (_supportDemolition) modes.Add("demolition");
            if (_supportTDM) modes.Add("team_deathmatch");
            if (_supportFFA) modes.Add("free_for_all");
            return modes.ToArray();
        }

        private static GameObject CreateChild(GameObject parent, string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            return child;
        }
    }
}
#endif
