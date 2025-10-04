// 文件名建议为 CatchableDataEditor.cs (放在Editor文件夹内)
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;

// =======================================================================
// 1. 这是“父编辑器”，包含了所有通用的绘制逻辑
// 注意：这个父类前面没有 [CustomEditor] 属性
// =======================================================================
public abstract class CatchableDataEditor : Editor
{
    protected SerializedProperty calmBehaviorProp;
    protected SerializedProperty struggleBehaviorProp;

    private ReorderableList _calmList;
    private ReorderableList _struggleList;
    private List<System.Type> _fishActionTypes;
    private string[] _fishActionTypeNames;

    protected virtual void OnEnable()
    {
        calmBehaviorProp = serializedObject.FindProperty("CalmBehaviorSequence");
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

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // 绘制除了行为列表之外的所有其他属性
        DrawPropertiesExcluding(serializedObject, "CalmBehaviorSequence", "StruggleBehaviorSequence", "m_Script");

        EditorGUILayout.Space(); // 加个间距，更美观

        // 绘制行为列表
        _calmList.DoLayoutList();
        _struggleList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }

    private void SetupReorderableList(ReorderableList list, string headerText)
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
// 2. 这是FishData的“子编辑器”，非常简洁
// =======================================================================
[CustomEditor(typeof(FishData))]
public class FishDataEditor : CatchableDataEditor
{
    // 它自动继承了父类的所有功能，我们什么都不用写！
}


// =======================================================================
// 3. 这是TrashData的“子编辑器”，同样非常简洁
// =======================================================================
[CustomEditor(typeof(TrashData))]
public class TrashDataEditor : CatchableDataEditor
{
    // 它也自动继承了父类的所有功能！
}
// TreasureChestDataEditor.cs

[CustomEditor(typeof(TreasureChestData))]
public class TreasureChestDataEditor : CatchableDataEditor
{
    // 完美！我们什么都不用写。
    // 它会自动继承父类 CatchableDataEditor 的所有强大功能。
}