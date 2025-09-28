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

    private List<System.Type> _fishActionTypes;
    private string[] _fishActionTypeNames;
    private int _selectedActionTypeIndex = 0;

    // OnEnable 方法在选中对象、脚本被加载时调用
    private void OnEnable()
    {
        // 步骤 2: 挑出FishData里两个属性，分配到这个代理人上

        calmBehaviorProp = serializedObject.FindProperty("CalmBehaviorSequence");
        struggleBehaviorProp = serializedObject.FindProperty("StruggleBehaviorSequence");
        _fishActionTypes = System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(FishAction)) && !type.IsAbstract)
            .ToList();

        // 创建一个对用户更友好的名称数组，用于下拉菜单显示
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
        //EditorGUILayout.Foldout(目前列表状态, 列表标签（名字）, 赋值为true)，但这个函数并没有改变listProperty.isExpanded的值，所以我们需要手动赋值
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
                // 获取当前列表第i个元素对应的代理人，命名为elementProp
                SerializedProperty elementProp = listProperty.GetArrayElementAtIndex(i);

                //使用“万能绘制工具”，根据刚刚获取的“第I个元素对应代理人”，在 Inspector 上画出对应的 UI 控件，名字为“Element i”，并且允许它展开子属性
                //EditorGUILayout.PropertyField(elementProp, new GUIContent("Element " + i), true);
                EditorGUILayout.LabelField("Element " + i, EditorStyles.boldLabel); // 用一个粗体标签代替默认的折叠
            
                // 关键：通过 managedReferenceValue 获取真实实例
                object actionObject = elementProp.managedReferenceValue;

                // 根据的实例具体类型，绘制不同的UI
                // 根据不同类型，通过代理人手动绘制各自专属的属性字段
                if (actionObject is Move_Action)
                {
                    EditorGUILayout.PropertyField(elementProp.FindPropertyRelative("Speed"));
                    EditorGUILayout.PropertyField(elementProp.FindPropertyRelative("Duration"));
                }
                else if (actionObject is Wait_Action)
                {
                    EditorGUILayout.PropertyField(elementProp.FindPropertyRelative("Duration"));
                }
                else if (actionObject is Jump_Action)
                {
                    EditorGUILayout.PropertyField(elementProp.FindPropertyRelative("PauseDurationRange"));
                }
                else if (actionObject is ChangeSpeed_Action)
                {
                    EditorGUILayout.PropertyField(elementProp.FindPropertyRelative("NewSpeed"));
                }
                else
                {
                    // 如果元素是null或者是一个未知的类型，显示一个帮助框
                    EditorGUILayout.HelpBox("这是一个空的或未知的行为类型。", MessageType.Warning);
                }
            }

            // 恢复缩进
            EditorGUI.indentLevel--;
        }
    }
}