# PvP Shot Mode — Map SDK

Unity 编辑器工具包，用于为 **PvP Shot Mode** 模组（How to Fish 游戏）创建自定义 PvP 地图。

## 安装方式

在 Unity 中打开 **Window → Package Manager → Add package from git URL**，输入：

```
https://github.com/<your-username>/PvPShotMode-MapSDK.git
```

或者克隆仓库到本地后，选择 **Add package from disk**，定位到 `package.json`。

## 快速上手

### 1. 创建地图模版

菜单: **PvPShotMode → 创建新地图模版**

在弹出的编辑器窗口中填写：
- **地图 ID**（英文小写+下划线，如 `my_arena`）
- **显示名称**（中文名）
- **作者**
- **支持的游戏模式**（勾选）
- **各阵营重生点数量**

点击 **★ 一键生成地图模版 ★** 后，场景中将自动创建完整层级：

```
MapRoot_my_arena
├── GamePlay
│   ├── TeamCT_respawn_points
│   │   ├── Spawn_CT_01 🔵  ← CT 重生点（蓝色半透明球）
│   │   ├── Spawn_CT_02 🔵
│   │   └── ...
│   ├── TeamT_respawn_points
│   │   ├── Spawn_T_01 🔴  ← T 重生点（红色半透明球）
│   │   └── ...
│   ├── FFA_respawn_points
│   │   ├── Spawn_FFA_01 🟢  ← FFA 重生点（绿色半透明球）
│   │   └── ...
│   ├── 回合开始空气墙组
│   │   └── Barrier_01 ⬜  ← 准备阶段屏障（白色半透明方块）
│   ├── 武器墙位置
│   │   ├── 警 🔵  ← CT 武器购买墙（蓝色半透明方块）
│   │   └── 匪 🔴  ← T 武器购买墙（红色半透明方块）
│   └── 包点
│       ├── A 🟡  ← 爆破包点区域（黄色球+BoxCollider）
│       └── B 🟡
└── MapInfo (TextAsset, JSON)
```

### 2. 编辑地图

- 在 `MapRoot` 下添加你的场景模型（地形、建筑、掩体等）
- 拖拽重生点到合适位置（Gizmo 球体自动显示）
- 调整包点大小（修改 BoxCollider.size）
- 添加/删除重生点和空气墙

### 3. 标记颜色含义

| 颜色 | 形状 | 含义 |
|------|------|------|
| 🔵 蓝色半透明球 | 球体 | CT (警察) 重生点 |
| 🔴 红色半透明球 | 球体 | T (匪徒) 重生点 |
| 🟢 绿色半透明球 | 球体 | FFA (个人死斗) 重生点 |
| 🟡 黄色半透明球 | 球体+BoxCollider | 爆破包点区域 |
| ⬜ 白色半透明方块 | 立方体 | 空气墙（准备阶段屏障） |
| 🔵/🔴 半透明方块 | 立方体 | 武器购买墙锚点 |

> 所有标记在**编辑器中可见**（Gizmo 半透明球/方块），**游戏中自动隐藏**。

### 4. 验证和导出

菜单: **PvPShotMode → 导出地图 (.map)**

1. 将 `MapRoot` 保存为 Prefab
2. 在导出窗口中指定 Prefab 和 MapInfo TextAsset
3. 点击 **验证地图结构** 检查完整性
4. 点击 **★ 一键导出 .map ★**
5. 将生成的 `.map` 文件复制到游戏的 `BepInEx/plugins/` 目录

### 5. MapInfo JSON 结构

```json
{
    "mapId": "my_arena",
    "displayName": "我的竞技场",
    "author": "MapMaker",
    "version": "1.0.0",
    "description": "一个小型竞技场地图",
    "supportedModes": ["demolition", "team_deathmatch", "free_for_all"],
    "previewImageName": "",
    "teamASpawnRoot": "TeamCT_respawn_points",
    "teamBSpawnRoot": "TeamT_respawn_points",
    "freeForAllSpawnRoot": "FFA_respawn_points",
    "bombsiteNames": ["A", "B"],
    "barrierRoot": "回合开始空气墙组",
    "weaponShopRoot": "武器墙位置",
    "weaponShopTeamAChild": "警",
    "weaponShopTeamBChild": "匪"
}
```

## 三种游戏模式

| 模式 | ID | 说明 |
|------|-----|------|
| **爆破模式** | `demolition` | CT 防守包点，T 安放 C4。回合制，先到目标分。需要包点。 |
| **团队竞技** | `team_deathmatch` | 双方对抗，消灭对方阵营所有玩家=回合胜利。无包点。 |
| **个人死斗** | `free_for_all` | 无阵营，击杀达到目标数获胜。自动重生。需要 FFA 重生点。 |

## 许可证

MIT License
