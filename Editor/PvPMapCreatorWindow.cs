using UnityEditor;
using UnityEngine;
using HowToFish.PvPShotMode.Map;
namespace PvPShotMode.MapSDK.Editor
{
    public sealed class PvPMapCreatorWindow : EditorWindow
    {
        private string mapId = "my_map", displayName = "我的地图";
        private bool demolition = true, teams = true, ffa = true;
        private bool infection;
        [MenuItem("PvPShotMode/新建地图模板")]
        public static void Open() => GetWindow<PvPMapCreatorWindow>("新建 PvP 地图");
        private void OnGUI()
        {
            mapId = EditorGUILayout.TextField("地图 ID", mapId);
            displayName = EditorGUILayout.TextField("显示名称", displayName);
            demolition = EditorGUILayout.Toggle("爆破", demolition);
            teams = EditorGUILayout.Toggle("团队竞技", teams);
            ffa = EditorGUILayout.Toggle("个人死斗", ffa);
            infection = EditorGUILayout.Toggle("生化模式", infection);
            if (infection) EditorGUILayout.HelpBox("生化：16个人类出生点、10个丧尸复活点、5个空投点、一个人类武器墙。无包点和准备空气墙。空投需有15米无遮挡上空。", MessageType.Info);
            EditorGUILayout.HelpBox("模板只提供 Gameplay 标记和空气墙碰撞体。请添加可行走地面、场景模型，并调整出生点与包点。蓝球=CT，红球=T，紫球=FFA，黄框=包点，青框=准备期空气墙，绿/青色槽位阵列=武器墙。武器墙本地 +Z 为展示正面，直接调整标记位置和旋转。", MessageType.Info);
            using (new EditorGUI.DisabledScope(!demolition && !teams && !ffa && !infection))
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
                if (infection) modes.Add(GameModeIds.Infection);
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
                CreateTestPlayer(root.transform);
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
                if (info.SupportsMode(GameModeIds.Infection))
                {
                    Spawns(game, info.humanSpawnRoot, SpawnPointMarker.SpawnTeam.Human, -12, 16);
                    Spawns(game, info.zombieSpawnRoot, SpawnPointMarker.SpawnTeam.Zombie, 12, 10);
                    var drops = Node(game, info.airdropRoot, Vector3.zero);
                    for (int i = 0; i < 5; i++) Node(drops, "Airdrop_" + (i + 1), new Vector3(i * 6 - 12, 0, 12)).gameObject.AddComponent<AirdropPointMarker>();
                    var shop = game.Find(info.weaponShopRoot) ?? Node(game, info.weaponShopRoot, Vector3.zero);
                    var wall = Node(shop, info.humanWeaponShopChild, new Vector3(-10, 1.65f, -5));
                    wall.gameObject.AddComponent<WeaponShopMarker>();
                }
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
        private static void CreateTestPlayer(Transform parent)
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "player";
            player.transform.SetParent(parent, false);
            player.transform.localPosition = new Vector3(0, 1, 0);
            player.transform.localRotation = Quaternion.identity;
            player.transform.localScale = Vector3.one;

            var body = player.AddComponent<Rigidbody>();
            body.mass = 80f;
            body.constraints = RigidbodyConstraints.FreezeRotation;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var cameraObject = new GameObject("PlayerCamera");
            cameraObject.transform.SetParent(player.transform, false);
            cameraObject.transform.localPosition = new Vector3(0, .65f, 0);
            var camera = cameraObject.AddComponent<Camera>();
            camera.nearClipPlane = .05f;
            camera.fieldOfView = 70f;
            var controller = player.AddComponent<global::PvPShotMode.MapSDK.SimpleFPSController>();
            controller.viewCamera = camera;
        }
        private static void Spawns(Transform parent, string name, SpawnPointMarker.SpawnTeam team, float x, int count = 8)
        {
            var group = Node(parent, name, Vector3.zero);
            for (int i = 0; i < count; i++)
            {
                var point = Node(group, "Spawn_" + (i + 1), new Vector3(x + (i % 2) * 2, 0, (i / 2) * 2 - 3));
                point.gameObject.AddComponent<SpawnPointMarker>().team = team;
            }
        }
    }
}
