using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CreateMast))]
public class CreateMastEditor : Editor
{
    /*public override void OnInspectorGUI()
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
    }*/

    private CreateMast script;
    private float maxHeight;

    public void OnEnable()
    {
        script = (CreateMast)target;
        maxHeight = script.maxHeight;
    }
    public override void OnInspectorGUI()
    {
        #region ErrorBox
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
        #endregion

        #region ShowVanillaInspector
        //Debug things to compare with original inspector, please ignore
        //base.OnInspectorGUI();
        //GUIStyle s = new GUIStyle(GUI.skin.label);
        //s.richText = true;
        //EditorGUILayout.LabelField("<b>CUSTOM PART HERE</b>", s);
        #endregion

        serializedObject.Update();

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        EditorGUI.EndDisabledGroup();

        //prefabs
        EditorGUILayout.PropertyField(serializedObject.FindProperty("winchPrefab"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("blockPrefab"));

        //mast options
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxSails"));
        
        SerializedProperty mastHeight = serializedObject.FindProperty("mastHeight");
        EditorGUILayout.Slider(mastHeight, 0f, maxHeight);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("onlyStaysails"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("bowsprit"));

        //script options
        EditorGUILayout.PropertyField(serializedObject.FindProperty("selfDestruct"));


        serializedObject.ApplyModifiedProperties();
    }
}
