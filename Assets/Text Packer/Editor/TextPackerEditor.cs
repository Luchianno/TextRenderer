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
        GUILayout.Label($"Debug: InProgress ={textPacker.InProgress}");
        GUI.enabled = !textPacker.InProgress;
        if (GUILayout.Button(textPacker.InProgress ? $"Generating ({textPacker.Progress * 100:00}%)..." : "Generate"))
        {
            textPacker.Generate();

        }
        else
        {
            GUI.enabled = textPacker.InProgress;
            if (GUILayout.Button("Stop"))
                textPacker.StopGeneration();
        }

        if (textPacker.InProgress)
            this.Repaint();
    }
}
