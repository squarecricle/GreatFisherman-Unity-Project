## AI Coding Agent Instructions — GreatFisherman

本指南帮助 AI 代理在本 Unity 项目中高效变更、修复和增量实现功能，聚焦于架构、数据流、约定与开发工作流。

### 一、项目架构与核心流转
- **主场景**：`Assets/Scenes/SampleScene.unity`，完整捕鱼流程入口。
- **核心管理器**：
  - `Assets/PFKcode/Manager/GameFlowManager.cs`：全局流程与 UI/场景切换。
  - `Assets/PFKcode/Manager/FishingMiniGameManager.cs`：小游���主控，调度 ProgressBar、PlayerBarController、FishController。
  - `Assets/PFKcode/Controller/FishController.cs`：鱼的行为、边界、生命周期（`Initialize()` 参数关键）。
  - `Assets/PFKcode/Manager/InventoryManager.cs`、`Assets/PFKcode/Controller/TreasureChestController.cs`：掉落、背包、宝箱集成。
- **数据驱动**：
  - 可钓物（鱼/宝箱等）为 ScriptableObject（`CatchableData`，见 `Assets/PFKcode/DataModels/` 与 `Assets/CatchableID/`）。
  - 行为序列用 `[SerializeReference]` 多态保存 `FishAction` 子类，支持扩展。

### 二、项目约定与模式
- **统一帧更新**：所有帧逻辑通过 `FishingMiniGameManager.Update()` 调度，禁止各自 MonoBehaviour.Update()，新逻辑应注册到 manager。
- **多态序列化**：`CatchableData` 的 Calm/Struggle 等行为字段必须 `[SerializeReference]`，否则数据丢失。
- **UI/动画**：
  - UI 坐标系 Y=0 为面板中心，鱼的 Y 需 clamp 到 `FishMinYBoundary`/`Max`（`FishController`）。
  - 动画用 AnimationCurve，提示/淡入淡出用 CanvasGroup+Coroutine，避免同步定时器。
- **场景引用**：通过 Inspector `[Header]` 绑定，脚本签名变更后需在 SampleScene 重新绑定。

### 三、开发与调试工作流
- **编辑/运行**：
  - 推荐用 Unity 编辑器打开 `GreatFinsherman.sln` 或 SampleScene，C# 脚本热重载。
  - 主要回归靠 Play 模式手动验证，无自动化测试/CI。
- **常见陷阱**：
  - 忘记 `[SerializeReference]` 导致行为丢失。
  - 控制器间隐式 Update 调用，破坏生命周期一致性。
  - Inspector 字段未重新绑定。

### 四、AI 变更操作建议
1. 先定位相关 MonoBehaviour，查找 Initialize/Start/Update/API，理解数据流（如谁向 InventoryManager 发送 AddItem）。
2. 新增鱼行为：继承 `FishAction`，加 `[System.Serializable]`，实现 `IEnumerator Execute()`，并在目标 `CatchableData` 资产中添加。
3. UI/动画变更：优先用 CanvasGroup+Coroutine、AnimationCurve，暴露 Inspector 字段。
4. 提交变更时，PR 描述需写明：变更文件、影响流程、人工回归步骤（SampleScene 验证）。

### 五、示例片段
- 新增鱼行为：
  ```csharp
  [System.Serializable]
  public class FishJumpAction : FishAction {
      public override IEnumerator Execute(FishController fish) { /* ... */ }
  }
  ```
- 注册帧逻辑：
  ```csharp
  fishingMiniGameManager.RegisterUpdateHandler(MyHandler);
  ```

 如需具体代码片段或补丁示例，请说明目标文件或功能点。

### 六、简明类比解释方法（面向初学者）
- 当遇到复杂的代码结构（如 `Dictionary<string, InventoryItem>`）时，使用多行简单语句做类比：
- `private int count = 1;`  
  - 单一整数变量，类似“数量”。
- `private string name = "Bob";`  
  - 单一字符串变量，表示文本值。
- `private List<InventoryItem> items = new List<InventoryItem>();`  
  - 列表按顺序存储，多次查找需要遍历，时间复杂度 O(n)。
- `private Dictionary<string, int> counts = new Dictionary<string, int>();`  
  - 字典：键是名称，值是数量；查找/更新均为 O(1)。
- `private Dictionary<string, InventoryItem> _items = new Dictionary<string, InventoryItem>();`  
  - 实际用法：键（string）是物品 ID 或名称，值（InventoryItem）保存物品信息，支持快速添加、查找、更新。

- 对每一句简单示例，都附带一句简短注释，帮助初学者建立从“简单语句”到“项目代码”的映射关系。