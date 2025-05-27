#if (UNITY_EDITOR)
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RemoveAllCreate))]
public class RemoveAllCreateEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RemoveAllCreate script = (RemoveAllCreate)target;

        EditorGUILayout.HelpBox("This will remove all the CreateMast, CreateBoatPart, CreateWalkCol script from the prefab and then self destruct. Use when the prefab is done.", MessageType.Warning);

        base.OnInspectorGUI();

        if (GUILayout.Button("Remove BoatBuilding Components"))
        {
            script.DoRemove();
        }
    }
}
#endif