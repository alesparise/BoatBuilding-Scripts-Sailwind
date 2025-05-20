using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CreateMast))]
public class CreateMastEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CreateMast script = (CreateMast)target;
        if (GUILayout.Button("Create Mast"))
        {
            script.DoCreate();
        }
    }
}
