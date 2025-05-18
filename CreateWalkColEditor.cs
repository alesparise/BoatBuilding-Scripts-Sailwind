using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CreateWalkCol))]
public class CreateWalkColEditor : Editor
{   /// Goal is to have the walk col generated at the click of a button
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CreateWalkCol myScript = (CreateWalkCol)target;
        if (GUILayout.Button("Create Walk Col"))
        {
            myScript.DoCreate();
        }
    }
}
