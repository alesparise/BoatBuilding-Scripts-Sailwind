using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CreateBoatPart))]
public class CreateBoatPartEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CreateBoatPart script = (CreateBoatPart)target;

        EditorGUI.BeginDisabledGroup(script.isNewPart);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("partIndex"));
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.HelpBox("Part Title is only used in the editor to make the BoatCustomParts script clearer to read.", MessageType.Info);

        serializedObject.ApplyModifiedProperties();

        // Button to create the mast
        if (GUILayout.Button("Create Boat Part"))
        {
            script.DoCreate();
        }
    }

}
