using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CreateMast))]
public class CreateMastEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CreateMast script = (CreateMast)target;

        Mast m = script.GetComponent<Mast>();
        CapsuleCollider cc = script.GetComponent<CapsuleCollider>();
        if (m != null || cc != null)
        {
            string error = "This object already has a ";
            if (m != null && cc != null) error += "Mast and CapsuleCollider Components";
            else if (cc != null) error += "CapsuleCollider Component";
            else if (m != null) error += "Mast Component";

            EditorGUILayout.HelpBox(error, MessageType.Error);
            return;
        }

        base.OnInspectorGUI();

        if (GUILayout.Button("Create Mast"))
        {
            script.DoCreate();
        }
    }
}
