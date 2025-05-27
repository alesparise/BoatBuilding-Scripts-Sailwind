#if (UNITY_EDITOR)
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

public class CreateChildOption : MonoBehaviour
{
    public GameObject[] childOptions;
    public Transform walkCol;

    [Header("ScriptOption")]
    [Tooltip("Automatically destroy this script once done")]
    public bool selfDestruct;

    public void Reset()
    {
        childOptions = new GameObject[0];
        Transform root = PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot.transform;
        BoatHorizon boatModel = root.GetComponentInChildren<BoatHorizon>();

        if (boatModel != null)
        {
            walkCol = root.Find("WALK " + boatModel.name);
        }
        else
        {
            Debug.LogWarning("BoatHorizon not found, needed to automatically assign the Walk Col");
        }
    }
    public void DoCreate()
    {   //Populate the boat part option childOption array with both the object and the walk col correspondent object

        BoatPartOption option = GetComponent<BoatPartOption>();
        if (option == null)
        {
            Debug.LogError("No BoatPartOption on this part");
            return;
        }

        option.childOptions = new GameObject[childOptions.Length * 2];
        int j = 0;
        for (int i = 0; i < option.childOptions.Length; i++)
        {
            option.childOptions[i] = childOptions[j];
            i++;
            option.childOptions[i] = CreateBoatPart.FindWalkColObject(childOptions[j].transform, walkCol);
            j++;
        }
        
        //keep last
        if (selfDestruct)
        {
            Debug.LogError("CreateChildOption script removed");
            DestroyImmediate(this);
        }
        else
        {
            LogGreen("kept the CreateChildOption script");
        }
    }

    private void LogGreen(string str)
    {
        Debug.Log("<color=green>BoatBuilder: " + str + "</color>");
    }
}
#endif