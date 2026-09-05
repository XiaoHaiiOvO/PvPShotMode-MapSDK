#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PvPShotMode.MapSDK.Editor
{
    /// <summary>
    /// 地图导出器 — 将预制体 + MapInfo 打包为 .map AssetBundle。
    /// 菜单: PvPShotMode → 导出地图
    /// </summary>
    public class PvPMapExporter : EditorWindow
    {
        private GameObject _mapPrefab;
        private TextAsset _mapInfoAsset;
        private string _outputDirectory = "";
        private Vector2 _scrollPos;
        private List<string> _validationMessages = new List<string>();

        [MenuItem("PvPShotMode/导出地图 (.map)", false, 200)]
        public static void ShowWindow()
        {
            var window = GetWindow<PvPMapExporter>("PvP 地图导出");
            window.minSize = new Vector2(420, 400);
        }

        private void OnGUI()
        {
            GUILayout.Label("★ PvP Shot Mode 地图导出器 ★", EditorStyles.boldLabel);
            EditorGUILayout.Space(6);

            _mapPrefab = (GameObject)EditorGUILayout.ObjectField("地图预制体", _mapPrefab, typeof(GameObject), true);
            _mapInfoAsset = (TextAsset)EditorGUILayout.ObjectField("MapInfo TextAsset", _mapInfoAsset, typeof(TextAsset), false);

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            _outputDirectory = EditorGUILayout.TextField("输出目录", _outputDirectory);
            if (GUILayout.Button("浏览", GUILayout.Width(50)))
            {
                string dir = EditorUtility.OpenFolderPanel("选择导出目录", _outputDirectory, "");
                if (!string.IsNullOrEmpty(dir)) _outputDirectory = dir;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);

            // 验证按钮
            if (GUILayout.Button("验证地图结构", GUILayout.Height(28)))
            {
                ValidateMap();
            }

            if (_validationMessages.Count > 0)
            {
                EditorGUILayout.Space(4);
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(120));
                foreach (string msg in _validationMessages)
                {
                    if (msg.StartsWith("✓"))
                        EditorGUILayout.HelpBox(msg, MessageType.Info);
                    else if (msg.StartsWith("⚠"))
                        EditorGUILayout.HelpBox(msg, MessageType.Warning);
                    else
                        EditorGUILayout.HelpBox(msg, MessageType.Error);
                }
                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.Space(8);

            GUI.backgroundColor = new Color(0.3f, 0.7f, 1f);
            GUI.enabled = _mapPrefab != null && _mapInfoAsset != null && !string.IsNullOrEmpty(_outputDirectory);
            if (GUILayout.Button("★ 一键导出 .map ★", GUILayout.Height(40)))
            {
                ExportMap();
            }
            GUI.enabled = true;
            GUI.backgroundColor = Color.white;
        }

        private void ValidateMap()
        {
            _validationMessages.Clear();

            if (_mapPrefab == null)
            {
                _validationMessages.Add("✘ 请指定地图预制体");
                return;
            }

            if (_mapInfoAsset == null)
            {
                _validationMessages.Add("✘ 请指定 MapInfo TextAsset");
                return;
            }

            MapInfo info;
            try
            {
                info = JsonUtility.FromJson<MapInfo>(_mapInfoAsset.text);
            }
            catch
            {
                _validationMessages.Add("✘ MapInfo JSON 解析失败");
                return;
            }

            _validationMessages.Add($"✓ mapId = {info.mapId}");
            _validationMessages.Add($"✓ displayName = {info.displayName}");
            _validationMessages.Add($"✓ supportedModes = [{string.Join(", ", info.supportedModes)}]");

            // 检查 GamePlay 节点
            Transform gameplay = _mapPrefab.transform.Find("GamePlay");
            if (gameplay == null)
            {
                _validationMessages.Add("✘ 缺少 'GamePlay' 子节点");
                return;
            }
            _validationMessages.Add("✓ GamePlay 节点存在");

            // 检查 CT 重生点
            Transform ctSpawns = gameplay.Find(info.teamASpawnRoot);
            if (ctSpawns == null || ctSpawns.childCount == 0)
                _validationMessages.Add($"⚠ CT 重生点 '{info.teamASpawnRoot}' 缺失或为空");
            else
                _validationMessages.Add($"✓ CT 重生点: {ctSpawns.childCount} 个");

            // 检查 T 重生点
            Transform tSpawns = gameplay.Find(info.teamBSpawnRoot);
            if (tSpawns == null || tSpawns.childCount == 0)
                _validationMessages.Add($"⚠ T 重生点 '{info.teamBSpawnRoot}' 缺失或为空");
            else
                _validationMessages.Add($"✓ T 重生点: {tSpawns.childCount} 个");

            // 检查 FFA 重生点（仅死斗模式需要）
            if (info.supportedModes.Contains("free_for_all"))
            {
                Transform ffaSpawns = gameplay.Find(info.freeForAllSpawnRoot);
                if (ffaSpawns == null || ffaSpawns.childCount == 0)
                    _validationMessages.Add($"⚠ FFA 重生点 '{info.freeForAllSpawnRoot}' 缺失或为空（个人死斗模式需要）");
                else
                    _validationMessages.Add($"✓ FFA 重生点: {ffaSpawns.childCount} 个");
            }

            // 检查包点（仅爆破模式需要）
            if (info.supportedModes.Contains("demolition") && info.bombsiteNames != null)
            {
                foreach (string bombsite in info.bombsiteNames)
                {
                    Transform site = gameplay.Find($"包点/{bombsite}");
                    if (site == null)
                        _validationMessages.Add($"⚠ 包点 '{bombsite}' 未找到");
                    else
                    {
                        var col = site.GetComponent<BoxCollider>();
                        if (col == null || !col.isTrigger)
                            _validationMessages.Add($"⚠ 包点 '{bombsite}' 缺少 BoxCollider(Trigger)");
                        else
                            _validationMessages.Add($"✓ 包点 {bombsite} 正常");
                    }
                }
            }

            // 检查空气墙组
            Transform barriers = gameplay.Find(info.barrierRoot);
            if (barriers == null)
                _validationMessages.Add($"⚠ 空气墙组 '{info.barrierRoot}' 未找到");
            else
                _validationMessages.Add($"✓ 空气墙组: {barriers.childCount} 面墙");

            // 检查武器墙
            Transform weaponShops = gameplay.Find(info.weaponShopRoot);
            if (weaponShops == null)
                _validationMessages.Add($"⚠ 武器墙 '{info.weaponShopRoot}' 未找到");
            else
                _validationMessages.Add($"✓ 武器墙位置 正常");
        }

        private void ExportMap()
        {
            if (_mapPrefab == null || _mapInfoAsset == null || string.IsNullOrEmpty(_outputDirectory))
            {
                EditorUtility.DisplayDialog("导出失败", "请填写所有必要字段。", "确定");
                return;
            }

            MapInfo info;
            try
            {
                info = JsonUtility.FromJson<MapInfo>(_mapInfoAsset.text);
            }
            catch
            {
                EditorUtility.DisplayDialog("导出失败", "MapInfo JSON 无法解析。", "确定");
                return;
            }

            // 设置 AssetBundle 名称
            string bundleName = $"{info.mapId}.map";
            string prefabPath = AssetDatabase.GetAssetPath(_mapPrefab);
            string infoPath = AssetDatabase.GetAssetPath(_mapInfoAsset);

            if (string.IsNullOrEmpty(prefabPath) || string.IsNullOrEmpty(infoPath))
            {
                EditorUtility.DisplayDialog("导出失败", "预制体或 MapInfo 必须是 Project 中的资产。", "确定");
                return;
            }

            AssetImporter prefabImporter = AssetImporter.GetAtPath(prefabPath);
            prefabImporter.assetBundleName = bundleName;

            // MapInfo TextAsset 必须命名为 "MapInfo"
            AssetImporter infoImporter = AssetImporter.GetAtPath(infoPath);
            infoImporter.assetBundleName = bundleName;

            // 构建
            if (!Directory.Exists(_outputDirectory))
                Directory.CreateDirectory(_outputDirectory);

            AssetBundleBuild[] builds = new[]
            {
                new AssetBundleBuild
                {
                    assetBundleName = bundleName,
                    assetNames = new[] { prefabPath, infoPath }
                }
            };

            BuildPipeline.BuildAssetBundles(_outputDirectory, builds,
                BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);

            string outputFile = Path.Combine(_outputDirectory, bundleName);
            if (File.Exists(outputFile))
            {
                EditorUtility.DisplayDialog("导出成功",
                    $"地图已导出至:\n{outputFile}\n\n" +
                    $"将此文件复制到游戏的 BepInEx/plugins/ 目录即可使用。",
                    "好的");
                EditorUtility.RevealInFinder(outputFile);
            }
            else
            {
                EditorUtility.DisplayDialog("导出失败",
                    "AssetBundle 构建未生成预期文件。请检查 Console 日志。", "确定");
            }

            // 清理 bundle 名
            prefabImporter.assetBundleName = "";
            infoImporter.assetBundleName = "";
        }
    }
}
#endif
