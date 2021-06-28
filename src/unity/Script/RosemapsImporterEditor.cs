using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

[CustomEditor(typeof(RosemapsImporter))]
public class RosemapsImporterEditor : ScriptedImporterEditor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var title = new GUIContent("Map names to import");
        var prop = serializedObject.FindProperty("importNames");
        EditorGUILayout.PropertyField(prop, title);
        serializedObject.ApplyModifiedProperties();
        base.ApplyRevertGUI();
    }
}
