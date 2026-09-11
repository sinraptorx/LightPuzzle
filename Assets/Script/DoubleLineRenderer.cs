using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class DoubleLineRenderer : MonoBehaviour
{
    // 線の枠をつくる
    [SerializeField] private LineRenderer source;

    public Color currentColor;

    private void OnEnable()
    {
        GameManager.FourthUpdate += Fourth;
    }
    private void OnDisable()
    {
        GameManager.FourthUpdate -= Fourth;
    }

    public void Fourth()
    {
        LineRenderer lr = GetComponent<LineRenderer>();
        lr.sortingOrder = 29;
        lr.sortingLayerName = "Foreground";

        lr.positionCount = source.positionCount;

        Vector3[] positions = new Vector3[source.positionCount];
        source.GetPositions(positions);

        lr.SetPositions(positions);
    }
}
