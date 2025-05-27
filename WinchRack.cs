#if (UNITY_EDITOR)
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WinchRack : MonoBehaviour
{
    [Tooltip("Add winches to this list to keep track of them")]
    public List<Transform> winches;

    [Tooltip("The rack will have an identical copy of itself placed symmetrically on the ship")]
    public bool isSymmetrical;

    [Tooltip("Maximum number of winches that can be added to the rack")]
    public int maxWinches = 6;

    [Tooltip("Spacing between winches")]
    public float winchSpacing = 0.33f;

    //[HideInInspector]
    //public float[] winchesPosY = { 0.825f, 0.495f, 0.165f, -0.165f, -0.495f, 0.825f };
    public float[] winchesPosY;

    //[HideInInspector]
    public int lastWinch = 0;

    [HideInInspector]
    public WinchRack other;

    [HideInInspector]
    public bool refresh;

    private float previousSpacing;

    public void Reset()
    {
        CalculatePos();
        Debug.LogWarning("lastWinch: " + lastWinch);
    }
    public void OnValidate()
    {
        if (winchesPosY.Length != maxWinches || winchSpacing != previousSpacing)
        {
            CalculatePos();
            previousSpacing = winchSpacing;
        }
    }
    public void InstantiateOther()
    {
        other = Instantiate(gameObject, transform.parent).GetComponent<WinchRack>();
        other.transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -transform.localPosition.z);
        other.transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y + 180f, transform.localRotation.eulerAngles.z);
        other.other = this;
        other.isSymmetrical = true;
        other.name = name + "_other" ;
        Array.Reverse(other.winchesPosY);
        Undo.RegisterCreatedObjectUndo(other, "Created other WinchRack for " + name);
    }
    public void CalculatePos()
    {
        winchesPosY = new float[maxWinches];
        float start = -(maxWinches - 1) * winchSpacing / 2f;
        for (int i = 0; i < maxWinches; i++)
        {
            winchesPosY[i] = Mathf.Round((start + i * winchSpacing) * 1000f) / 1000f;
        }
        Debug.LogWarning("winchesPosY: " + string.Join(", ", winchesPosY));
    }
}
#endif