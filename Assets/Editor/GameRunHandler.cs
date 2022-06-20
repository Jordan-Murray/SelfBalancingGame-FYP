using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameRunHandler
{
    SerializedProperty serProp;
    public List<string> propNameList = new List<string>();
    public int numberOfRuns;
    public int scriptStringIndex = 0;
    public int scriptPropertyIndex = 0;

    public List<string> GetProptiesFromAllScripts(GameObject source)
    {
        var scripts = source.GetComponents<MonoBehaviour>();
        var scriptStringArray = new List<string>();
        foreach (var item in scripts)
        {
            scriptStringArray.Add(item.ToString());
        }
        SerializedObject serObj = new SerializedObject(scripts[scriptStringIndex]);
        SerializedProperty prop = serObj.GetIterator();
        while (prop.NextVisible(true))
        {
            propNameList.Add(prop.name);
        }
        serProp = serObj.FindProperty(propNameList[scriptPropertyIndex]);
        switch (serProp.propertyType)
        {
            case SerializedPropertyType.Boolean:
                EditorGUILayout.LabelField(serProp.boolValue.ToString());
                break;
            case SerializedPropertyType.Bounds:
                EditorGUILayout.LabelField(serProp.boundsValue.ToString());
                break;
            case SerializedPropertyType.Color:
                EditorGUILayout.LabelField(serProp.colorValue.ToString());
                break;
            case SerializedPropertyType.Float:
                EditorGUILayout.LabelField(serProp.floatValue.ToString());
                break;
            case SerializedPropertyType.Integer:
                EditorGUILayout.LabelField(serProp.intValue.ToString());
                serProp.intValue = numberOfRuns;
                break;
            case SerializedPropertyType.ObjectReference:
                EditorGUILayout.LabelField(serProp.objectReferenceValue.ToString());
                break;
            case SerializedPropertyType.Rect:
                EditorGUILayout.LabelField(serProp.rectValue.ToString());
                break;
            case SerializedPropertyType.String:
                EditorGUILayout.LabelField(serProp.stringValue.ToString());
                break;
            case SerializedPropertyType.Vector2:
                EditorGUILayout.LabelField(serProp.vector2Value.ToString());
                break;
            case SerializedPropertyType.Vector3:
                EditorGUILayout.LabelField(serProp.vector3Value.ToString());
                break;
        }
        serObj.ApplyModifiedProperties();
        return scriptStringArray;
    }
}
