using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SaveLevelToJson))]
public class LevelEditorButtons : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SaveLevelToJson saveLevelToJson = (SaveLevelToJson)target;
        if (GUILayout.Button("Save Level"))
        {
            saveLevelToJson.Save();
        }

        if (GUILayout.Button("Sort Level"))
        {
            saveLevelToJson.SortObjectsIntoWorld();
        }
    }
}
