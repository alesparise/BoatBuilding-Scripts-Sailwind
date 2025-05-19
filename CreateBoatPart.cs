using System.Collections.Generic;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;
using UnityEditor;

public class CreateBoatPart : MonoBehaviour
{
    [Header("Boat Part Options")]
    [Tooltip("The name of the boat part option")]
    public string optionName;
    public int basePrice;
    public int installCost;
    public int mass;
    [Tooltip("The part category. 0 = masts, 1 = other, 2 = stays")]
    public int category;
    [Header("Script Options")]
    [Tooltip("This is only used in the editor to make the list in BoatCustomParts clearer")]
    public string partTitle;
    [Tooltip("Enable if it's a new part and not an option for an existing part")]
    public bool isNewPart;
    [Tooltip("Enable if the part should have an option to be fully removed. Will create an empty boat part option")]
    public bool canBeDisabled;
    [Tooltip("Set this to the index of the part in the BoatCustomParts script if it's NOT a new part")]
    [HideInInspector]
    public int partIndex;
    [Tooltip("Self destroy this script once done")]
    public bool selfDestruct;

    private GameObject boat;
    private Transform boatModel;
    private GameObject walkColObject;

    public void DoCreate()
    {
        boat = PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot;   //this is the prefab root object AKA the BOAT object
        LogGreen("Boat: " + boat.name);
        // add the boat option and set it up
        BoatPartOption option = gameObject.AddComponent<BoatPartOption>();
        option.optionName = optionName;
        option.basePrice = basePrice;
        option.installCost = installCost;
        option.mass = mass;
        //set the walk col reference
        FindBoatModel(transform.parent);    //find the boatModel object
        Transform walkCol = boat.transform.Find("WALK " + boatModel.name);  //find the WALK boatModel object
        FindWalkColObject(walkCol);     //find the correct walk col object inside of the WALK object
        option.walkColObject = walkColObject;   //note, this requires every potential boat part option to have a custom name, otherwise it won't work properly

        //add the part to the BoatCustomParts script
        BoatCustomParts customParts = boat.GetComponent<BoatCustomParts>();
        if (customParts == null)
        {
            Debug.LogError("BoatCustomParts script not found on the boat prefab");
            return;
        }
        if (isNewPart)
        {   //need to add this as a new part to the list
            BoatPart newPart = new BoatPart {
                activeOption = 0,
                category = category,
                partOptions = new List<BoatPartOption> { option }
            };

            LogGreen("newPart.activeOption: " + newPart.activeOption + "newPart.category: " + newPart.category + "newPart.partOptions.Count: " + newPart.partOptions.Count);
            customParts.availableParts.Add(newPart);
            
        }
        else
        {
            //need to add this as an option to an existing part
            if (partIndex > customParts.availableParts.Count)
            {
                Debug.LogError("Could not find a part with this partIndex");
                return;
            }
            customParts.availableParts[partIndex].partOptions.Add(option);
        }

        //save the changes (set the boat prefab dirty)
        EditorUtility.SetDirty(boat);

        //self destruct if needed (this bit should be last)
        if (selfDestruct)
        {
            Debug.Log("<color=orange>Self destructing CreateBoatPart script. DON'T PANIC!!! Part Title won't work for this!</orange>");
            DestroyImmediate(this);
        }
        else
        {
            LogGreen("Did not self destruct CreateBoatPart script");
        }
    }
    private void FindBoatModel(Transform tra)
    {   // finds the boatModel object going up recursively in the hierarchy
        if (tra.GetComponent<BoatHorizon>() != null)
        {   
            boatModel = tra;
            return;
        }
        else
        {
            if (tra.parent != boat.transform) FindBoatModel(tra.parent);
            else Debug.LogError("boatModel not found, an object shoudl have the BoatHorizon script for this to work"); return;
        }
    }
    private void FindWalkColObject(Transform tra)
    {   //recursively searches for the right walk col object
        foreach (Transform child in tra)
        {
            if (child.name == name)
            {
                walkColObject = child.gameObject;
                LogGreen("Found the corresponding walk col object for " + name);

                return;
            }
            else
            {
                FindWalkColObject(child);
            }
        }
    }
    private void LogGreen(string str)
    {
        Debug.Log($"<color=green>{str}</color>");
    }
}
