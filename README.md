# PvPShotMode Map SDK

《How to Fish》PvPShotMode 的 Unity 地图制作工具，版本 **2.1.0**。

本仓库只包含地图数据规范、编辑器模板、Gizmos 和导出工具，按 MIT 协议开源。玩法 DLL、游戏源码、模型、音效和第三方资源不在本仓库中。

## 导入 Unity

使用 **Unity 6000.4.4f1**，与当前验证的游戏版本一致。地图目标平台为 **Windows 64 位**。

1. 打开 Unity 的 `Window → Package Management → Package Manager`。
2. 点击 `+ → Install package from git URL`（部分布局显示 `Add package from git URL`）。
3. 输入：

```text
https://github.com/XiaoHaiiOvO/PvPShotMode-MapSDK.git#v2.1.0
```

也可以在工程的 `Packages/manifest.json` 中添加：

```json
"com.htf.pvpshotmode-mapsdk": "https://github.com/XiaoHaiiOvO/PvPShotMode-MapSDK.git#v2.1.0"
```

本机需要安装 Git；制作地图不需要 GitHub 账号，也不需要导入玩法 DLL。

## 制作第一张地图

1. 菜单 `PvPShotMode → 新建地图模板`，填写唯一 ID、名称和支持的模式。
2. 点击“一键生成并保存模板”，选择 `Assets` 内的目录。工具生成 Prefab 和 `*_Definition.asset` 地图定义。
3. 双击 Prefab 进入编辑模式。**模板没有场景模型或可行走地面**；添加自己的地面、碰撞体、掩体和模型。
4. 调整出生点、包点、空气墙和武器墙位置。保存 Prefab。
5. 选择地图定义，在 Inspector 填写简介、作者、版本、支持模式以及个人死斗规则。`prefab` 指向刚才保存的 Prefab。
6. 菜单 `PvPShotMode → 导出地图 (.map)`，选择地图定义，点击“校验并导出”。
7. 把生成的 `地图ID.map` 放在游戏的 `BepInEx/plugins` 下任意子目录，重启游戏。所有玩家安装**完全相同**的文件。
8. 房主按 **P**，选择地图和模式，前往传送岛。团队模式进入双方传送门选队；个人死斗进入任一传送门确认参赛，至少两人。全部确认并加载完成后开始倒计时。

## 编辑器标记图例

Scene 视图请开启 Gizmos。标记使用半透明颜色，球上方有名称，箭头表示朝向。

| 标记 | 含义 | 游戏中的行为 |
| --- | --- | --- |
| 蓝球 | CT 出生点 | 玩家出生位置 |
| 红球 | T 出生点 | 玩家出生位置 |
| 紫球 | 个人死斗出生点 | 开局分配；死亡后优先选择远离其他存活玩家的位置 |
| 黄色半透明方框 | 爆破包点 | 保留触发碰撞体，区域内可安放 C4 |
| 青色半透明方框 | 准备期空气墙 | 保留实体 BoxCollider，准备结束后关闭 |
| 绿球 | 武器墙锚点 | 团队模式的武器与配件商店位置 |

这些标记只使用编辑器 Gizmos，没有运行时渲染器。导出克隆会移除标记脚本，保留节点与必要碰撞体；原始 Prefab 的标记仍然可编辑。

## 模式规则

| ID | 规则 | 必需标记 |
| --- | --- | --- |
| `demolition` | 安放 / 拆除 C4；全灭与未安放超时按爆破规则判定；先到房主设定分数获胜 | CT/T 出生点、空气墙组、两侧武器墙、至少一个包点 |
| `team_deathmatch` | 消灭对方所有玩家赢得回合；超时或双方同时全灭为平局；先到房主设定分数获胜 | CT/T 出生点、空气墙组、两侧武器墙 |
| `free_for_all` | 每人互为敌人；击杀数先达到目标获胜；死亡后自动重生 | FFA 出生点 |

个人死斗参数：`targetKills` 默认 30，`respawnSeconds` 默认 3，`spawnProtectionSeconds` 默认 2，`freeForAllWeaponId` 默认 66。保护期间不能造成或受到玩家伤害。武器 ID 必须是当前游戏可生成的枪械。

一张地图可支持多种模式，必须具备这些模式要求的对象。模式切换仅能在会话启动前或传送岛等待阶段进行，切换会清空参赛确认并重新同步。

## 地图资源规范

`.map` 是 Unity AssetBundle，固定资源地址为：

| 地址 | 类型 | 内容 |
| --- | --- | --- |
| `MapInfo` | TextAsset | UTF-8 JSON 元信息，`schemaVersion = 1` |
| `MapPrefab` | GameObject | 地图 Prefab |

字段定义见 [MapInfo.cs](Runtime/MapInfo.cs)，校验实现见 [MapValidation.cs](Runtime/MapValidation.cs)。旧 SDK 的 `*_MapInfo` TextAsset 仍可由 2.1.0 玩法兼容读取，但建议重新导出。

节点名称可以修改。`gameplayRoot` 指定 Gameplay 根名称；其他节点路径相对于该根。包点名称数组相对于 `bombsiteRoot`，顺序就是联机包点编号，支持 1–254 个包点。出生点组必须包含子对象，其世界坐标用作出生位置。

每个正式 `.map` 包含一个地图定义与对应 Prefab。`mapId` 使用小写英文、数字、下划线或连字符，不能与其他地图重复。房主同步地图版本、内容校验和所选模式；内容不一致会被拒绝 Ready。

## 导出约束与排错

- 使用游戏兼容的 Unity 版本和 Windows 64 位平台。修改扩展名不能把普通文件变成 AB。
- 模型必须有可行走的碰撞体；空模板不能直接当作可玩地图。
- 导出会拒绝丢失脚本及非 SDK 的 MonoBehaviour。将自定义脚本移除或烘焙为原生组件；玩法逻辑由 DLL 提供。
- 旧版 SDK 将多个组件放在同一个文件，可能留下 Missing Script。新版每个组件独立成文件。旧 Prefab 请重新挂载相应标记、检查碰撞体后再导出。
- 导出不修改原始 Prefab 和 AssetImporter 的 Bundle 名称；生成后回读 AB，验证固定地址、JSON、必需对象和无残留脚本。
- P 菜单没有地图时，查看 BepInEx 日志中的 `[PvP][Maps]`，检查重复 ID、缺少出生点、Unity 版本和 JSON。
- `previewImageName` 为保留字段，当前菜单显示文字信息。

## SDK 开发

编辑器 API：

```csharp
var definition = PvPShotMode.MapSDK.Editor.PvPMapCreatorWindow.CreateTemplate(
    "Assets/MyMaps", new HowToFish.PvPShotMode.Map.MapInfo { mapId = "arena_01" });
string output = PvPShotMode.MapSDK.Editor.PvPMapExporter.Export(definition, "D:/ExportedMaps");
```

不要将玩法核心或游戏程序集提交到本仓库。变更元信息时维护协议版本，并同步更新游戏侧对公开数据源的引用。
