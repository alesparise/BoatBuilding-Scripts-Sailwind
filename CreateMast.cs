using System.IO;
using UnityEngine;
using UnityEditor;

public class CreateMast : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("The prefab for the winch")]
    public GameObject winchPrefab;
    [Tooltip("The prefab for the block")]
    public GameObject blockPrefab;
    [Header("Mast Options")]
    [Tooltip("The maximum number of sails this mast allows")]
    public int maxSails;
    //[Range(0f,50f)]
    [Tooltip("The usable height of the mast")]
    [Range(0f, 40f)]
    public float mastHeight;
    [Tooltip("Only allow staysails")]
    public bool onlyStaysails;
    [Tooltip("Only allow squares (bowsprit)")]
    public bool onlySquareSails;
    [Tooltip("The radius of the collider, make it smaller if it's too big compared to the mast")]
    public float colliderRadius = 0.25f;

    [Header("Script Options")]
    [Tooltip("Destroy this script once it's done")]
    public bool selfDestruct;

    private Mast mast;

    private static int mastCount = 0;
    private Transform walkCol;

    public void DoCreate()
    {
        if (!AllGood()) return;
        
        //add Mast component and Collider, set collider reference, rigidbody reference, walk col reference
        mast = gameObject.AddComponent<Mast>();

        //add the capsule collider and set it up
        CapsuleCollider cc = gameObject.AddComponent<CapsuleCollider>();
        cc.direction = 2;
        cc.radius = 0.25f;
        cc.height = mastHeight + 2f;
        cc.center = new Vector3(0f,0f, - (mastHeight / 2f));
        mast.mastCols = new CapsuleCollider[1];
        mast.mastCols[0] = cc;

        //rigidbody reference
        mast.shipRigidbody = transform.parent.GetComponentInParent<Rigidbody>() ??  //this is the Null Coalescing operator: if the first is null it will use the second
                      transform.parent.parent.GetComponentInParent<Rigidbody>();  //this should work because only the parent has a rigidbody. Won't work if the mast is nested deeper inside the object
        if (mast.shipRigidbody == null)
        {
            Debug.LogError("Could not find ship rigidbody");
            return;
        }

        //Add the walk col reference
        mast.walkColMast = walkCol.Find(gameObject.name);

        LogGreen("Initialized Mast component for " + gameObject.name);

        //configure options
        mast.onlyStaysails = onlyStaysails;
        mast.onlySquareSails = onlySquareSails;
        mast.maxSails = maxSails;
        mast.mastHeight = mastHeight;
        mast.orderIndex = mastCount;
        mastCount++;
        LogGreen("Configured Mast variables");

        //initialize the mast with the correct winch arrays sizes
        if (!onlyStaysails)
        {   //normal mast
            mast.leftAngleWinch = new GPButtonRopeWinch[1];
            mast.rightAngleWinch = new GPButtonRopeWinch[1];
            if (!onlySquareSails)
            {   //no need for the mid angle winch on the bowsprit
                mast.midAngleWinch = new GPButtonRopeWinch[maxSails];
            }
        }
        else
        {   //a pair for each staysail
            mast.leftAngleWinch = new GPButtonRopeWinch[maxSails];
            mast.rightAngleWinch = new GPButtonRopeWinch[maxSails];
        }
        mast.reefWinch = new GPButtonRopeWinch[maxSails];   //always one for each sail
        mast.mastReefAttExtension = new Transform[maxSails]; //always one for each sail

        //create the appropriate number of winches and blocks
        CreateWinchesAndBlocks();
        //CreateBlocks();


        //self destroy this script (this should be last)
        if (selfDestruct)
        {
            Debug.LogWarning("Self destructing Create Mast script on " + gameObject.name);
            DestroyImmediate(this);
        }
        else
        {
            LogGreen("Did not self destruct");
        }
    }
    private void CreateWinchesAndBlocks()
    {   //we need: 
        //1 winch for the left, one for the right
        //1 winch for each sail (reef)
        //1 winch for each sail (mid sheet) (not on the bowsprit)
        //if it's for a staysail only mast we need a left and right winch for each mast
        Vector3 mastPos = transform.position;
        if (!onlyStaysails)
        {   //normal masts only need one winch for each side for squares
            mast.leftAngleWinch[0] = Instantiate(winchPrefab, mastPos + Vector3.left * 5f + Vector3.down * mastHeight, Quaternion.identity, transform).GetComponent<GPButtonRopeWinch>();
            mast.rightAngleWinch[0] = Instantiate(winchPrefab, mastPos + Vector3.right * 5f + Vector3.down * mastHeight, Quaternion.identity, transform).GetComponent<GPButtonRopeWinch>();
            mast.leftAngleWinch[0].transform.localEulerAngles = new Vector3(0f, 0f, -90f);
            mast.rightAngleWinch[0].transform.localEulerAngles = new Vector3(0f, 0f, 90f);
            mast.leftAngleWinch[0].name = "winch_left";
            mast.rightAngleWinch[0].name = "winch_right";
        }
        //add all the winches that are required for each sail
        for (int i = 0; i < maxSails; i++)
        {
            //reef
            mast.reefWinch[i] = Instantiate(winchPrefab, mastPos + Vector3.down * (mastHeight - i * 0.35f) + Vector3.back * 0.25f, Quaternion.identity, transform).GetComponent<GPButtonRopeWinch>();
            mast.reefWinch[i].name = "reef_winch_" + i;
            mast.reefWinch[i].transform.localEulerAngles = new Vector3(0f, -90f, 0f);
            //blocks
            mast.mastReefAttExtension[i] = Instantiate(blockPrefab, mastPos + Vector3.down * (i * 0.35f) + Vector3.back * 0.25f, Quaternion.identity, transform).transform;
            mast.mastReefAttExtension[i].name = "reef_block_" + i;
            mast.mastReefAttExtension[i].localEulerAngles = new Vector3(0f, 45f, 0f);
            mast.mastReefAttExtension[i].localScale = new Vector3(0.4f, 0.4f, 0.4f);

            //code for left right in stays and code for mid sheet
            if (!onlySquareSails && !onlyStaysails)
            {   //mid sheet only exists for non staysails and non bowsprit masts
                mast.midAngleWinch[i] = Instantiate(winchPrefab, mastPos + Vector3.down * mastHeight + Vector3.back * (3f + i * 0.35f), Quaternion.identity, transform).GetComponent<GPButtonRopeWinch>();
                mast.midAngleWinch[i].name = "mid_winch_" + i;
                mast.midAngleWinch[i].transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            }
            if (onlyStaysails)
            {   //have two winches for each staysail
                mast.leftAngleWinch[i] = Instantiate(winchPrefab, mastPos + Vector3.left * 5f + Vector3.down * mastHeight + Vector3.back * (i * 0.35f), Quaternion.identity, transform).GetComponent<GPButtonRopeWinch>();
                mast.rightAngleWinch[i] = Instantiate(winchPrefab, mastPos + Vector3.right * 5f + Vector3.down * mastHeight + Vector3.back * (i * 0.35f), Quaternion.identity, transform).GetComponent<GPButtonRopeWinch>();
                mast.leftAngleWinch[i].name = "winch_left_" + i;
                mast.rightAngleWinch[i].name = "winch_right_" + i;
                mast.leftAngleWinch[i].transform.localEulerAngles = new Vector3(0f, 0f, -90f);
                mast.rightAngleWinch[i].transform.localEulerAngles = new Vector3(0f, 0f, 90f);
            }
        }
    }
    private bool AllGood()
    {   //Checks if everything is set up correctly before running the script, also logs the errors
        if (mastHeight <= 0)
        {
            Debug.LogError("Mast height must be greater than 0");
            return false;
        }
        if (maxSails <= 0)
        {
            Debug.LogError("Max sails must be greater than 0");
            return false;
        }
        string walkName = "WALK " + transform.parent.name;
        walkCol = transform.parent.parent.Find(walkName);
        if (walkCol == null)
        {
            Debug.LogError("Could not find walk col " + walkName);
            return false;
        }
        if (GetComponent<Mast>() != null)
        {
            Debug.LogError("Mast component already exists on " + gameObject.name);
            return false;
        }
        if (GetComponent<CapsuleCollider>() != null)
        {
            Debug.LogError("Capsule collider already exists on " + gameObject.name);
            return false;
        }

        return true;
    }
    public void Reset()
    {
        string scriptPath = GetScriptFolderPath();
        string prefabPath = Path.Combine(Path.GetDirectoryName(scriptPath), "Prefabs");
        winchPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(prefabPath, "Winch.prefab"));
        blockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(prefabPath, "Block.prefab"));
        string missingPrefabs =
            (winchPrefab == null ? "Winch prefab, " : "") +
            (blockPrefab == null ? "Block prefab, " : "");

        if (!string.IsNullOrEmpty(missingPrefabs))
        {
            Debug.LogError($"{missingPrefabs.TrimEnd(',', ' ')} not found. Should be in the {prefabPath} folder");
        }
        else
        {
            Debug.Log("<color=green>Winch and block prefabs loaded</color>");
        }
    }
    public void OnDrawGizmos()
    {
        Vector3 size = new Vector3(0.75f, 0.01f, 0.75f);

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 1f);
        Gizmos.DrawCube(transform.position, size);

        Gizmos.color = new Color(0.2f, 1f, 0.2f, 1f);
        Gizmos.DrawCube(new Vector3 (transform.position.x, transform.position.y - mastHeight, transform.position.z), size);
    }
    private string GetScriptFolderPath()
    {
        string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(this));
        return Path.GetDirectoryName(scriptPath);
    }
    private void LogGreen(string str)
    {
        Debug.Log($"<color=green>{str}</color>");
    }
}
