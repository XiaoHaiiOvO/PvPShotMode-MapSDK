using UnityEditor;
using UnityEngine;
using HowToFish.PvPShotMode.Map;
namespace PvPShotMode.MapSDK.Editor
{
    public sealed class PvPMapCreatorWindow : EditorWindow
    {
        private string mapId = "my_map", displayName = "我的地图";
        private bool demolition = true, teams = true, ffa = true;
        [MenuItem("PvPShotMode/新建地图模板")]
        public static void Open() => GetWindow<PvPMapCreatorWindow>("新建 PvP 地图");
        private void OnGUI()
        {
            mapId = EditorGUILayout.TextField("地图 ID", mapId);
            displayName = EditorGUILayout.TextField("显示名称", displayName);
            demolition = EditorGUILayout.Toggle("爆破", demolition);
            teams = EditorGUILayout.Toggle("团队竞技", teams);
            ffa = EditorGUILayout.Toggle("个人死斗", ffa);
            EditorGUILayout.HelpBox("模板只提供 Gameplay 标记和空气墙碰撞体。请添加可行走地面、场景模型，并调整出生点与包点。蓝球=CT，红球=T，紫球=FFA，黄框=包点，青框=准备期空气墙，绿/青色槽位阵列=武器墙。武器墙本地 +Z 为展示正面，直接调整标记位置和旋转。", MessageType.Info);
            using (new EditorGUI.DisabledScope(!demolition && !teams && !ffa))
            if (GUILayout.Button("一键生成并保存模板"))
            {
                string folder = EditorUtility.SaveFolderPanel("选择 Assets 下的目录", Application.dataPath, "");
                if (string.IsNullOrEmpty(folder)) return;
                string path = FileUtil.GetProjectRelativePath(folder);
                if (!(path == "Assets" || path.StartsWith("Assets/"))) { EditorUtility.DisplayDialog("错误", "请选择工程 Assets 内的目录", "确定"); return; }
                var modes = new System.Collections.Generic.List<string>();
                if (demolition) modes.Add(GameModeIds.Demolition);
                if (teams) modes.Add(GameModeIds.TeamDeathmatch);
                if (ffa) modes.Add(GameModeIds.FreeForAll);
                var info = new MapInfo { mapId = mapId.Trim(), displayName = displayName, supportedModes = modes.ToArray() };
                Selection.activeObject = CreateTemplate(path, info);
            }
        }
        public static PvPMapDefinition CreateTemplate(string folder, MapInfo info)
        {
            if (string.IsNullOrWhiteSpace(info.mapId) || System.Text.RegularExpressions.Regex.IsMatch(info.mapId, "[^a-z0-9_-]"))
                throw new System.ArgumentException("地图 ID 只允许小写英文、数字、下划线和连字符");
            var root = new GameObject(info.mapId);
            try
            {
                var game = Node(root.transform, info.gameplayRoot, Vector3.zero);
                if (info.SupportsMode(GameModeIds.Demolition) || info.SupportsMode(GameModeIds.TeamDeathmatch))
                {
                    Spawns(game, info.teamASpawnRoot, SpawnPointMarker.SpawnTeam.CT, -10);
                    Spawns(game, info.teamBSpawnRoot, SpawnPointMarker.SpawnTeam.T, 10);
                    var barriers = Node(game, info.barrierRoot, Vector3.zero);
                    foreach (float x in new[] { -6f, 6f })
                    {
                        var wall = Node(barriers, "Barrier_" + x, new Vector3(x, 2, 0));
                        wall.gameObject.AddComponent<BarrierGroupMarker>();
                        wall.GetComponent<BoxCollider>().size = new Vector3(.3f, 4, 12);
                    }
                    var shop = Node(game, info.weaponShopRoot, Vector3.zero);
                    foreach (var entry in new[] { (info.weaponShopTeamAChild, -11f), (info.weaponShopTeamBChild, 11f) })
                    {
                        var anchor = Node(shop, entry.Item1, new Vector3(entry.Item2, 1.65f, 4));
                        anchor.localRotation = Quaternion.Euler(0, 180, 0);
                        anchor.gameObject.AddComponent<WeaponShopMarker>();
                    }
                }
                if (info.SupportsMode(GameModeIds.FreeForAll)) Spawns(game, info.freeForAllSpawnRoot, SpawnPointMarker.SpawnTeam.FFA, 0);
                if (info.SupportsMode(GameModeIds.Demolition))
                {
                    var sites = Node(game, info.bombsiteRoot, Vector3.zero);
                    for (int i = 0; i < info.bombsiteNames.Length; i++)
                    {
                        var site = Node(sites, info.bombsiteNames[i], new Vector3(i * 10 - 5, 1, 10));
                        site.gameObject.AddComponent<BombsiteMarker>();
                        var box = site.GetComponent<BoxCollider>(); box.size = new Vector3(5, 2, 5); box.isTrigger = true;
                    }
                }
                string prefabPath = AssetDatabase.GenerateUniqueAssetPath(folder + "/" + info.mapId + ".prefab");
                var definition = CreateInstance<PvPMapDefinition>();
                definition.info = info; definition.prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                AssetDatabase.CreateAsset(definition, AssetDatabase.GenerateUniqueAssetPath(folder + "/" + info.mapId + "_Definition.asset"));
                AssetDatabase.SaveAssets(); return definition;
            }
            finally { DestroyImmediate(root); }
        }
        private static Transform Node(Transform parent, string name, Vector3 position)
        {
            var t = new GameObject(name).transform; t.SetParent(parent, false); t.localPosition = position; return t;
        }
        private static void Spawns(Transform parent, string name, SpawnPointMarker.SpawnTeam team, float x)
        {
            var group = Node(parent, name, Vector3.zero);
            for (int i = 0; i < 8; i++)
            {
                var point = Node(group, "Spawn_" + (i + 1), new Vector3(x + (i % 2) * 2, 0, (i / 2) * 2 - 3));
                point.gameObject.AddComponent<SpawnPointMarker>().team = team;
            }
        }
    }
}
