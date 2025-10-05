// 文件名: CatchableEditor.cs
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;

// =======================================================================
// 1. 基类编辑器：负责提供公共变量和列表绘制逻辑
// =======================================================================
public abstract class CatchableDataEditor : Editor
{
    // 改为 protected，以便子类可以访问
    protected SerializedProperty calmBehaviorProp;
    protected SerializedProperty struggleBehaviorProp;
    protected ReorderableList _calmList;
    protected ReorderableList _struggleList;

    private List<System.Type> _fishActionTypes;
    private string[] _fishActionTypeNames;

    protected virtual void OnEnable()
    {
        calmBehaviorProp = serializedObject.FindProperty("CalmBehaviorSequence");//
        struggleBehaviorProp = serializedObject.FindProperty("StruggleBehaviorSequence");

        _calmList = new ReorderableList(serializedObject, calmBehaviorProp, true, true, true, true);
        SetupReorderableList(_calmList, "冷静行为序列 (Calm Behavior)");
        _struggleList = new ReorderableList(serializedObject, struggleBehaviorProp, true, true, true, true);
        SetupReorderableList(_struggleList, "挣扎行为序列 (Struggle Behavior)");

        _fishActionTypes = System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(FishAction)) && !type.IsAbstract)
            .ToList();
        _fishActionTypeNames = _fishActionTypes.Select(type => type.Name.Replace("_Action", "")).ToArray();
    }

    // 基类不再提供通用的OnInspectorGUI，由各个子类自己实现
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("这是一个CatchableData的基类编辑器。");
    }

    private void SetupReorderableList(ReorderableList list, string headerText)//绘制多行为序列
    {
        list.drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, headerText);
        list.elementHeightCallback = (int index) =>
        {
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element, GUIContent.none, true) + 4f;
        };
        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
            rect.x += 15;
            rect.width -= 15;
            string title = GetElementTitle(element);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + 2f, rect.width, EditorGUIUtility.singleLineHeight), element, new GUIContent(title), true);
        };
        list.onAddDropdownCallback = (Rect buttonRect, ReorderableList l) =>
        {
            var menu = new GenericMenu();
            for (int i = 0; i < _fishActionTypes.Count; i++)
            {
                int localIndex = i;
                menu.AddItem(new GUIContent(_fishActionTypeNames[localIndex]), false, () =>
                {
                    var property = l.serializedProperty;
                    property.arraySize++;
                    l.index = property.arraySize - 1;
                    SerializedProperty newElement = property.GetArrayElementAtIndex(l.index);
                    newElement.managedReferenceValue = System.Activator.CreateInstance(_fishActionTypes[localIndex]);
                    serializedObject.ApplyModifiedProperties();
                });
            }
            menu.ShowAsContext();
        };
    }

    private string GetElementTitle(SerializedProperty element)
    {
        object actionObject = element.managedReferenceValue;
        if (actionObject == null) return "Element is null!";
        
        string typeName = actionObject.GetType().Name.Replace("_Action", "");
        switch (actionObject)
        {
            case Move_Action move: return $"{typeName} | T:{move.MoveTimes}, MaxD:{move.MaxDurationPerMove:F1}";
            case Wait_Action wait: return $"{typeName} | D: {wait.Duration:F1}s";
            case Jump_Action jump: return $"{typeName} | P: {jump.PauseDurationRange.x:F1}s-{jump.PauseDurationRange.y:F1}s";
            case ChangeSpeed_Action speed: return $"{typeName} | New Speed: {speed.NewSpeed}";
            case Jitter_Action jitter: return $"{typeName} | Dist: {jitter.MinMaxJitterDistance.x}-{jitter.MinMaxJitterDistance.y}";
            default: return typeName;
        }
    }
}


// =======================================================================
// 2. FishData的子编辑器：绘制所有属性
// =======================================================================
[CustomEditor(typeof(FishData))]
public class FishDataEditor : CatchableDataEditor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 使用DrawPropertiesExcluding可以方便地绘制所有属性，除了我们想手动处理的
        DrawPropertiesExcluding(serializedObject, "m_Script", "CalmBehaviorSequence", "StruggleBehaviorSequence");

        EditorGUILayout.Space();
        
        // 手动绘制行为列表
        _calmList.DoLayoutList();
        _struggleList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }
}


// =======================================================================
// 3. TrashData的子编辑器：只绘制CatchableData中的通用属性
// =======================================================================
[CustomEditor(typeof(TrashData))]
public class TrashDataEditor : CatchableDataEditor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.LabelField("垃圾：作为可钓物参与博弈", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ItemName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Description"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ItemIcon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("BaseWeight"));
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("博弈小游戏参数", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("InitialNormalizedPosition"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("BaseMoveSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("StruggleThreshold"));

        EditorGUILayout.Space();

        // 绘制行为列表
        _calmList.DoLayoutList();
        _struggleList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }
}


// =======================================================================
// 4. TreasureChestData的子编辑器：只绘制核心信息和宝箱专属属性
// =======================================================================
[CustomEditor(typeof(TreasureChestData))]
public class TreasureChestDataEditor : CatchableDataEditor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("宝箱：作为额外奖励出现", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ItemName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Description"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ItemIcon"));
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("宝箱专属设置", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Tier"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("DropChance"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("LootCountRange"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("LootPool"));

        serializedObject.ApplyModifiedProperties();
    }
}