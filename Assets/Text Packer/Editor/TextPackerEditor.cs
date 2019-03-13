using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TextPacker))]
public class TextPackerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var textPacker = (TextPacker)target;
        base.OnInspectorGUI();
        GUI.enabled = !textPacker.InProgress;
        if (GUILayout.Button(textPacker.InProgress ? "Generating..." : "Generate"))
        {
            textPacker.Generate();
        }
        // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
        serializedObject.Update();
    }
}
