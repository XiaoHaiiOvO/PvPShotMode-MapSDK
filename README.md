# PvPShotMode Map SDK

《How to Fish》PvPShotMode 的 Unity 地图制作工具，版本 **2.4.0**。

本仓库只包含地图数据规范、编辑器模板、Gizmos 和导出工具，按 MIT 协议开源。玩法 DLL、游戏源码、模型、音效和第三方资源不在本仓库中。

## 2.3.0 生化模式与武器墙预览

新增 `infection`，对应 PvPShotMode 2.3.0 或更新兼容版本。此版本同时包含 2.2.1 的固定武器墙布局预览，以及 2.2.2 的 URP 灯光组件兼容修复。

```text
地图根
├─ Map
└─ GamePlay
   ├─ Human_respawn_points   # 5～16，模板默认16
   ├─ Zombie_respawn_points  # 1～15，模板默认10
   ├─ Airdrop_points         # 1～10，模板默认5
   └─ 武器墙位置
      └─ 人类               # 唯一武器墙，本地+Z正面
```

创建模板时勾选生化模式。该模式不要求 CT/T 出生点、包点或空气墙；混合模式地图仍需满足其余所选模式的要求。新增 MapInfo 字段 `humanSpawnRoot`、`zombieSpawnRoot`、`airdropRoot`、`humanWeaponShopChild` 均相对 GamePlay（武器墙子名相对武器墙根）。

出生标记放在脚底地面附近；运行时校验地面与胶囊空间，并在作者点位附近为最多32人补位，无法安全分配则拒绝开战。点位数不是最大玩家数。

空投点需要约2.6米宽的下降通道，降落伞最高约3.2米；房主会检查屋顶并缩短下降高度，少于2米或通道占用则跳过。Gizmos 仅为设计提示，发布前须检查地图实际碰撞。

生化 `人类` 武器墙预览7种普通武器及配件，不含投掷物。共享丧尸、音频、防护服等放在插件的 `infection_assets`，不重复塞入每张地图。

## 导入 Unity

使用 **Unity 6000.4.4f1**，与当前验证的游戏版本一致。地图目标平台为 **Windows 64 位**。

1. 打开 Unity 的 `Window → Package Management → Package Manager`。
2. 点击 `+ → Install package from git URL`（部分布局显示 `Add package from git URL`）。
3. 输入：

```text
https://github.com/XiaoHaiiOvO/PvPShotMode-MapSDK.git#v2.4.0
```

也可以在工程的 `Packages/manifest.json` 中添加：

