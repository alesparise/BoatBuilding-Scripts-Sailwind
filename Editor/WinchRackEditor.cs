#if (UNITY_EDITOR)
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WinchRack))]
public class WinchRackEditor : Editor
{
    private WinchRack script;
    public void OnEnable()
    {
        script = (WinchRack)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (script.isSymmetrical && script.other == null && script.winches.Count == 0)
        {
            script.InstantiateOther();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(script);
        }
    }
}
#endif