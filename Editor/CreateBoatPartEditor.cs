using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CreateBoatPart))]
public class CreateBoatPartEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CreateBoatPart script = (CreateBoatPart)target;
        if (script.GetComponent<BoatPartOption>() != null)
        {
            EditorGUILayout.HelpBox("This object already has a BoatPartOption component", MessageType.Error);
            return;
        }

        base.OnInspectorGUI();
        
        EditorGUI.BeginDisabledGroup(script.isNewPart);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("partIndex"));
        var (message, messageType) = ListParts(script);
        EditorGUILayout.HelpBox(message, messageType);
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.HelpBox("Part Title is only used in the editor to make the BoatCustomParts script clearer to read.", MessageType.Info);

        serializedObject.ApplyModifiedProperties();

        // Button to create the mast
        if (GUILayout.Button("Create Boat Part"))
        {
            script.DoCreate();
        }
    }
    private (string, MessageType) ListParts(CreateBoatPart script)
    {
        BoatCustomParts bcp = script.boat.GetComponent<BoatCustomParts>();
        if (bcp == null)
        {
            return ("The boat object does not have a BoatCustomParts component", MessageType.Error);
        }
        else if (script.partIndex >= bcp.availableParts.Count)
        {
            return ("There is no part with this index", MessageType.Error);
        }
        else
        {
            string list = "Parts at index " + script.partIndex + ":";
            foreach (BoatPartOption option in bcp.availableParts[script.partIndex].partOptions)
            {
                if (option != null && option.optionName != null)
                {
                    list += "\n  → " + option.optionName;
                }
                else
                {
                    list += "\n  → null";
                }
            }
            return (list, MessageType.None);
        }    
    }
}
