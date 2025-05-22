using System.IO;
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;

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
    [Tooltip("The usable height of the mast")]
    public float mastHeight;
    [Tooltip("Only allow staysails")]
    public bool onlyStaysails;
    [Tooltip("Only allow squares (bowsprit)")]
    public bool bowsprit;

    [Header("Script Options")]
    [Tooltip("Destroy this script once it's done")]
    public bool selfDestruct;

    private Mast mast;

    private Transform walkCol;

    [HideInInspector]
    public float maxHeight;
    private float radius;

    private static int mastCount = 0;


    public void DoCreate()
    {
        if (!AllGood()) return;

        Undo.SetCurrentGroupName("Set up mast " + name);
        int group = Undo.GetCurrentGroup();
        //add Mast component and Collider, set collider reference, rigidbody reference, walk col reference
        mast = Undo.AddComponent<Mast>(gameObject);

        //add the capsule collider and set it up
        CapsuleCollider cc = Undo.AddComponent<CapsuleCollider>(gameObject);
        cc.direction = 2;
        cc.radius = radius * 1.2f;  //keeping a bit of margin to avoid sails clipping
        cc.height = mastHeight + 2f;
        cc.center = new Vector3(0f,0f, - (mastHeight / 2f));
        mast.mastCols = new CapsuleCollider[1];
        mast.mastCols[0] = cc;

        //rigidbody reference
        mast.shipRigidbody = transform.parent.GetComponentInParent<Rigidbody>() ??  //this is the Null Coalescing operator: if the first is null it will use the second
                      transform.parent.parent.GetComponentInParent<Rigidbody>();  //this should work because only the parent has a rigidbody. Won't work if the mast is nested deeper inside the object
        if (mast.shipRigidbody == null)
        {
            LogRed("Could not find ship rigidbody");
            return;
        }

        //Add the walk col reference
        mast.walkColMast = walkCol.Find(gameObject.name);

        LogGreen("Initialized Mast component for " + gameObject.name);

        //configure options
        mast.onlyStaysails = onlyStaysails;
        mast.onlySquareSails = bowsprit;
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
            if (!bowsprit)
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
        mast.mastReefAtt = new Transform[maxSails]; //always one for each sail

        //create the appropriate number of winches and blocks
        CreateWinchesAndBlocks();

        Undo.CollapseUndoOperations(group);

        //save the changes in the prefab (setting it as dirty seems to do the trick)
        GameObject go = PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot;
        EditorUtility.SetDirty(go);
        LogGreen("Saved changes to the prefab");

        //self destroy this script (this should be last)
        if (selfDestruct)
        {
            LogOrange("Self destructing Create Mast script on " + gameObject.name);
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
        Vector3 left = -transform.up;
        Vector3 right = transform.up;
        Vector3 down = -transform.forward;
        Vector3 back = -transform.right;

        if (!onlyStaysails)
        {   //normal masts only need one winch for each side for squares
            mast.leftAngleWinch[0] = Instantiate(winchPrefab, mastPos + left * 5f + down * mastHeight, Quaternion.identity, transform).GetComponent<GPButtonRopeWinch>();
            mast.rightAngleWinch[0] = Instantiate(winchPrefab, mastPos + right * 5f + down * mastHeight, Quaternion.identity, transform).GetComponent<GPButtonRopeWinch>();
            mast.leftAngleWinch[0].transform.localEulerAngles = new Vector3(0f, 0f, -90f);
            mast.rightAngleWinch[0].transform.localEulerAngles = new Vector3(0f, 0f, 90f);
            mast.leftAngleWinch[0].name = "winch_left";
            mast.rightAngleWinch[0].name = "winch_right";
            Undo.RegisterCreatedObjectUndo(mast.leftAngleWinch[0].gameObject, "Create winch for mast " + gameObject.name);
            Undo.RegisterCreatedObjectUndo(mast.rightAngleWinch[0].gameObject, "Create winch for mast " + gameObject.name);
        }
        //add all the winches that are required for each sail
        for (int i = 0; i < maxSails; i++)
        {
            //reef
            mast.reefWinch[i] = Instantiate(winchPrefab, mastPos + down * (mastHeight - i * 0.35f) + back * radius, Quaternion.identity, transform).GetComponent<GPButtonRopeWinch>();
            mast.reefWinch[i].name = "reef_winch_" + i;
            mast.reefWinch[i].transform.localEulerAngles = new Vector3(0f, -90f, 0f);
            Undo.RegisterCreatedObjectUndo(mast.reefWinch[i].gameObject, "Create winch for mast " + gameObject.name);

            //blocks
            mast.mastReefAtt[i] = Instantiate(blockPrefab, mastPos + down * (-0.5f -i * 0.25f) + back * (radius + 0.1f), Quaternion.identity, transform).transform;
            mast.mastReefAtt[i].name = "reef_block_" + i;
            mast.mastReefAtt[i].localEulerAngles = new Vector3(0f, 45f, 0f);
            mast.mastReefAtt[i].localScale = new Vector3(0.4f, 0.4f, 0.4f);
            Undo.RegisterCreatedObjectUndo(mast.mastReefAtt[i].gameObject, "Create block for mast " + gameObject.name);

            //code for left right in stays and code for mid sheet
            if (!bowsprit && !onlyStaysails)
            {   //mid sheet only exists for non staysails and non bowsprit masts
                mast.midAngleWinch[i] = Instantiate(winchPrefab, mastPos + down * mastHeight + back * (3f + i * 0.35f), Quaternion.identity, transform).GetComponent<GPButtonRopeWinch>();
                mast.midAngleWinch[i].name = "mid_winch_" + i;
                mast.midAngleWinch[i].transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                Undo.RegisterCreatedObjectUndo(mast.midAngleWinch[i].gameObject, "Create winch for mast " + gameObject.name);
            }
            if (onlyStaysails)
            {   //have two winches for each staysail
                mast.leftAngleWinch[i] = Instantiate(winchPrefab, mastPos + left * 5f + down * mastHeight + back * (i * 0.35f), Quaternion.identity, transform).GetComponent<GPButtonRopeWinch>();
                mast.rightAngleWinch[i] = Instantiate(winchPrefab, mastPos + right * 5f + down * mastHeight + back * (i * 0.35f), Quaternion.identity, transform).GetComponent<GPButtonRopeWinch>();
                mast.leftAngleWinch[i].name = "winch_left_" + i;
                mast.rightAngleWinch[i].name = "winch_right_" + i;
                mast.leftAngleWinch[i].transform.localEulerAngles = new Vector3(0f, 0f, -90f);
                mast.rightAngleWinch[i].transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                Undo.RegisterCreatedObjectUndo(mast.leftAngleWinch[i].gameObject, "Create winch for mast " + gameObject.name);
                Undo.RegisterCreatedObjectUndo(mast.rightAngleWinch[i].gameObject, "Create winch for mast " + gameObject.name);
            }
        }
    }
    private bool AllGood()
    {   //Checks if everything is set up correctly before running the script, also logs the errors
        if (mastHeight <= 0)
        {
            LogRed("Mast height must be greater than 0");
            return false;
        }
        if (maxSails <= 0)
        {
            LogRed("Max sails must be greater than 0");
            return false;
        }
        string walkName = "WALK " + transform.parent.name;
        walkCol = transform.parent.parent.Find(walkName);
        if (walkCol == null)
        {
            LogRed("Could not find walk col " + walkName);
            return false;
        }
        if (GetComponent<Mast>() != null)
        {
            LogRed("Mast component already exists on " + gameObject.name);
            return false;
        }
        if (GetComponent<CapsuleCollider>() != null)
        {
            LogRed("Capsule collider already exists on " + gameObject.name);
            return false;
        }
        if (onlyStaysails && bowsprit)
        {
            LogRed("A bowsprit cannot allow only staysails");
            return false;
        }

        return true;
    }
    public void Reset()
    {   //this runs every time you add the script or when clicking "reset" from the options
        if (gameObject.GetComponent<BoatPartOption>() == null && gameObject.GetComponent<CreateBoatPart>() == null)
            Undo.AddComponent<CreateBoatPart>(gameObject);

        Bounds bounds = GetComponent<MeshFilter>().sharedMesh.bounds;
        Vector3 extents = bounds.extents;
        radius = extents.x;
        maxHeight = (float)Math.Round(extents.z * 2f - (extents.z + bounds.center.z), 2);

        mastHeight = (float)Math.Round(0.75f * maxHeight, 2);

        string scriptPath = GetScriptFolderPath();
        string prefabPath = Path.Combine(Path.GetDirectoryName(scriptPath), "Prefabs");
        winchPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(prefabPath, "Winch.prefab"));
        blockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(prefabPath, "Block.prefab"));
        string missingPrefabs =
            (winchPrefab == null ? "Winch prefab, " : "") +
            (blockPrefab == null ? "Block prefab, " : "");

        if (!string.IsNullOrEmpty(missingPrefabs))
        {
            LogRed($"{missingPrefabs.TrimEnd(',', ' ')} not found. Should be in the {prefabPath} folder");
        }
        else
        {
            LogGreen("Winch and block prefabs loaded");
        }
    }
    public void OnDrawGizmosSelected()
    {
        Vector3 size = new Vector3(1f, 0.01f, 2.4f * radius);
        Vector3 offset = new Vector3(0f, 0f, -mastHeight);
        Vector3 worldPosition = transform.TransformPoint(offset);
        Quaternion gizmoRotation = transform.rotation * Quaternion.Euler(90f, 0f, 0f);

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.9f);
        Gizmos.matrix = Matrix4x4.TRS(transform.position, gizmoRotation, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, size);

        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.9f);
        Gizmos.matrix = Matrix4x4.TRS(worldPosition, gizmoRotation, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, size);
        Gizmos.matrix = Matrix4x4.identity;
        //Gizmos.DrawCube(new Vector3 (transform.position.x, transform.position.y - mastHeight, transform.position.z), size);
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
    private void LogRed(string str)
    {
        Debug.LogError($"<color=red>{str}</color>");
    }
    private void LogOrange(string str)
    {
        Debug.LogWarning($"<color=orange>{str}</color>");
    }
}
