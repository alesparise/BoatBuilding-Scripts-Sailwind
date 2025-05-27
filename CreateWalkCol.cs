#if (UNITY_EDITOR)
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

public class CreateWalkCol : MonoBehaviour
{
    [Header("Walk Col Position")]
    [Tooltip("The position the walk col will be created at.+")]
    public Vector3 position;
    [Header("Script Options")]
    [Tooltip("Automatically adds mesh colliders to every child in the walk col. Only use for quick tests.")]
    public bool autoMeshColliders;
    [Tooltip("Automatically removes this script once used.")]
    public bool selfDestroy;
    [Tooltip("Destroy unnecessary objects from the walk col")]
    public bool removeObjects = true;
    [HideInInspector]
    [Tooltip("The names of the objects that should be removed from the walk col. This does not include winches and blocks")]
    public string[] objectsToRemove = { "embark_col", "water_mask", "hull_push", "water_damage", "splash_mask" }; //add or change this to remove other objects. Does not include winches and blocks

    private List<BoatPartOption> partOptions = new List<BoatPartOption>();
    private List<Mast> masts = new List<Mast>();
    private List<GameObject> winches = new List<GameObject>();

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

        Transform prefab = boat.transform;  //the prefab's transform

        GameObject walkCol = Instantiate(gameObject, prefab);
        walkCol.transform.localPosition = position;
        walkCol.name = "WALK " + name;

        Undo.RegisterCreatedObjectUndo(walkCol, "Create Walk Col"); //can use ctrl+z to undo

        Debug.Log("<color=green>BoatBuilder: Instantiated Walk Col</color>");


        //remove all unnecessary components
        if (removeObjects)
        {   //remove all winches and blocks from the walk col
            CollectWinches(walkCol);
            foreach (GameObject winch in winches)
            {
                DestroyImmediate(winch);
            }
            Debug.Log("<color=green>BoatBuilder: Removed <b>" + removedWinches + "</b> winches and blocks from the walkCol</color>");
        }
        RemoveComponents(walkCol);

        //set all BoatPartOptions and Mast references to the WALK col object
        TraverseHierarchy(transform);
        SetWalkCols(walkCol.transform);

        //add mesh colliders to all children
        if (autoMeshColliders)
        {
            meshColliderCount = 0;
            AddMeshColliders(walkCol);
            Debug.LogWarning("<color=orange>BoatBuilder: Added <b>" + meshColliderCount + "</b> mesh colliders to the walkCol</color>");
            Debug.LogWarning("<color=orange>NOTE: this is not ideal, you likely don't need this many!!!</color>");
        }
        else
        {
            Debug.Log("<color=green>BoatBuilder: Did not add any mesh colliders</color>");
        }

        //set the walk col reference in the right place
        BoatRefs boatRefs = prefab.GetComponent<BoatRefs>();
        if (boatRefs == null)
        {
            Debug.LogError("BoatBuilder: Could not find BoatRefs component on the prefab");
        }
        else
        {
            boatRefs.walkCol = walkCol.transform;
            Debug.Log("<color=green>BoatBuilder: Set walkCol reference in BoatRefs</color>");
        }
        BoatEmbarkCollider bec = GetComponentInChildren<BoatEmbarkCollider>();
        if (bec == null)
        {
            Debug.LogError("BoatBuilder: Could not find BoatEmbarkCollider component inside of the prefab");
        }
        else
        {
            bec.walkCollider = walkCol.transform;
            Debug.Log("<color=green>BoatBuilder: Set walkCol reference in BoatEmbarkCollider</color>");
        }

        //set the walk col to the right layer
        SetLayer(walkCol);
        Debug.Log("<color=green>BoatBuilder: Set walkCol layer to WalkCols (8)</color>");

        //remove this script
        if (selfDestroy)
        {
            Debug.LogWarning("<color=orange>BoatBuilder: Self destroying! (DON'T PANIC!)</color>");
            DestroyImmediate(this);
        }
        else
        {
            Debug.Log("<color=green>BoatBuilder: Kept self</color>");
        }
    }
    private void SetWalkCols(Transform tra)
    {

        foreach (BoatPartOption option in partOptions)
        {
            option.walkColObject = CreateBoatPart.FindWalkColObject(option.transform, tra);
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
        if (mast != null) masts.Add(mast); //Debug.Log("Added mast: " + mast.name + " to list");

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
        foreach (Component component in components)
        {
            if (component is Transform || component is MeshRenderer || component is MeshFilter)
            {
                continue;
            }
            else
            {
                i++;
                Debug.Log("BoatBuilder: Removing " + component.GetType() + " from the walkCol");
                DestroyImmediate(component);
            }
        }
        Debug.Log("<color=green>BoatBuilder: Removed <b>" + i + "</b> components from the walkCol</color>");

        //remove the objects mentioned in the objectsToRemove array
        if (!removeObjects) return;
        Transform walkTransform = walkCol.transform;
        foreach (string obj in objectsToRemove)
        {
            Transform toRemove = walkTransform.Find(obj);
            if (toRemove == null)
            {
                Debug.LogError("<color=red>BoatBuilder: Could not find " + obj + " object</color>");
            }
            else
            {
                DestroyImmediate(toRemove.gameObject);
                Debug.Log("<color=green>Removed " + obj + " object</color>");
            }
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