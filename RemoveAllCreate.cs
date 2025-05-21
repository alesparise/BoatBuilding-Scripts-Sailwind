using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

public class RemoveAllCreate : MonoBehaviour
{
    public void DoRemove()
    {
        GameObject root = PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot;

        CreateBoatPart[] cbp = root.GetComponentsInChildren<CreateBoatPart>(true);
        CreateMast[] cm = root.GetComponentsInChildren<CreateMast>(true);
        CreateWalkCol[] cwc = root.GetComponentsInChildren<CreateWalkCol>(true);

        foreach (CreateBoatPart part in cbp)
        {   //remove CreateBoatPart components
            DestroyImmediate(part);
        }
        foreach (CreateMast mast in cm)
        {   //remove CreateMast components
            DestroyImmediate(mast);
        }
        foreach (CreateWalkCol walkCol in cwc)
        {   //remove CreateWalkCol components
            DestroyImmediate(walkCol);
        }
        //remove this script
        Debug.LogWarning("<color=orange>Removed all BoatBuilding components from " + root.name + "</color>");
        DestroyImmediate(this);
    }
}
