using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FishData))]
public class FishDataEditor : Editor
{
    // 步骤 1: 提名两个变量作为一会fishdata数据里的代理人
    SerializedProperty calmBehaviorProp;
    SerializedProperty struggleBehaviorProp;

    // OnEnable 方法在选中对象、脚本被加载时调用
    private void OnEnable()
    {
        // 步骤 2: 挑出FishData里两个属性，分配到这个代理人上
       
        calmBehaviorProp = serializedObject.FindProperty("CalmBehaviorSequence");
        struggleBehaviorProp = serializedObject.FindProperty("StruggleBehaviorSequence");
    }

    public override void OnInspectorGUI()
    {
        // 步骤 3: 对接一下FishData里最新消息
        serializedObject.Update();

        // 除了两个代理人，其他属性都用默认的方式绘制
        DrawPropertiesExcluding(serializedObject, "CalmBehaviorSequence", "StruggleBehaviorSequence");

        //这两个代理人被单独伶出来等着被调教
        EditorGUILayout.LabelField("冷静行为序列 (自定义UI将在这里实现)");
        EditorGUILayout.LabelField("挣扎行为序列 (自定义UI将在这里实现)");

        // 步骤 4: 记住刚才对代理人干的事情
        serializedObject.ApplyModifiedProperties();
    }
}