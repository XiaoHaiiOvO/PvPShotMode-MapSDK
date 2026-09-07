using System;
using UnityEngine;

namespace HowToFish.PvPShotMode.Map
{
    [Serializable]
    public sealed class MapInfo
    {
        public int schemaVersion = 1;
        public string mapId = "my_map";
        public string displayName = "我的地图";
        public string author = "";
        public string version = "1.0.0";
        public string description = "";
        public string[] supportedModes = { GameModeIds.Demolition, GameModeIds.TeamDeathmatch };
        public string prefabAssetName = "MapPrefab";
        public string previewImageName = "";
        public string gameplayRoot = "GamePlay";
        public string teamASpawnRoot = "TeamCT_respawn_points";
        public string teamBSpawnRoot = "TeamT_respawn_points";
        public string freeForAllSpawnRoot = "FFA_respawn_points";
        public string bombsiteRoot = "包点";
        public string[] bombsiteNames = { "A", "B" };
        public string barrierRoot = "回合开始空气墙组";
        public string weaponShopRoot = "武器墙位置";
        public string weaponShopTeamAChild = "警";
        public string weaponShopTeamBChild = "匪";
        public int targetKills = 30;
        public float respawnSeconds = 3f;
        public float spawnProtectionSeconds = 2f;
        // Native weapon ID; 66 is a verified firearm in the supported game version.
        public int freeForAllWeaponId = 66;

        public bool SupportsMode(string id) => supportedModes != null &&
            Array.Exists(supportedModes, x => string.Equals(x, id, StringComparison.OrdinalIgnoreCase));

        public static MapInfo CreateFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("MapInfo JSON 为空");
            var info = JsonUtility.FromJson<MapInfo>(json);
            if (info == null) throw new ArgumentException("MapInfo JSON 无效");
            info.NormalizeLegacyFields();
            return info;
        }

        public void NormalizeLegacyFields()
        {
            if (schemaVersion == 0) schemaVersion = 1;
            if (string.IsNullOrEmpty(gameplayRoot)) gameplayRoot = "GamePlay";
            if (string.IsNullOrEmpty(bombsiteRoot)) bombsiteRoot = "包点";
            if (string.IsNullOrEmpty(teamASpawnRoot)) teamASpawnRoot = "TeamCT_respawn_points";
            if (string.IsNullOrEmpty(teamBSpawnRoot)) teamBSpawnRoot = "TeamT_respawn_points";
            if (string.IsNullOrEmpty(freeForAllSpawnRoot)) freeForAllSpawnRoot = "FFA_respawn_points";
            if (string.IsNullOrEmpty(barrierRoot)) barrierRoot = "回合开始空气墙组";
            if (string.IsNullOrEmpty(weaponShopRoot)) weaponShopRoot = "武器墙位置";
            if (string.IsNullOrEmpty(weaponShopTeamAChild)) weaponShopTeamAChild = "警";
            if (string.IsNullOrEmpty(weaponShopTeamBChild)) weaponShopTeamBChild = "匪";
            if (targetKills <= 0) targetKills = 30;
            if (respawnSeconds <= 0) respawnSeconds = 3;
            if (freeForAllWeaponId <= 0) freeForAllWeaponId = 66;
        }

        public static MapInfo CreateLegacyDefault(string name) => new MapInfo {
            mapId = name, displayName = name, prefabAssetName = name,
            description = "兼容旧地图：建议使用新版 SDK 补充元信息后重新导出。"
        };
    }

}
