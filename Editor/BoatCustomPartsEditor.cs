using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

//written based on: https://www.reddit.com/r/Unity3D/comments/5nf87r/how_to_change_the_elements_name_of_an_array_in/

[CustomEditor(typeof(BoatCustomParts))]
public class BoatCustomPartsEditor : Editor
{
    private BoatCustomParts script;
    public override void OnInspectorGUI()
    {
        #region ShowVanillaInspector
        //Debug things to compare with original inspector, please ignore
        //base.OnInspectorGUI();
        //GUIStyle s = new GUIStyle(GUI.skin.label);
        //s.richText = true;
        //EditorGUILayout.LabelField("<b>CUSTOM PART HERE</b>", s);
        #endregion

        script = (BoatCustomParts)target;

        serializedObject.Update();

        SerializedProperty prop = serializedObject.GetIterator();
        if (prop.NextVisible(true))
        {
            do
            {
                EditorGUI.BeginDisabledGroup(prop.name == "m_Script");  //simulates the disabled field which shows the script
                
                DrawMyStuff(prop.Copy());
                
                EditorGUI.EndDisabledGroup();
            }
            // Skip the children (the draw will handle it)
            while (prop.NextVisible(false));
        }
        serializedObject.ApplyModifiedProperties();
    }
    
    public void DrawMyStuff(SerializedProperty prop)
    {
        if (EditorGUILayout.PropertyField(prop))
        {
            EditorGUI.indentLevel++;

            //first draw the size using Array.size, this way it can be changed dinamically
            SerializedProperty size = prop.FindPropertyRelative("Array.size");
            EditorGUILayout.PropertyField(size, new GUIContent("Number Of Parts"));

            // Head into our children
            prop.NextVisible(true);

            // first children is the size, but we already drew it so do nothing

            // Next is the array itself
            int index = 0;
            while (prop.NextVisible(false) && !SerializedProperty.EqualContents(prop, prop.GetEndProperty()))
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                string elementName = GetName(index);
                //string elementName = "aa";
                if (elementName != "")
                {
                    GUIStyle style = new GUIStyle(GUI.skin.label);
                    style.richText = true;
                    GUIContent label = new GUIContent("<b>" + elementName + "</b>");
                    EditorGUILayout.LabelField(label, style);
                }
                EditorGUILayout.PropertyField(prop, true);
                index++;
                EditorGUILayout.EndVertical();
            }
            EditorGUI.indentLevel--;
        }
    }
    private string GetName(int index)
    {
        List<BoatPart> availableParts = script?.availableParts;
        if (availableParts.Count == 0)
        {
            return "";
        }
        int activeOption = availableParts[index].activeOption;
        if (activeOption >= availableParts[index].partOptions.Count)
        {
            return "";
        }
        BoatPartOption option = availableParts[index].partOptions[activeOption];
        if (option == null)
        {
            return "";
        }
        CreateBoatPart cbp = option.GetComponent<CreateBoatPart>();
        if (cbp != null && cbp.partTitle != "")
        {
            return option.GetComponent<CreateBoatPart>().partTitle;
        }
        else
        {
            return option.optionName;
        }
    }
}

