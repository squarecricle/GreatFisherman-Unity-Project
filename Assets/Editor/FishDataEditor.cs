using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;               
using UnityEditorInternal;// 用于处理可序列化列表

[CustomEditor(typeof(FishData))]
public class FishDataEditor : Editor
{
    // 步骤 1: 提名两个变量作为一会fishdata数据里的代理人
    SerializedProperty calmBehaviorProp;
    SerializedProperty struggleBehaviorProp;

    private ReorderableList _calmList;
    private ReorderableList _struggleList;
    private List<System.Type> _fishActionTypes;// 用于存放所有继承自 FishAction 的类型
    private string[] _fishActionTypeNames;// 用于下拉菜单显示

    // OnEnable 方法在选中对象、脚本被加载时调用
    private void OnEnable()
    {
        // 步骤 2: 让代理人对接到FishData里的真实数据
        calmBehaviorProp = serializedObject.FindProperty("CalmBehaviorSequence");
        struggleBehaviorProp = serializedObject.FindProperty("StruggleBehaviorSequence");

        // 初始化冷静行为列表
        _calmList = new ReorderableList(serializedObject, calmBehaviorProp, true, true, true, true);
        SetupReorderableList(_calmList, "冷静行为序列 (Calm Behavior)");

        // 初始化挣扎行为列表
        _struggleList = new ReorderableList(serializedObject, struggleBehaviorProp, true, true, true, true);
        SetupReorderableList(_struggleList, "挣扎行为序列 (Struggle Behavior)");

        // 获取所有继承自 FishAction 的非抽象类类型
        _fishActionTypes = System.AppDomain.CurrentDomain.GetAssemblies()//获取当前程序正在运行的所有“代码库”（程序集 Assemblies）。这包括了您自己项目的代码、Unity引擎的代码以及您可能引入的任何第三方插件的代码。返回一个代码库的集合。
            .SelectMany(assembly => assembly.GetTypes())//遍历上一步获取到的每一个“代码库”，然后从每个库中取出它所包含的所有类型。
                                                        //SelectMany的作用是将这些来自不同代码库的类型列表“拍平”，合并成一个巨大的、包含所有类型的单一列表。
            .Where(type => type.IsSubclassOf(typeof(FishAction)) && !type.IsAbstract)//筛选出所有继承自 FishAction 的类型，同时排除掉抽象类（因为抽象类不能被实例化）。
            .ToList();//将所有通过筛选的、符合条件的 Type 对象，最终集合成一个 List<Type> 类型的列表，并赋值给 _fishActionTypes 变量。

        // 创建一个对用户更友好（去掉_Action）的名称数组，用于下拉菜单显示
        _fishActionTypeNames = _fishActionTypes.Select(type => type.Name.Replace("_Action", "")).ToArray();

    }

    public override void OnInspectorGUI()
    {
        //对接一下FishData里最新消息
        serializedObject.Update();

        //除了两个代理人，其他属性都用默认的方式绘制
        DrawPropertiesExcluding(serializedObject, "CalmBehaviorSequence", "StruggleBehaviorSequence");

        //这两个代理人被单独伶出来等着被调教
        //DrawBehaviorList(calmBehaviorProp, "冷静行为序列 (Calm Behavior)");
        //DrawBehaviorList(struggleBehaviorProp, "挣扎行为序列 (Struggle Behavior)");
        _calmList.DoLayoutList();
        _struggleList.DoLayoutList();
        //记住刚才对代理人干的事情
        serializedObject.ApplyModifiedProperties();
    }
    // private void DrawBehaviorList(SerializedProperty listProperty, string label)
    
    // {
    //     //绘制一个可折叠的区域标题。
    //     // listProperty.isExpanded 会自动为每个列表记住它们的折叠状态
    //     EditorGUILayout.Space(); // 添加一点垂直间距，让UI更美观
    //     listProperty.isExpanded = EditorGUILayout.Foldout(listProperty.isExpanded, label, true);
    //     //EditorGUILayout.Foldout(目前列表状态, 列表标签（名字）, 点击模式true：整行点那里点都行)，但这个函数并没有改变listProperty.isExpanded的值，所以我们需要手动赋值
    //     //如果用户展开了这个区域，我们就绘制列表的内容
    //     if (listProperty.isExpanded)
    //     {
    //         // 增加缩进，让列表内容看起来有层级感
    //         EditorGUI.indentLevel++;

    //         //智能绘制UI（生成一个“代理对象”实例关于括号内部（列表尺寸的地址））
    //         EditorGUILayout.PropertyField(listProperty.FindPropertyRelative("Array.size"));
    //         // 这行代码会自动处理用户对Size字段的修改
    //         //遍历列表中的每一个元素并绘制它们
    //         for (int i = 0; i < listProperty.arraySize; i++)
    //         {
    //             // --- 水平布局，让删除按钮和元素在同一行 ---
    //             EditorGUILayout.BeginHorizontal();

    //             SerializedProperty elementProp = listProperty.GetArrayElementAtIndex(i);
    //             EditorGUILayout.PropertyField(elementProp, new GUIContent("Element " + i), true);

    //             // --- 删除按钮 ---
    //             if (GUILayout.Button("X", GUILayout.Width(25)))
    //             {
    //                 // 先置空引用，防止Unity报错
    //                 elementProp.managedReferenceValue = null;
    //                 // 然后从列表中删除
    //                 listProperty.DeleteArrayElementAtIndex(i);
    //                 // 退出循环，因为列表长度已改变
    //                 break;
    //             }
    //             EditorGUILayout.EndHorizontal();
    //         }

    //         // --- 添加新元素的功能区 ---
    //         EditorGUILayout.Space();

