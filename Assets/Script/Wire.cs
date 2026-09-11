using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : MonoBehaviour
{
    public GameObject start, end;

    public float duration = 2.0f;
    public Color startColor = Color.green;
    public Color endColor = Color.red;
    private Color currentColor;
    private float elapsedTime;
    void Update()
    {
        elapsedTime += Time.deltaTime;
        float t = elapsedTime / duration;
        currentColor = Color.Lerp(startColor, endColor, t);

        LineRenderer lr = GetComponent<LineRenderer>();
        lr.startColor = currentColor;
        lr.endColor = currentColor;

        lr.positionCount = 0;
        lr.positionCount = 1;
        lr.SetPosition(0, start.transform.position);
        lr.positionCount = 2;
        lr.SetPosition(1, end.transform.position);

        lr.sortingOrder = 30;
        lr.sortingLayerName = "Foreground";

        if (elapsedTime >= duration)
        {
            Color temp = startColor;
            startColor = endColor;
            endColor = temp;
            elapsedTime = 0;
        }
    }
}
