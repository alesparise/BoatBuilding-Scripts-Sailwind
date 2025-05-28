using Crest;
using System.Reflection;
using UnityEngine;

public class ShowBoatInfos : MonoBehaviour
{   //Goes on the main Boat object (the one with BoatProbes etc)
    [Header("BoatProbes")]
    public BoatProbes probes;
    public bool showBoatProbes = true;

    [Header("BoatAlignNormal")]
    public BoatAlignNormal alignNormal;
    public bool showBoatAlignNormal = true;

    [Header("BoatKeel")]
    public BoatKeel keel;
    public bool showBoatKeel = true;

    private void Reset()
    {
        probes = GetComponent<BoatProbes>();
        if (probes == null)
        {
            Debug.LogError("<color=red>BoatProbes component not found</color>");
        }
        alignNormal = GetComponent<BoatAlignNormal>();
        if (alignNormal == null)
        {
            Debug.LogError("<color=red>BoatAlignNormal component not found</color>");
        }
        keel = GetComponent<BoatKeel>();
        if (keel == null)
        {
            Debug.LogError("<color=red>BoatKeel component not found</color>");
        }
    }
    public void OnDrawGizmosSelected()
    {
        if (probes !=null && showBoatProbes)
        { 
            //BOAT PROBES
            Quaternion gizmoRotation = transform.rotation;

            //Draw center of mass
            FieldInfo field2 = typeof(BoatProbes).GetField("_centerOfMass", BindingFlags.NonPublic | BindingFlags.Instance);
            Vector3 center = (Vector3)field2.GetValue(probes);

            Gizmos.matrix = Matrix4x4.TRS(transform.position, gizmoRotation, Vector3.one);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(center, 0.2f);

            //Draw boat width
            Gizmos.color = Color.magenta;
            Vector3 probesBoatSize = new Vector3 (probes.ObjectWidth, 1f, 1f);
            Gizmos.DrawWireCube(Vector3.zero, probesBoatSize);

            //Draw force points
            FieldInfo field = typeof(BoatProbes).GetField("customScale", BindingFlags.NonPublic | BindingFlags.Instance);
            Vector3 customScale = (Vector3)field.GetValue(probes);

            Gizmos.color = Color.blue;
            foreach (FloaterForcePoints point in probes._forcePoints)
            {
                Vector3 pos = Vector3.Scale(point._offsetPosition, customScale);
                Gizmos.matrix = Matrix4x4.TRS(transform.position, gizmoRotation, Vector3.one);
                Gizmos.DrawCube(pos, Vector3.one * 0.5f);
            }
        }
        if (alignNormal != null && showBoatAlignNormal)
        {
            //BOAT ALIGN NORMAL
            FieldInfo field3 = typeof(BoatAlignNormal).GetField("_boatLength", BindingFlags.NonPublic | BindingFlags.Instance);
            float boatLen = (float)field3.GetValue(alignNormal);
            FieldInfo field4 = typeof(BoatAlignNormal).GetField("_bottomH", BindingFlags.NonPublic | BindingFlags.Instance);
            float bottomH = (float)field4.GetValue(alignNormal);

            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Vector3 boatSize = new Vector3(alignNormal.ObjectWidth, 1f, boatLen);
            Gizmos.DrawWireCube(Vector3.zero, boatSize);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(Vector3.zero * -bottomH, 0.2f);
        }
        if (keel != null && showBoatKeel)
        {
            //BOAT KEEL
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(keel.centerOfMass, 0.2f);
        }
        Gizmos.matrix = Matrix4x4.identity;

    }
}
