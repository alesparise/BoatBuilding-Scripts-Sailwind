using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(LogParts))]
public class LogPartsEditor : Editor
{
    LogParts script;
    public void OnEnable()
    {
        script = (LogParts)target;
    }
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Log Parts"))
        {
            script.DoLog();
        }
    }
}
