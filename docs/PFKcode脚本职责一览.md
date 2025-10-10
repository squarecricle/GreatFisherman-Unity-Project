# PFKcode 脚本职责一览

> 说明：本表围绕 `Assets/PFKcode` 目录，按 **Manager 管理者** / **Controller 控制器** 分类。Manager 负责全局状态或系统调度；Controller 专注具体对象或 UI 面板的即时行为。列出了主要职责、关键引用与补充说明，便于梳理依赖关系和后续重构。

| 分类 | 脚本 | 核心职责 | 关键 Inspector 引用 | 当前状态 | 下一步 TODO | 备注 |
| ---- | ---- | -------- | -------------------- | ---------- | ------------- | ---- |
| Manager | `EquipmentManager` | 未来负责装备数据的集中管理与升级逻辑（当前为空壳，预留入口）。 | 无 | 未实现 | 设计数据结构，接入鱼竿/鱼箱升级消耗。 | 计划接入鱼竿/鱼箱等可升级资源。 |
| Manager | `FishingMiniGameManager` | 统筹钓鱼博弈 UI、状态机、鱼行为驱动，并触发战利品、宝箱、背包结算。 | `GameFlowManager`、`TreasureChestController`、`InventoryManager`、`MiniGamePanel`、`ProgressBar`、`PlayerBar`、`PlayerBarController`、`FishController`、`ChestLootText` 等 | 核心流程可跑通 | 拆分状态机、事件化抛竿→小游戏衔接。 | 通过 `CatchResult` 结构向背包汇报结果；在 `Update()` 中驱动玩家条与进度条。 |
| Manager | `GameFlowManager` | 切换主菜单与钓鱼场景 UI、分发钓点数据、触发抛竿流程；后续承载场景跳转（鱼箱/商店等）。 | `MainMenuPanel`、`FishingUIPanel`、`SpotTitleText`、`CastingAndHookingController`、`FishingMiniGameManager`、各 `FishingSpot` | 主流程可用 | 引入状态枚举并整合 UI 显隐逻辑。 | 准则中保持 Manager 角色，集中管理高层流程与 UI 面板显隐。 |
| Manager | `InventoryManager` | （TODO）存储渔获/资源，负责背包增删、分类查询、出售结算。 | 无 | 未实现 | 完成 `AddItem` / `SellAllItems`，并提供查询 API。 | 目前仅输出调试日志，后续需实现 `AddItem` / `SellAllItems`。 |
| Manager | `PlayerWalletManager` | 管理玩家金币，提供增减方法并负责与 UI 同步。 | 无 | 雏形 | 实现金币加减与 UI 刷新对接。 | 日志已更新为 `PlayerWalletManager` 前缀。 |
| Manager | `ShopManager` | （TODO）承载商店库存、价格、结算逻辑。 | 无 | 未实现 | 建立商品表与购买校验流程。 | 与 UI 控制脚本分离，后续管理商品刷新与资源校验。 |
| Data | `CatchableData` + 派生 (`FishData`, `TrashData`, `TreasureChestData`) | ScriptableObject 基类：定义可钓物通用属性、行为序列、移动参数与分类；派生类扩展鱼类稀有度/长度、垃圾稀有度、宝箱掉落表。 | 行为序列 (`CalmBehaviorSequence`, `StruggleBehaviorSequence`)、初始位置、速度等 | 已投入使用 | 增加数据校验与编辑器预览。 | `[SerializeReference]` 支持多态 `FishAction`，新行为需继承 `FishAction` 并实现 `Execute()`。 |
| Data | `InventoryItem` / `FishInventoryItem` | 背包存档用实体对象；封装数量、价格计算，鱼类包含品质与长度。 | 无（序列化） | 已投入使用 | 等待 InventoryManager 调用并补充序列化策略。 | `FishInventoryItem` 覆写价格以品质倍率结算。 |
| Controller | `CastingAndHookingController` | 控制抛竿蓄力、甜蜜点动画、等待咬钩与提线反应；与 MiniGameManager 接力。 | `CastingAndHookingPanel`、`HookIcon`、`FishingMiniGameManager`、`FishingSpot`、`SweetSpotRect`、`ParabolicPowerBarController` | 核心流程可跑通 | 抽离状态逻辑与 UI 演出配置。 | 自建状态机管理流程；完美抛竿影响 `Hooking` 时限。 |
| Controller | `BackpackUIController` | （TODO）负责背包界面单面板交互显示。 | 无 | 未实现 | 设计背包格子 UI 生成与事件响应。 | Manager/Controller 准则下聚焦 UI 组件。 |
| Controller | `FishTankUIController` | 处理鱼箱面板按钮（如“全部出售”）与刷新显示。 | 无 | 雏形 | 对接 InventoryManager、PlayerWalletManager 更新 UI。 | 需与 `InventoryManager`、`PlayerWalletManager` 对接。 |
| Controller | `MapUIController` | （TODO）负责地图选择界面的按钮显隐与交互。 | 无 | 未实现 | 完成按钮绑定与 GameFlowManager 状态切换。 | 替代旧 `MapUIManager`，专注具体 UI 控制。 |
| Controller | `ShopUIController` | （TODO）控制商店界面按钮、选项卡与面板切换。 | 无 | 未实现 | 实现选项卡切换，并触发 ShopManager 逻辑。 | 搭配 `ShopManager` 管理商品逻辑。 |
| Controller | `CommonUIController` | 提供通用导航按钮事件（如“鱼箱”按钮），后续补充主页/地图按钮显隐。 | 需要绑定 `GameFlowManager`、具体按钮（待设置） | 雏形 | 与 GameFlowManager 对齐导航流程，补充按钮引用。 | 按钮回调目前仅输出日志。 |
| Controller | `FishController` | 控制迷你游戏中鱼/可捕捉物的移动与行为执行；根据进度条状态切换行为序列。 | `FishingMiniGameManager`（运行时注入） | 核心流程可跑通 | 优化行为切换与性能监控（避免重复协程）。 | 负责边界计算、行为协程调度、移动速度调整。 |
| Controller | `ParabolicPowerBarController` | 控制抛竿力度条的蓄力曲线（基于 AnimationCurve）与可视化。 | `PowerBarSlider`、`SpeedProfileCurve` | 核心流程可跑通 | 增加 Editor 预览与曲线参数校验。 | `CastingAndHookingController` 驱动其开始/停止。 |
| Controller | `PlayerBarController` | 驱动玩家绿条位置，响应鼠标输入与重力。 | `RectTransform`（自身） | 核心流程可跑通 | 拆分输入与物理参数，便于平衡调节。 | 由 `FishingMiniGameManager` 每帧调用 `HandleUpdate()`。 |
| Controller | `TreasureChestController` | 基于掉落概率随机授予宝箱数据。 | `AvailableChests` | 核心流程可跑通 | 引入权重与保底策略，支持日志回放。 | 返回 `TreasureChestData` 给 MiniGameManager 触发奖励。 |
| Other | `FishBehaviorActions` | 定义 `FishAction` 抽象类及其具体行为（Move、Wait、Jump、ChangeSpeed、Jitter），供 ScriptableObject 行为序列配置。 | 依赖 `FishController` 运行时环境 | 核心流程可跑通 | 增强参数校验、加入更多行为组合。 | 行为以协程形式执行，可组合出不同鱼种 AI。 |
| Other | `FishingSpot` | 钓点配置：包含战利品权重池并负责启动抛竿流程。 | `FishingMiniGameManager`、`CastingAndHookingController`、`LootPool`、`StartFishingButton` | 核心流程可跑通 | 分离 loot 表 ScriptableObject，支持多个钓点共享。 | 选中物品后写入 `CurrentCatchableData` 并开启抛竿。 |

## 使用说明
1. 打开 `SampleScene` 后，检查场景物体上的脚本引用是否匹配表格中的“关键引用”。若存在 Missing Script，按照新命名重新指派。
2. 新增系统时遵循命名准则：
   - 全局/资源管理 -> `*Manager`
   - 单对象/单 UI 面板控制 -> `*Controller`
3. 在实现 TODO 时，保持日志/注释中 Manager/Controller 名称的一致性，便于搜索。

> 若需要进一步的系统依赖图或流程图，可在此文档基础上继续扩展。