    //         // --- 开始水平布局，让下拉菜单和添加按钮在同一行 ---
    //         EditorGUILayout.BeginHorizontal();

    //         // 绘制下拉菜单
    //         _selectedActionTypeIndex = EditorGUILayout.Popup("选择行为类型", _selectedActionTypeIndex, _fishActionTypeNames);

    //         // 绘制添加按钮
    //         if (GUILayout.Button("添加行为"))
    //         {
    //             //通过索引从_fishActionTypes列表中获取用户在下拉菜单中选择的类型。
    //             System.Type selectedType = _fishActionTypes[_selectedActionTypeIndex];
    //             // 这是修改列表长度的“官方”方式。我们让“总代理人”将真实列表的 size 加一，这会在列表的末尾创建一个新的、空的元素“格子”。
    //             listProperty.arraySize++;//也就是后面的
    //             // 我们获取这个刚刚创建的、位于列表最末端的空“格子”的“专属代理人”。
    //             SerializedProperty newElement = listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1);
    //             // 创建所选类型的实例，并赋值给新元素
    //             newElement.managedReferenceValue = System.Activator.CreateInstance(selectedType);
    //         }
    //         // 结束水平布局
    //         EditorGUILayout.EndHorizontal();

    //         // 恢复缩进
    //         EditorGUI.indentLevel--;
    //     }
    // }
    /// <summary>
    /// 辅助方法：配置一个ReorderableList的外观和行为
    /// </summary>
    private void SetupReorderableList(ReorderableList list, string headerText)
{
    // --- 绘制列表标题 (这部分不变) ---
    list.drawHeaderCallback = (Rect rect) =>
    {
        EditorGUI.LabelField(rect, headerText);//在列表顶部绘制一个标签，显示传入的标题文本
    };

    // --- 动态计算每个元素的高度 (这部分不变) ---
    list.elementHeightCallback = (int index) =>// Lambda 表达式，定义一个匿名函数,计算结果返回给elementHeightCallback属性，这个属性负责告诉ReorderableList每个元素需要多高的显示空间
    {
        SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);//获取当前索引的元素的“专属代理人”
        return EditorGUI.GetPropertyHeight(element, GUIContent.none, true) + 4f;//计算这个元素在Inspector中所需的高度，并加上4像素的额外间距
    };

    // --- 自定义绘制每个元素 ---
    list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
    {
        SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);

        // 1. 【解决重叠】我们手动向右缩进15像素，为左侧的拖拽图标留出专属空间
        rect.x += 15;
        rect.width -= 15;

        // 2. 【实现标题】调用我们刚写好的方法来生成动态标题
        string title = GetElementTitle(element);

        // 3. 绘制属性，但这次传入我们自己生成的、带有信息的标题
        EditorGUI.PropertyField(
            new Rect(rect.x, rect.y + 2f, rect.width, EditorGUIUtility.singleLineHeight),//我们给PropertyField传入一个新的Rect，这个Rect在原有的基础上向下移动了2像素，以避免与上方的边框重叠
            element, //传入当前元素的“专属代理人”
            new GUIContent(title), // 使用我们自己的标题
            true);// true表示如果这个属性本身是一个复杂类型（有子属性），就展开显示它的所有子属性
    };

    // --- 处理点击“+”号按钮的事件 ---
    list.onAddDropdownCallback = (Rect buttonRect, ReorderableList l) =>//当用户点击列表右上角的“+”按钮时，这个回调函数会被触发
    {
        var menu = new GenericMenu();//创建一个新的通用菜单对象，稍后我们会向这个菜单添加选项
        // 遍历所有可用的行为类型，并为每一种类型添加一个菜单项
        for (int i = 0; i < _fishActionTypes.Count; i++)
        {
            int localIndex = i; // 创建一个局部变量以避免闭包问题
            menu.AddItem(new GUIContent(_fishActionTypeNames[localIndex]), false, () => {//
                var property = l.serializedProperty;//获取当前列表的“总代理人”
                property.arraySize++;
                l.index = property.arraySize - 1;
                SerializedProperty newElement = property.GetArrayElementAtIndex(l.index);
                newElement.managedReferenceValue = System.Activator.CreateInstance(_fishActionTypes[localIndex]);
                serializedObject.ApplyModifiedProperties();
            });
        }
        menu.ShowAsContext();//显示这个菜单，位置在用户点击的按钮附近
    };
}
    /// <summary>
    /// 辅助方法：根据Action的具体内容生成一个可读的标题
    /// </summary>
    private string GetElementTitle(SerializedProperty element)
    {
        // managedReferenceValue可以直接获取到这个属性背后的实际类实例
        object actionObject = element.managedReferenceValue;
        if (actionObject == null)
        {
            return "Element is null!";
        }

        // 获取类型名并美化一下（例如, "Move_Action" -> "Move"）
        string typeName = actionObject.GetType().Name.Replace("_Action", "");

        // 根据不同的Action类型，显示不同的关键参数
        switch (actionObject)
        {
            case Move_Action move:
                return $"{typeName} | Duration: {move.Duration:F1}s";
            case Wait_Action wait:
                return $"{typeName} | Duration: {wait.Duration:F1}s";
            case Jump_Action jump:
                return $"{typeName} | Pause: {jump.PauseDurationRange.x:F1}s - {jump.PauseDurationRange.y:F1}s";
            case ChangeSpeed_Action speed:
                return $"{typeName} | New Speed: {speed.NewSpeed}";
            case Jitter_Action jitter:
                return $"{typeName} | Distance: {jitter.MinMaxMoveDistance.x}-{jitter.MinMaxMoveDistance.y}";
            default:
                return typeName;
        }
    }




}