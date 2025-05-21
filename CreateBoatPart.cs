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
    [Tooltip("The part category. 0 = masts, 1 = other (shrouds), 2 = stays")]
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

    [HideInInspector]
    public GameObject boat;
    private Transform boatModel;

    public void Reset()
    {   //add CreateMast component when the script is attached if it has mast or stay in the name, also adjust category
        boat = PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot;   //this is the prefab root object AKA the BOAT object
        if (name.Contains("mast"))
        {   
            category = 0;
            if (gameObject.GetComponent<Mast>() == null && gameObject.GetComponent<CreateMast>() == null) 
                gameObject.AddComponent<CreateMast>();
        }
        else if (name.Contains("Stay"))
        {
            category = 2;
            if (gameObject.GetComponent<Mast>() == null && gameObject.GetComponent<CreateMast>() == null)
                gameObject.AddComponent<CreateMast>();
        }
        else if (name.Contains("shrouds"))
        {
            category = 1;
        }
    }
    public void DoCreate()
    {
        if (GetComponent<BoatPartOption>() != null)
        {
            Debug.LogError("This object already has a BoatPartOption component");
            return;
        }
        //add the boat option and set it up
        BoatPartOption option = gameObject.AddComponent<BoatPartOption>();
        option.optionName = optionName;
        option.basePrice = basePrice;
        option.installCost = installCost;
        option.mass = mass;
        //set the walk col reference
        FindBoatModel(transform.parent);    //find the boatModel object
        Transform walkCol = boat.transform.Find("WALK " + boatModel.name);  //find the WALK boatModel object
        if (walkCol == null)
        {
            Debug.LogError("Could not find the WALK col object in the prefab");
            return;
        }
        option.walkColObject = FindWalkColObject(transform, walkCol);     //find the correct walk col object inside of the WALK object

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
            customParts.availableParts.Add(newPart);
        }
        else
        {   //need to add this as an option to an existing part
            if (partIndex > customParts.availableParts.Count)
            {
                Debug.LogError("Could not find a part with this partIndex");
                return;
            }
            customParts.availableParts[partIndex].partOptions.Add(option);
        }
        if (canBeDisabled && EmptyPartCheck(customParts.availableParts))
        {   //if the part can be disabled, and there is no "no_part" existing already, add an empty option to the list
            GameObject empty = new GameObject();   //create a new game object
            GameObject noPart = Instantiate(empty, transform.parent);
            noPart.name = "no_" + name;
            BoatPartOption noOption = noPart.AddComponent<BoatPartOption>();
            noOption.optionName = "No " + optionName;
            noOption.basePrice = 0;
            noOption.installCost = 0;
            noOption.mass = 0;

            //create the empty walk col object in the right place
            Transform walkObjParent = option.walkColObject.transform.parent;
            GameObject noPartWalk = Instantiate(empty, walkObjParent);
            noPartWalk.name = noPart.name;
            noOption.walkColObject = noPartWalk;

            DestroyImmediate(empty);    //destroy the empty object since it creates one in the scene too for some reason

            //add the part to the list
            customParts.availableParts[customParts.availableParts.Count - 1].partOptions.Add(noOption);
        }

        //save the changes (set the boat prefab dirty)
        EditorUtility.SetDirty(boat);

        //self destruct if needed (this bit should be last)
        if (selfDestruct)
        {
            Debug.Log("<color=orange>Self destructing CreateBoatPart script. DON'T PANIC!!! \nHowever, Part Title won't work for this part!</orange>");
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
    public static GameObject FindWalkColObject(Transform target, Transform traversedTransform)
    {   //recursively searches for the right walk col object
        foreach (Transform child in traversedTransform)
        {
            if (child.name == target.name)
            {
                Debug.Log("Found the corresponding walk col object for " + target.name);
                return child.gameObject;
            }
            GameObject found = FindWalkColObject(target, child);
            if (found != null) return found;
        }

        return null;
    }
    private bool EmptyPartCheck(List<BoatPart> availableParts)
    {
        int lastPart = availableParts.Count - 1;
        List<BoatPartOption> partOptions = availableParts[lastPart].partOptions;
        foreach (BoatPartOption option in partOptions)
        {
            if (option.optionName.StartsWith("no_"))
            {
                Debug.Log("<color=orange>BoatBuilder: This part already has a no option</color>");
                return false;
            }
        }
        return true;
    }
    private void LogGreen(string str)
    {
        Debug.Log($"<color=green>{str}</color>");
    }
}
