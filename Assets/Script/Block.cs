using Shapes2D;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Block : MonoBehaviour
{
    public bool liftable;

    [SerializeField] private Material[] materials;

    public enum Mat // ブロックの材質
    {
        Reflection,
        Absorption,
        Refraction,
    }
    public Mat blockMaterial;
    public bool isPrism;

    enum Motion
    {
        Stop,
        Move,
        Rotate,
    }
    [SerializeField] private Motion blockMotion;

    [SerializeField] private Vector3 target1;
    [SerializeField] private Vector3 target2;
    [SerializeField] private float moveSpeed;
    private Vector3 currentTarget;

    void Start()
    {
        currentTarget = target1;
    }

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

        Shape shape = GetComponent<Shape>();

        shape.settings.outlineColor = currentColor;

        if (elapsedTime >= duration)
        {
            Color temp = startColor;
            startColor = endColor;
            endColor = temp;
            elapsedTime = 0;
        }

        switch (blockMotion)
        {
            case Motion.Move:
                transform.position = Vector3.MoveTowards(transform.position, currentTarget, moveSpeed * Time.deltaTime);
                if (transform.position == currentTarget)
                {
                    if (currentTarget == target1)
                    {
                        currentTarget = target2;
                    }
                    else
                    {
                        currentTarget = target1;
                    }
                }
                break;
            case Motion.Rotate:
                break;
            default:
                break;
        }
    }

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
            offset = transform.position - mouseWorldPos;
        }
        GameManager.GM.choseGameObject = gameObject;
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

        newPosition.z = transform.position.z;

        if (blockMaterial == Mat.Refraction && isPrism)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, GameManager.GM.left.transform.position.x + 0.25f, GameManager.GM.right.transform.position.x - 0.25f);
            newPosition.y = Mathf.Clamp(newPosition.y, GameManager.GM.bottom.transform.position.y + 0.25f, GameManager.GM.top.transform.position.y - 0.25f);

            if (newPosition.x > -(GridManager.GM.gridWidth - 1) / 2 - 0.75f &&
                newPosition.x < (GridManager.GM.gridWidth - 1) / 2 + 0.75f &&
                newPosition.y > -(GridManager.GM.gridHeight - 1) / 2 - 0.75f &&
                newPosition.y < (GridManager.GM.gridHeight - 1) / 2 + 0.75f)
                return;
        }
        else
        {
            newPosition.x = Mathf.Clamp(newPosition.x, GameManager.GM.left.transform.position.x + 0.25f, GameManager.GM.right.transform.position.x - 0.25f);
            newPosition.y = Mathf.Clamp(newPosition.y, GameManager.GM.bottom.transform.position.y + 0.25f, GameManager.GM.top.transform.position.y - 0.25f);
        }

        transform.position = newPosition;
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = -GameManager.GM.gameCamera.gameObject.transform.position.z;
        return GameManager.GM.gameCamera.gameObject.GetComponent<Camera>().ScreenToWorldPoint(mouseScreenPos);
    }

}
