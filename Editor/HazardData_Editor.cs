using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HazardData))]
public class HazardData_Editor : Editor
{
    private SerializedProperty HazardName;
    private SerializedProperty WarningVisualPrefab;
    private SerializedProperty HazardPrefab;
    private SerializedProperty WarningTime;
    private SerializedProperty EffectType;
    private SerializedProperty EffectValue;
    private SerializedProperty RequiresBraking;
    private void OnEnable()
    {
        HazardName = serializedObject.FindProperty("hazardName");
        WarningVisualPrefab = serializedObject.FindProperty("WarningVisualPrefab");
        HazardPrefab = serializedObject.FindProperty("HazardPrefab");
        WarningTime = serializedObject.FindProperty("WarningTime");
        EffectType = serializedObject.FindProperty("EffectType");
        EffectValue = serializedObject.FindProperty("EffectValue");
        RequiresBraking = serializedObject.FindProperty("RequiresBraking");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.ApplyModifiedProperties();
        EditorGUILayout.PropertyField(HazardName);
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Timing", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(WarningTime);
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Gameplay Effect", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(EffectType);

        HazardeffectType effectTypeEnum = (HazardEffectType)EffectType.enumValueIndex;
        switch (effectTypeEnum)
        {
            case HazardeffectType.Damage:
                EditorGUILayout.PropertyField(EffectValue, new GUIContent("Damage Amount"));
                break;
            case HazardeffectType.InstantKill:
                EditorGUILayout.LabelField("This hazard will instantly kill the player.");
                break;
            case HazardeffectType.ControlLoss:
                EditorGUILayout.PropertyField(EffectValue, new GUIContent("Control Loss Duration(sec)"));
                break;
            case HazardeffectType.Slow:
                EditorGUILayout.PropertyField(EffectValue, new GUIContent("Slow Percentage (Percent)"));
                break;
        }
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Special Rules", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(RequiresBraking);
        serializedObject.ApplyModifiedProperties();
    }
}