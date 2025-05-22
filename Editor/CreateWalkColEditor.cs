using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CreateWalkCol))]
public class CreateWalkColEditor : Editor
{   /// Goal is to have the walk col generated at the click of a button

    bool showObjects = true;

    public override void OnInspectorGUI()
    {
        CreateWalkCol script = (CreateWalkCol)target;

        if (script.transform.parent.Find("WALK " + script.name) != null)
        {
            EditorGUILayout.HelpBox("This object already has a Walk Col", MessageType.Error);
            return;
        }
        base.OnInspectorGUI();
        //DrawPropertiesExcluding(serializedObject, "m_Script", "objectsToRemove");

        //manually draw the objectToRemove array
        serializedObject.Update();

        SerializedProperty prop = serializedObject.FindProperty("objectsToRemove");
        ShowArray(prop);


        serializedObject.ApplyModifiedProperties();

        string toRemove = "Objects that will be deleted are:\n  • object with 'winch_' in the name\n  • object with 'block_' in the name";
        foreach (string obj in script.objectsToRemove)
        {
            toRemove += "\n  • object named '" + obj + "'";
        }
        EditorGUILayout.HelpBox(toRemove, MessageType.Info);
        if (GUILayout.Button("Create Walk Col"))
        {
            script.DoCreate();
        }
    }
    private void ShowArray(SerializedProperty array)
    {
        showObjects = EditorGUILayout.Foldout(showObjects, "Objects to remove");
        if (showObjects)
        {
            EditorGUILayout.PropertyField(array.FindPropertyRelative("Array.size"), new GUIContent("Number Of Objects"));
            EditorGUI.indentLevel++;
            if (array.isExpanded)
            {
                for (int i = 0; i < array.arraySize; i++)
                {
                    SerializedProperty element = array.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(element, new GUIContent("Object " + (i)));
                }
            }
            EditorGUI.indentLevel--;
        }
    }
}