```json
"com.htf.pvpshotmode-mapsdk": "https://github.com/XiaoHaiiOvO/PvPShotMode-MapSDK.git#v2.4.0"
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

## 编辑器跑图测试 player

新模板会在地图根下创建 `player`：Unity 原生 1:1:1 胶囊体、`CapsuleCollider`、质量 80 的 `Rigidbody`、第一人称相机和 `SimpleFPSController`。其体积接近游戏玩家，可在 Play Mode 中检查地图比例、通道宽度、台阶、场景碰撞体和边界阻挡。

- `W/A/S/D` 移动，`Shift` 加速，`Space` 跳跃，鼠标观察。
- `Esc` 释放或重新锁定鼠标。
- 将 `player` 移到待测试位置后进入 Play Mode；如果场景另有相机，请先禁用它。
- 可以移动或重命名该对象；导出器通过 `SimpleFPSController` 组件识别并删除整个测试对象。
- `.map` 只清理导出克隆，原始 Prefab 中的 `player` 会保留，便于继续测试。

## 编辑器标记图例

Scene 视图请开启 Gizmos。出生点使用半透明球体，武器墙使用真实槽位尺寸的方框阵列，箭头表示朝向。

| 标记 | 含义 | 游戏中的行为 |
| --- | --- | --- |
| 蓝球 | CT 出生点 | 玩家出生位置 |
| 红球 | T 出生点 | 玩家出生位置 |
| 紫球 | 个人死斗出生点 | 开局分配；死亡后优先选择远离其他存活玩家的位置 |
| 青球/胶囊框 | 生化人类出生点 | 开局人类出生位置；可在附近安全补位 |
| 绿球/胶囊框 | 生化丧尸出生点 | 感染和复活位置；可在附近安全补位 |
| 黄色线框柱 | 生化空投点 | 标示降落伞尺寸与约 15 米下降通道 |
| 黄色半透明方框 | 爆破包点 | 保留触发碰撞体，区域内可安放 C4 |
| 青色半透明方框 | 准备期空气墙 | 保留实体 BoxCollider，准备结束后关闭 |
| 绿色/青色槽位阵列 | 武器墙锚点 | 绿色武器/投掷物板、青色配件板；本地 +Z 为展示正面 |

这些标记只使用编辑器 Gizmos，没有运行时渲染器。导出克隆会移除标记脚本，保留节点与必要碰撞体；原始 Prefab 的标记仍然可编辑。

## 武器墙：按预览摆放，不再自动朝向出生点

需要配合 **PvPShotMode 2.2.1 或更新兼容版本的 DLL**。只更新 SDK、继续使用旧 DLL，不会改变游戏中的朝向算法。

1. 打开地图 Prefab，选择 `GamePlay/武器墙位置/警` 或 `匪`（自定义名称以地图定义为准）。旧节点没有脚本时，添加 `WeaponShopMarker` 组件；不需要球体、Mesh 或 Collider。
2. 开启 Scene 的 **Gizmos**，将移动/旋转工具切到 **Local**。蓝色 **+Z 箭头指向玩家应站的一侧**，绿色 **+Y 是上方**，红色 **+X 是排布方向**。
3. 移动、旋转该节点，使槽位放在墙前。标记原点就是布局原点，不是地面落点，也不是 Collider 中心；建议初始高度距地面约 1.65 米。新模板已使用该高度，并设置固定的 Y=180° 示例方向。
4. 保持标记及 Gameplay 上级层级的 Scale 为 `(1,1,1)`；改变 Scale 不会放大布局。非均匀缩放叠加旋转可能让运行时展示板变形，Inspector 会提示。
5. 绿色槽位固定 10 个：第一行手枪/霰弹枪/冲锋枪/狙击枪/步枪，第二行指虎/小刀/手雷/烟雾弹/闪光弹。未安装或未解锁的物品可能不显示，预览为完整布局。
6. 青色配件槽默认预览 7 个；`Preview Attachment Count` 仅影响编辑器显示，游戏按原版实际配件数量居中排列，不通过 SDK 创建配件。
7. 保存 Prefab，再使用 SDK 导出 `.map`。房主和客户端都更新 DLL 与地图文件。

**固定布局（单位：米）**：武器板 `0.72 × 0.46 × 0.14`，5 列 × 2 行，横向间距 1.05，纵向间距 0.75；配件板 `0.54 × 0.34 × 0.12`，间距 0.72。10 个武器槽和 7 个配件槽的购买板总包围框为 **4.92 × 1.90 × 0.14**；标题另在原点上方 1.47 米。边框精确对应购买板/碰撞体，不包含物品模型向前突出部分及可变长度文字，SDK 不包含原生武器模型。

SDK 与玩法代码共用 [WeaponShopLayout.cs](Runtime/WeaponShopLayout.cs)。运行时读取导出节点的世界 Position/Rotation，再叠加固定的本地槽位偏移；忽略出生点、相机、Collider.center 和标记 Scale，不再推算正反面、自动向前挪动或让文字追踪相机。自定义旋转（包括俯仰）会被保留。

**旧地图迁移注意**：不会自动转换以前依赖“面向出生点”摆放的节点。旧 `.map` 可以加载，但必须在新预览里检查摆放并重新导出，不能假定旧节点的旋转正确。

如果 SDK 已嵌入工程的 `Packages/com.htf.pvpshotmode-mapsdk`，直接使用该本地包，无需重复导入 Git 包。以后切换回 Git 版本前先备份本地 SDK 修改并通过 Package Manager 移除本地包；不要同时保留两套相同脚本或直接修改 `Library/PackageCache`。

## 模式规则

| ID | 规则 | 必需标记 |
| --- | --- | --- |
| `demolition` | 安放 / 拆除 C4；全灭与未安放超时按爆破规则判定；先到房主设定分数获胜 | CT/T 出生点、空气墙组、两侧武器墙、至少一个包点 |
| `team_deathmatch` | 单局按房主设置的时间或团队击杀目标决胜；限时平分进入双方随机一人单挑；死亡后按配置重生 | CT/T 出生点、空气墙组、两侧武器墙 |
| `free_for_all` | 每人互为敌人；击杀数先达到目标获胜；死亡后自动重生 | FFA 出生点 |
| `infection` | 开局随机母体感染其他玩家；人类坚持到时限或消灭母体获胜，丧尸感染全部人类获胜 | 5～16 个人类点、1～15 个丧尸点、1～10 个空投点、一个人类武器墙 |

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
- 导出会拒绝丢失脚本和未列入兼容白名单的 MonoBehaviour，并列出对象路径。SDK 标记会从导出克隆移除；已验证游戏提供的 `Unity.RenderPipelines.Universal.Runtime` 程序集中的 `UniversalAdditionalLightData` 会保留（必须与 Light 位于同一对象）。不会按 UnityEngine 命名空间或整个程序集一律放行。
- 导出前和 AB 回读使用相同的组件校验；禁用对象或禁用脚本不会绕过检查。当前白名单仅额外支持上述灯光数据，不代表自定义脚本、NavMeshSurface 或所有 URP 组件均已支持。
- Area 灯在当前 URP 中仅支持烘焙；需要随预制体工作的实时补光时使用 Point/Spot。当前 `.map` 导出器不负责场景 Lightmap 的导出与运行时绑定。
- 旧版 SDK 将多个组件放在同一个文件，可能留下 Missing Script。新版每个组件独立成文件。旧 Prefab 请重新挂载相应标记、检查碰撞体后再导出。
- 新模板中的 `SimpleFPSController` 测试 `player` 会在导出克隆中连同相机、Rigidbody、碰撞体和模型一起删除，不进入 `.map`。
- 导出不修改原始 Prefab 和 AssetImporter 的 Bundle 名称；生成后回读 AB，验证固定地址、JSON、必需对象及组件白名单。
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
