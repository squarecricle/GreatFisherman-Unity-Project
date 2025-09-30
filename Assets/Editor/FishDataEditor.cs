using UnityEditor;
using UnityEngine;
using System.Collections.Generic; // <-- 新增
using System.Linq;               // <-- 新增

[CustomEditor(typeof(FishData))]
public class FishDataEditor : Editor
{
    // 步骤 1: 提名两个变量作为一会fishdata数据里的代理人
    SerializedProperty calmBehaviorProp;
    SerializedProperty struggleBehaviorProp;

    private List<System.Type> _fishActionTypes;// 用于存放所有继承自 FishAction 的类型
    private string[] _fishActionTypeNames;// 用于下拉菜单显示
    private int _selectedActionTypeIndex = 0;// 下拉菜单当前选中的索引

    // OnEnable 方法在选中对象、脚本被加载时调用
    private void OnEnable()
    {
        // 步骤 2: 让代理人对接到FishData里的真实数据
        calmBehaviorProp = serializedObject.FindProperty("CalmBehaviorSequence");
        struggleBehaviorProp = serializedObject.FindProperty("StruggleBehaviorSequence");
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
        // 移除旧的LabelField, 改为调用我们新的辅助方法
        DrawBehaviorList(calmBehaviorProp, "冷静行为序列 (Calm Behavior)");
        DrawBehaviorList(struggleBehaviorProp, "挣扎行为序列 (Struggle Behavior)");

        //记住刚才对代理人干的事情
        serializedObject.ApplyModifiedProperties();
    }
    private void DrawBehaviorList(SerializedProperty listProperty, string label)
    {
        //绘制一个可折叠的区域标题。
        // listProperty.isExpanded 会自动为每个列表记住它们的折叠状态
        EditorGUILayout.Space(); // 添加一点垂直间距，让UI更美观
        listProperty.isExpanded = EditorGUILayout.Foldout(listProperty.isExpanded, label, true);
        //EditorGUILayout.Foldout(目前列表状态, 列表标签（名字）, 点击模式true：整行点那里点都行)，但这个函数并没有改变listProperty.isExpanded的值，所以我们需要手动赋值
        //如果用户展开了这个区域，我们就绘制列表的内容
        if (listProperty.isExpanded)
        {
            // 增加缩进，让列表内容看起来有层级感
            EditorGUI.indentLevel++;

            //智能绘制UI（生成一个“代理对象”实例关于括号内部（列表尺寸的地址））
            EditorGUILayout.PropertyField(listProperty.FindPropertyRelative("Array.size"));
            // 这行代码会自动处理用户对Size字段的修改
            //遍历列表中的每一个元素并绘制它们
            for (int i = 0; i < listProperty.arraySize; i++)
            {
                // --- 水平布局，让删除按钮和元素在同一行 ---
                EditorGUILayout.BeginHorizontal();

                SerializedProperty elementProp = listProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(elementProp, new GUIContent("Element " + i), true);

                // --- 删除按钮 ---
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    // 先置空引用，防止Unity报错
                    elementProp.managedReferenceValue = null;
                    // 然后从列表中删除
                    listProperty.DeleteArrayElementAtIndex(i);
                    // 退出循环，因为列表长度已改变
                    break; 
                }
                EditorGUILayout.EndHorizontal();
            }

            // --- 添加新元素的功能区 ---
            EditorGUILayout.Space();
            
            // --- 开始水平布局，让下拉菜单和添加按钮在同一行 ---
            EditorGUILayout.BeginHorizontal();
            
            // 绘制下拉菜单
            _selectedActionTypeIndex = EditorGUILayout.Popup("选择行为类型", _selectedActionTypeIndex, _fishActionTypeNames);
            
            // 绘制添加按钮
            if (GUILayout.Button("添加行为"))
            {
                //通过索引从_fishActionTypes列表中获取用户在下拉菜单中选择的类型。
                System.Type selectedType = _fishActionTypes[_selectedActionTypeIndex];
                // 这是修改列表长度的“官方”方式。我们让“总代理人”将真实列表的 size 加一，这会在列表的末尾创建一个新的、空的元素“格子”。
                listProperty.arraySize++;//也就是后面的
                // 我们获取这个刚刚创建的、位于列表最末端的空“格子”的“专属代理人”。
                SerializedProperty newElement = listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1);
                // 创建所选类型的实例，并赋值给新元素
                newElement.managedReferenceValue = System.Activator.CreateInstance(selectedType);
            }
            // 结束水平布局
            EditorGUILayout.EndHorizontal();

            // 恢复缩进
            EditorGUI.indentLevel--;
        }
    }
}