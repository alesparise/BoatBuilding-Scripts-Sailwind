#if (UNITY_EDITOR)
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

/// <summary>
/// NOTE: This script goes on the object that would have the BoatHorizon component (the boat model)
/// </summary>
public class CreateWalkCol : MonoBehaviour
{
    [Header("Walk Col Position")]
    [Tooltip("The position the walk col will be created at")]
    public Vector3 position;
    [Header("Script Options")]
    [Tooltip("Automatically adds mesh colliders to every child in the walk col. Only use for quick tests")]
    public bool autoMeshColliders;
    [Tooltip("Automatically removes this script once used")]
    public bool selfDestroy;
    [Tooltip("Destroy unnecessary objects from the walk col")]
    public bool removeObjects = true;
    [HideInInspector]
    [Tooltip("The names of the objects that should be removed from the walk col. This does not include winches and blocks")]
    public string[] objectsToRemove = { "embark_col", "water_mask", "hull_push", "water_damage", "splash_mask" }; //add or change this to remove other objects. Does not include winches and blocks

    private List<BoatPartOption> partOptions = new List<BoatPartOption>();
    private List<Mast> masts = new List<Mast>();
    private List<GameObject> winches = new List<GameObject>();
    private List<string> hadColliders = new List<string>();    //we store the names of the objects that have walk colliders when we update the walk col, so that we can automatically reassign them eventually

    private GameObject boat;    //prefab root object

    private int removedWinches = 0;

