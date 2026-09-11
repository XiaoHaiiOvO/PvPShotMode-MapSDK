using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HowToFish.PvPShotMode.Map
{
    public static class MapValidation
    {
        public static Transform FindGameplay(GameObject prefab, MapInfo info) => prefab == null ? null :
            prefab.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == info.gameplayRoot);

        // Called by the exporter and the game, so acceptance rules cannot drift.
        public static List<string> Validate(GameObject prefab, MapInfo info, string selectedMode = null)
        {
            var errors = new List<string>();
            if (info == null) { errors.Add("缺少 MapInfo"); return errors; }
            if (info.schemaVersion != 1) errors.Add("不支持的 schemaVersion：" + info.schemaVersion);
            if (string.IsNullOrWhiteSpace(info.mapId)) errors.Add("mapId 不能为空");
            if (string.IsNullOrWhiteSpace(info.displayName)) errors.Add("displayName 不能为空");
            if (string.IsNullOrWhiteSpace(info.version)) errors.Add("version 不能为空");
            if (info.supportedModes == null || info.supportedModes.Length == 0) errors.Add("至少选择一种模式");
            if (selectedMode != null && !info.SupportsMode(selectedMode)) errors.Add("地图不支持模式：" + selectedMode);
            var root = FindGameplay(prefab, info);
            if (root == null) { errors.Add("缺少 " + info.gameplayRoot); return errors; }
            bool teams = selectedMode == null
                ? info.SupportsMode(GameModeIds.Demolition) || info.SupportsMode(GameModeIds.TeamDeathmatch)
                : selectedMode == GameModeIds.Demolition || selectedMode == GameModeIds.TeamDeathmatch;
            bool bomb = selectedMode == GameModeIds.Demolition || selectedMode == null && info.SupportsMode(GameModeIds.Demolition);
            bool ffa = selectedMode == GameModeIds.FreeForAll || selectedMode == null && info.SupportsMode(GameModeIds.FreeForAll);
            bool infection = selectedMode == GameModeIds.Infection || selectedMode == null && info.SupportsMode(GameModeIds.Infection);
            if (infection)
            {
                RequireCount(root, info.humanSpawnRoot, 5, 16, errors);
                RequireCount(root, info.zombieSpawnRoot, 1, 15, errors);
                RequireCount(root, info.airdropRoot, 1, 10, errors);
                Require(root, info.weaponShopRoot + "/" + info.humanWeaponShopChild, errors);
            }
            if (teams)
            {
                RequireSpawns(root, info.teamASpawnRoot, errors);
                RequireSpawns(root, info.teamBSpawnRoot, errors);
                Require(root, info.barrierRoot, errors);
                Require(root, info.weaponShopRoot + "/" + info.weaponShopTeamAChild, errors);
                Require(root, info.weaponShopRoot + "/" + info.weaponShopTeamBChild, errors);
            }
            if (ffa)
            {
                RequireSpawns(root, info.freeForAllSpawnRoot, errors);
                if (info.targetKills < 1 || info.targetKills > 1000) errors.Add("targetKills 必须为 1~1000");
                if (!Finite(info.respawnSeconds) || info.respawnSeconds < .5f || info.respawnSeconds > 60) errors.Add("respawnSeconds 必须为 0.5~60");
                if (!Finite(info.spawnProtectionSeconds) || info.spawnProtectionSeconds < 0 || info.spawnProtectionSeconds > 10) errors.Add("spawnProtectionSeconds 必须为 0~10");
                if (info.freeForAllWeaponId < 0 || info.freeForAllWeaponId > 254) errors.Add("freeForAllWeaponId 必须为有效物品 ID");
            }
            if (bomb)
            {
                if (info.bombsiteNames == null || info.bombsiteNames.Length == 0 || info.bombsiteNames.Length > 254)
                    errors.Add("爆破模式必须配置 1~254 个包点");
                else foreach (var name in info.bombsiteNames)
                {
                    var site = root.Find(info.bombsiteRoot + "/" + name);
                    if (site == null || site.GetComponent<BoxCollider>() == null)
                        errors.Add("包点缺少 BoxCollider：" + name);
                }
            }
            return errors;
        }
        private static bool Finite(float f) => !float.IsNaN(f) && !float.IsInfinity(f);
        private static void RequireCount(Transform root, string path, int min, int max, List<string> errors)
        {
            var group = string.IsNullOrEmpty(path) ? null : root.Find(path);
            if (group == null || group.childCount < min || group.childCount > max)
                errors.Add("GamePlay/" + path + " 必须有 " + min + "～" + max + " 个直接子点位");
        }
        private static void Require(Transform root, string path, List<string> errors)
        { if (string.IsNullOrEmpty(path) || root.Find(path) == null) errors.Add("缺少 GamePlay/" + path); }
        private static void RequireSpawns(Transform root, string path, List<string> errors)
        {
            var t = string.IsNullOrEmpty(path) ? null : root.Find(path);
            if (t == null || t.childCount == 0) errors.Add("出生点组至少需要一个子对象：" + path);
        }
    }
}
