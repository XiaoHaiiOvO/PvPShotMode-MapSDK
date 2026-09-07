using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using HowToFish.PvPShotMode.Map;
namespace PvPShotMode.MapSDK.Editor
{
    public sealed class PvPMapExporter : EditorWindow
    {
        private PvPMapDefinition definition;
        [MenuItem("PvPShotMode/导出地图 (.map)")]
        public static void Open() => GetWindow<PvPMapExporter>("导出 PvP 地图");
        private void OnGUI()
        {
            definition = (PvPMapDefinition)EditorGUILayout.ObjectField("地图定义", definition, typeof(PvPMapDefinition), false);
            EditorGUILayout.HelpBox("编辑地图定义里的名称、简介、支持模式和规则。保存 Prefab 后导出。所有编辑器标记会被移除，原始 Prefab 保持可编辑。目标平台 Windows 64 位，Unity 版本请与游戏一致。", MessageType.Info);
            using (new EditorGUI.DisabledScope(definition == null))
            if (GUILayout.Button("校验并导出 .map"))
            {
                string folder = EditorUtility.OpenFolderPanel("地图输出目录", "", "");
                if (string.IsNullOrEmpty(folder)) return;
                try { string file = Export(definition, folder); EditorUtility.DisplayDialog("导出成功", file, "确定"); }
                catch (Exception ex) { Debug.LogException(ex); EditorUtility.DisplayDialog("导出失败", ex.Message, "确定"); }
            }
        }
        public static string Export(PvPMapDefinition definition, string outputDirectory)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            var info = MapInfo.CreateFromJson(JsonUtility.ToJson(definition.info));
            var errors = MapValidation.Validate(definition.prefab, info);
            if (System.Text.RegularExpressions.Regex.IsMatch(info.mapId ?? "", "[^a-z0-9_-]")) errors.Add("mapId 只允许小写英文、数字、下划线和连字符");
            if (errors.Count > 0) throw new InvalidOperationException(string.Join("\n", errors));
            if (!Application.unityVersion.StartsWith("6000.4."))
                Debug.LogWarning("当前支持的游戏使用 Unity 6000.4.4f1，使用其他 Unity 版本生成的 AB 可能不兼容。");
            string temp = "Assets/__PvPMapExport_" + Guid.NewGuid().ToString("N");
            string staging = Path.Combine(Path.GetTempPath(), "PvPMapBuild_" + Guid.NewGuid().ToString("N"));
            GameObject clone = null;
            try
            {
                Directory.CreateDirectory(temp); Directory.CreateDirectory(staging);
                clone = Instantiate(definition.prefab); clone.name = definition.prefab.name;
                // Missing scripts are rejected: silently removing them could delete gameplay or collision setup.
                foreach (var t in clone.GetComponentsInChildren<Transform>(true))
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) > 0)
                        throw new InvalidOperationException("存在丢失脚本：" + t.name + "。请修复后导出。");
                foreach (var marker in clone.GetComponentsInChildren<PvPMapMarker>(true)) DestroyImmediate(marker);
                foreach (var mb in clone.GetComponentsInChildren<MonoBehaviour>(true))
                    throw new InvalidOperationException("地图包含游戏未提供的脚本：" + mb.GetType().FullName + "。请使用原生组件或烘焙为静态资源。");
                info.prefabAssetName = "MapPrefab";
                string prefabPath = temp + "/MapPrefab.prefab", jsonPath = temp + "/MapInfo.json";
                PrefabUtility.SaveAsPrefabAsset(clone, prefabPath);
                File.WriteAllText(jsonPath, JsonUtility.ToJson(info, true));
                AssetDatabase.ImportAsset(jsonPath, ImportAssetOptions.ForceSynchronousImport);
                string fileName = info.mapId + ".map";
                var build = new AssetBundleBuild { assetBundleName = fileName,
                    assetNames = new[] { prefabPath, jsonPath }, addressableNames = new[] { "MapPrefab", "MapInfo" } };
                var manifest = BuildPipeline.BuildAssetBundles(staging, new[] { build },
                    BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.StrictMode, BuildTarget.StandaloneWindows64);
                if (manifest == null || !File.Exists(Path.Combine(staging, fileName))) throw new InvalidOperationException("Unity 未生成地图资源");
                var bundle = AssetBundle.LoadFromFile(Path.Combine(staging, fileName));
                if (bundle == null) throw new InvalidOperationException("导出的地图无法回读");
                try
                {
                    var loadedInfo = bundle.LoadAsset<TextAsset>("MapInfo");
                    var loadedPrefab = bundle.LoadAsset<GameObject>("MapPrefab");
                    if (loadedInfo == null || loadedPrefab == null) throw new InvalidOperationException("地图缺少固定地址 MapInfo/MapPrefab");
                    var roundTripErrors = MapValidation.Validate(loadedPrefab, MapInfo.CreateFromJson(loadedInfo.text));
                    if (roundTripErrors.Count > 0) throw new InvalidOperationException(string.Join("\n", roundTripErrors));
                    if (loadedPrefab.GetComponentsInChildren<MonoBehaviour>(true).Length != 0) throw new InvalidOperationException("导出资源残留脚本");
                }
                finally { bundle.Unload(true); }
                Directory.CreateDirectory(outputDirectory);
                string destination = Path.GetFullPath(Path.Combine(outputDirectory, fileName));
                File.Copy(Path.Combine(staging, fileName), destination, true);
                Debug.Log("[PvPShotMode SDK] 导出并回读验证成功：" + destination);
                return destination;
            }
            finally
            {
                if (clone != null) DestroyImmediate(clone);
                AssetDatabase.DeleteAsset(temp);
                if (Directory.Exists(staging)) Directory.Delete(staging, true);
            }
        }
    }
}
