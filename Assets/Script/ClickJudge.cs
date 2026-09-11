using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickJudge : MonoBehaviour
{
    private Vector3 offset;

    public bool isFirst = true;
    public bool beChosen = false;
    public Vector3 initialPos;

    private void OnMouseDown()
    {
        if (GameManager.GM.stageClear)
            return;

        initialPos = transform.position;
        beChosen = true;

        if (GameManager.GM.choseGameObject != null)
        {
            Vector3 mouseWorldPos = GetMouseWorldPos();
            offset = transform.parent.gameObject.transform.position - mouseWorldPos;
        }
        GameManager.GM.choseGameObject = transform.parent.gameObject;
    }

    private void OnMouseUp()
    {
        if (GameManager.GM.choseGameObject)
        {
            if (isFirst)
                isFirst = false;
        }
    }

    private void OnMouseDrag()
    {
        if (isFirst)
            return;

        Vector3 newPosition = GetMouseWorldPos() + offset;

        newPosition.z = transform.parent.transform.position.z;

        newPosition.x = Mathf.Clamp(newPosition.x, GameManager.GM.left.transform.position.x, GameManager.GM.right.transform.position.x);
        newPosition.y = Mathf.Clamp(newPosition.y, GameManager.GM.bottom.transform.position.y, GameManager.GM.top.transform.position.y);

        transform.parent.transform.position = newPosition;
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = -GameManager.GM.gameCamera.gameObject.transform.position.z;
        return GameManager.GM.gameCamera.gameObject.GetComponent<Camera>().ScreenToWorldPoint(mouseScreenPos);
    }

}