    private int meshColliderCount = 0; //number of mesh colliders added to the walk col
    public void DoCreate()
    {
        boat = PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot;
        //reset
        removedWinches = 0;
        partOptions.Clear();
        masts.Clear();
        winches.Clear();
        hadColliders.Clear();

        Transform oldWalk = boat.transform.Find("WALK " + name);
        if (oldWalk != null)
        {   //if the walk col already exists, we "update" it
            Debug.LogWarning("<color=orange>CreateWalkCol: Walk Col already exists, updating it!</color>");
            Collider[] colliders = oldWalk.GetComponentsInChildren<Collider>(true);
            foreach (Collider collider in colliders)
            {   //add all colliders to the list of objects names that have colliders
               hadColliders.Add(collider.name);
            }
            DestroyImmediate(oldWalk.gameObject);
            Debug.LogWarning("<color=orange>CreateWalkCol: Deleted old walk col, recreating...</color>");
        }

        Transform prefab = boat.transform;

        GameObject walkCol = Instantiate(gameObject, prefab);
        walkCol.transform.localPosition = position;
        walkCol.transform.localRotation = Quaternion.identity;
        walkCol.name = "WALK " + name;

        Undo.RegisterCreatedObjectUndo(walkCol, "Create Walk Col"); //can use ctrl+z to undo

        Debug.Log("<color=green>CreateWalkCol: Instantiated Walk Col</color>");

        //remove all unnecessary components
        if (removeObjects)
        {   //remove all winches and blocks from the walk col
            CollectWinches(walkCol);
            foreach (GameObject winch in winches)
            {
                DestroyImmediate(winch);
            }
            Debug.Log("<color=green>CreateWalkCol: Removed <b>" + removedWinches + "</b> winches and blocks from the walkCol</color>");
        }
        RemoveComponents(walkCol);
        RemoveComponents(walkCol);
        RemoveComponents(walkCol); //running it trice to delete all components that were skipped because of dependencies

        //set all BoatPartOptions and Mast references to the WALK col object
        //restore the MeshColliders if we are updating a walk col
        TraverseHierarchy(transform);
        SetWalkCols(walkCol.transform);
        if (hadColliders.Count != 0) RestoreColliders(walkCol.transform);

        //add mesh colliders to all children
        if (autoMeshColliders && hadColliders.Count == 0)
        {
            meshColliderCount = 0;
            AddMeshColliders(walkCol);
            Debug.LogWarning("<color=orange>CreateWalkCol: Added <b>" + meshColliderCount + "</b> mesh colliders to the walkCol</color>");
            Debug.LogWarning("<color=orange>NOTE: this is not ideal, you likely don't need this many!!!</color>");
        }
        else
        {
            Debug.Log("<color=green>CreateWalkCol: Did not add any mesh colliders</color>");
        }

        //set the walk col reference in the right place
        BoatRefs boatRefs = prefab.GetComponent<BoatRefs>();
        if (boatRefs == null)
        {
            Debug.LogError("CreateWalkCol: Could not find BoatRefs component on the prefab");
        }
        else
        {
            boatRefs.walkCol = walkCol.transform;
            Debug.Log("<color=green>CreateWalkCol: Set walkCol reference in BoatRefs</color>");
        }
        BoatEmbarkCollider bec = GetComponentInChildren<BoatEmbarkCollider>();
        if (bec == null)
        {
            Debug.LogError("CreateWalkCol: Could not find BoatEmbarkCollider component inside of the prefab");
        }
        else
        {
            bec.walkCollider = walkCol.transform;
            Debug.Log("<color=green>CreateWalkCol: Set walkCol reference in BoatEmbarkCollider</color>");
        }

        //set the walk col to the right layer
        SetLayer(walkCol);
        Debug.Log("<color=green>CreateWalkCol: Set walkCol layer to WalkCols (8)</color>");

        //remove this script
        if (selfDestroy)
        {
            Debug.LogWarning("<color=orange>CreateWalkCol: Self destroying! (DON'T PANIC!)</color>");
            DestroyImmediate(this);
        }
        else
        {
            Debug.Log("<color=green>CreateWalkCol: Kept self</color>");
        }
    }
    private void SetWalkCols(Transform tra)
    {
        foreach (BoatPartOption option in partOptions)
        {
            option.walkColObject = CreateBoatPart.FindWalkColObject(option.transform, tra);

            if (option.childOptions != null && option.childOptions.Length > 0)
            {   //if this part option has child options, we set the walk col object for them as well
                GameObject[] children = new GameObject[option.childOptions.Length / 2];
                int j = 0;
                for (int i = 0; i < option.childOptions.Length; i++)
                {   // condensate the child options into a new array, skipping nulls
                    if (option.childOptions[i] == null) continue; //skip if the child is null
                    children[j] = option.childOptions[i];
                    j++;
                }
                j = 0;
                option.childOptions = new GameObject[children.Length * 2];
                for (int i = 0; i < option.childOptions.Length; i++)
                {   //set the walk col object for each child option
                    option.childOptions[i] = children[j];
                    i++;
                    option.childOptions[i] = CreateBoatPart.FindWalkColObject(option.childOptions[i].transform, tra);
                    j++;
                }
                Debug.Log("Set " + option.childOptions.Length + " child options for " + option.name);
            }
            Debug.Log("Set walk col object for " + option.name);
        }
        foreach (Mast mast in masts)
        {
            mast.walkColMast = CreateBoatPart.FindWalkColObject(mast.transform, tra).transform;
            Debug.Log("Set walk col object for " + mast.name);
        }
    }
    private void TraverseHierarchy(Transform tra)
    {
        Mast mast = tra.GetComponent<Mast>();
        if (mast != null) masts.Add(mast);

        BoatPartOption partOption = tra.GetComponent<BoatPartOption>();
        if (partOption != null) partOptions.Add(partOption);
        
        foreach (Transform child in tra.transform)
        {
            TraverseHierarchy(child);
        }
    }
    private void RemoveComponents(GameObject walkCol)
    {   //remove all unnecessary components from the walk col
        int i = 0;
        Component[] components = walkCol.GetComponentsInChildren<Component>(true);

        for (int j = 0; j < components.Length; j++)
        {   //go through all components and remove the ones that are not needed
            Component component = components[j];
            if (component is Transform || component is MeshRenderer || component is MeshFilter) continue; //keep these components
            DestroyImmediate(component);
            i++;
        }
        Debug.Log("<color=green>CreateWalkCol: Removed <b>" + i + "</b> components from the walkCol</color>");

        //remove the objects mentioned in the objectsToRemove array
        if (!removeObjects) return;
        Transform walkTransform = walkCol.transform;
        foreach (string obj in objectsToRemove)
        {
            Transform toRemove = walkTransform.Find(obj);
            if (toRemove == null)
            {
                Debug.LogError("<color=red>CreateWalkCol: Could not find " + obj + " object</color>");
            }
            else
            {
                DestroyImmediate(toRemove.gameObject);
                Debug.Log("<color=green>Removed " + obj + " object</color>");
            }
        }
    }
    private void RestoreColliders(Transform tra)
    {   //restore the colliders that were removed from the walk col

        if (hadColliders.Contains(tra.name))
        {
            MeshCollider col = tra.gameObject.AddComponent<MeshCollider>();
            col.convex = false;
        }
        foreach (Transform child in tra.transform)
        {   
            RestoreColliders(child);
        }
    }
    private void CollectWinches(GameObject obj)
    {   //remove all winches from the walk col
        
        if (obj.name.Contains("winch_") || obj.name.Contains("block_"))
        {
            winches.Add(obj);
            removedWinches++;
        }
        else
        {
            foreach (Transform child in obj.transform)
            {   //go through all children recursively
                CollectWinches(child.gameObject);
            }
        }
    }
    private void AddMeshColliders(GameObject parent)
    {   //add mesh colliders recursively
        if (parent.GetComponent<MeshCollider>() == null && parent.GetComponent<MeshFilter>() != null)
        {   //if this DOES NOT have a mesh collider and DOES HAVE a mesh filter, we add a mesh collider
            parent.AddComponent<MeshCollider>();
            meshColliderCount++;
        }
        foreach (Transform child in parent.transform)
        {   //call this very same thing on all children recursively
            AddMeshColliders(child.gameObject);
        }
    }
    private void SetLayer(GameObject parent)
    {
        parent.layer = 8;
        foreach (Transform child in parent.transform)
        {
            SetLayer(child.gameObject);
        }
    }
}
#endif