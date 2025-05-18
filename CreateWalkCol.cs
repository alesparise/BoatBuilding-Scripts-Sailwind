using UnityEditor;
using UnityEngine;

public class CreateWalkCol : MonoBehaviour
{
    [Header("Walk Col Position")]
    [SerializeField]
    [Tooltip("The position the walk col will be created at.+")]
    private Vector3 pos;
    [Header("Options")]
    [SerializeField]
    [Tooltip("Automatically adds mesh colliders to every child in the walk col. Only use for quick tests.")]
    private bool automaticMeshColliders;
    [SerializeField]
    [Tooltip("Automatically removes this script once used.")]
    private bool selfDestroy;

    private int meshColliderCount = 0; //number of mesh colliders added to the walk col
    public void DoCreate()
    {
        Transform prefab = transform.parent;  //the prefab's transform

        GameObject walkCol = Instantiate(gameObject, prefab);
        walkCol.transform.localPosition = pos;
        walkCol.name = "WALK " + gameObject.name;

        Undo.RegisterCreatedObjectUndo(walkCol, "Create Walk Col"); //can use ctrl+z to undo

        Debug.Log("<color=green>BoatBuilder: Instantiated Walk Col</color>");

        //remove all unnecessary components
        RemoveComponents(walkCol);

        //add mesh colliders to all children
        if (automaticMeshColliders)
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

        //remove the embark_col and water_mask objects entirely
        Transform walkTransform = walkCol.transform;
        Transform embark_col = walkTransform.Find("embark_col");
        Transform water_mask = walkTransform.Find("water_mask");
        if (embark_col == null)
        {
            Debug.LogError("BoatBuilder: Could not find embark_col object");
        }
        else
        {
            DestroyImmediate(embark_col.gameObject);
            Debug.Log("<color=green>Removed embark_col object</color>");
        }
        if (water_mask == null)
        {
            Debug.LogError("BoatBuilder: Could not find water_mask object");
        }
        else
        {
            DestroyImmediate(water_mask.gameObject);
            Debug.Log("<color=green>Removed water_mask object</color>");
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
