using System;
using UnityEngine;

namespace PvPShotMode.MapSDK
{
    /// <summary>
    /// 地图元信息数据类 — 序列化为 JSON，打包到 .map AssetBundle 中名为 "MapInfo" 的 TextAsset。
    /// 与 PvPShotMode.dll 运行时的 MapInfo 结构保持一致。
    /// </summary>
    [Serializable]
    public class MapInfo
    {
        [Tooltip("地图唯一标识 (英文小写+下划线，如 dust2)")]
        public string mapId = "my_map";

        [Tooltip("地图显示名称")]
        public string displayName = "My Custom Map";

        [Tooltip("作者名")]
        public string author = "Unknown";

        [Tooltip("版本号 (如 1.0.0)")]
        public string version = "1.0.0";

        [Tooltip("地图简介")]
        [TextArea(2, 5)]
        public string description = "自定义 PvP 地图";

        [Tooltip("支持的游戏模式 ID 列表\ndemolition = 爆破模式\nteam_deathmatch = 团队竞技\nfree_for_all = 个人死斗")]
        public string[] supportedModes = { "demolition", "team_deathmatch" };

        [Tooltip("预览图名称 (Bundle 内的 Sprite 资产名)")]
        public string previewImageName = "";

        [Header("节点名称约定")]
        [Tooltip("CT(警)重生点父节点名")]
        public string teamASpawnRoot = "TeamCT_respawn_points";

        [Tooltip("T(匪)重生点父节点名")]
        public string teamBSpawnRoot = "TeamT_respawn_points";

        [Tooltip("FFA 重生点父节点名 (个人死斗模式)")]
        public string freeForAllSpawnRoot = "FFA_respawn_points";

        [Tooltip("爆破包点名列表 (爆破模式必须，如 A, B)")]
        public string[] bombsiteNames = { "A", "B" };

        [Tooltip("空气墙组父节点名")]
        public string barrierRoot = "回合开始空气墙组";

        [Tooltip("武器购买墙父节点名")]
        public string weaponShopRoot = "武器墙位置";

        [Tooltip("武器墙 CT 子节点名")]
        public string weaponShopTeamAChild = "警";

        [Tooltip("武器墙 T 子节点名")]
        public string weaponShopTeamBChild = "匪";
    }
}
