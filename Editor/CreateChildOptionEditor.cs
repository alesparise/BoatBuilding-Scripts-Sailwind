#if (UNITY_EDITOR)
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CreateChildOption))]
public class CreateChildOptionEditor : Editor
{
    private CreateChildOption script;
    void OnEnable()
    {
        script = (CreateChildOption)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Assign Child Options"))
        {
            script.DoCreate();
        }
    }
}
#endif