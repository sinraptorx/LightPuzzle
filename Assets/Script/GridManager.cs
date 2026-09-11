using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GridManager : MonoBehaviour
{
    public static GridManager GM { get; set; } // 窓口

    [SerializeField] private GameObject grid;
    [SerializeField] private GameObject frame;
    public List<GameObject> blinkingGameObjects = new List<GameObject>();
    private SpriteRenderer[,] gridRenderers;

    public GameObject CLEAR; // ステージごとのクリア時のグリッド

    public int gridWidth;
    public int gridHeight;
    public float spaceLength;

    public float duration = 2.0f;
    public Color startColor = Color.red;
    public Color endColor = Color.blue;
    private Color currentColor;
    private float elapsedTime;

    void Awake()
    {
        GM = this;
    }

    void Start()
    {
        startColor = new Color32(0xFF, 0x00, 0xFF, 0xFF);
        endColor = new Color32(0x00, 0xFF, 0xFF, 0xFF);
    }

    void Update()
    {
        if (GameManager.GM.stageStarted)
        {
            GameManager.GM.left.transform.position = new Vector3(-(gridWidth - 1) / 2 - 0.5f - spaceLength, 0, 0);
            GameManager.GM.right.transform.position = new Vector3((gridWidth - 1) / 2 + 0.5f, 0, 0);
            GameManager.GM.top.transform.position = new Vector3(-spaceLength / 2.0f, (gridHeight - 1) / 2 + 0.5f, 0);
            GameManager.GM.bottom.transform.position = new Vector3(-spaceLength / 2.0f, -(gridHeight - 1) / 2 - 0.5f, 0);

            GameManager.GM.left.transform.localScale = new Vector3(0.2f, gridHeight, 0);
            GameManager.GM.right.transform.localScale = new Vector3(0.2f, gridHeight, 0);
            GameManager.GM.top.transform.localScale = new Vector3(gridWidth + spaceLength + 0.2f, 0.2f, 0);
            GameManager.GM.bottom.transform.localScale = new Vector3(gridWidth + spaceLength + 0.2f, 0.2f, 0);

            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            currentColor = Color.Lerp(startColor, endColor, t);
            foreach (GameObject g in blinkingGameObjects)
            {
                SpriteRenderer sr = g.GetComponent<SpriteRenderer>();
                sr.color = currentColor;
            }
            if (elapsedTime >= duration)
            {
                Color temp = startColor;
                startColor = endColor;
                endColor = temp;
                elapsedTime = 0;
            }

            for (int y = 0; y < gridHeight; y++)
                for (int x = 0; x < gridWidth; x++)
                {
                    SpriteRenderer sr = gridRenderers[x, y];
                    sr.color = Color.black;
                }
        }
    }

    public void Paint(Vector2 point, Color col)
    {
        int x = (int)Math.Round(point.x, MidpointRounding.AwayFromZero);
        int y = (int)Math.Round(point.y, MidpointRounding.AwayFromZero);

        if (x > (gridWidth - 1) / 2 ||
            x < -(gridWidth - 1) / 2 ||
            y > (gridHeight - 1) / 2 ||
            y < -(gridHeight - 1) / 2)
            return;

        SpriteRenderer sr = gridRenderers[x + (gridWidth - 1) / 2, y + (gridHeight - 1) / 2];
        sr.color = col;
    }

    public void StageStart(int gridW, int gridH, float spaceL)
    {
        gridWidth = gridW;
        gridHeight = gridH;
        spaceLength = spaceL;

        gridRenderers = new SpriteRenderer[gridWidth, gridHeight];

        for (int y = -(gridHeight - 1) / 2; y < (gridHeight - 1) / 2 + 1; y++)
        {
            if (y != (gridHeight - 1) / 2)
            {
                GameObject g = Instantiate(frame, new Vector2(0, y + 0.5f), Quaternion.identity);
                g.transform.localScale = new Vector2(gridWidth, 0.05f);
            }
            for (int x = -(gridWidth - 1) / 2; x < (gridWidth - 1) / 2 + 1; x++)
            {
                if (y == 0 && x != (gridWidth - 1) / 2)
                {
                    GameObject gg = Instantiate(frame, new Vector2(x + 0.5f, 0), Quaternion.identity);
                    gg.transform.localScale = new Vector2(0.05f, gridHeight);
                }

                gridRenderers[x + (gridWidth - 1) / 2, y + (gridHeight - 1) / 2] = Instantiate(grid, new Vector2(x, y), Quaternion.identity).GetComponent<SpriteRenderer>();
            }
        }
        blinkingGameObjects.Add(Instantiate(frame, new Vector2(-(gridWidth - 1) / 2 - 0.5f, 0), Quaternion.identity));
        blinkingGameObjects[^1].transform.localScale = new Vector2(0.1f, gridHeight + 0.1f);
        blinkingGameObjects[^1].AddComponent<BoxCollider2D>();

        blinkingGameObjects.Add(Instantiate(frame, new Vector2((gridWidth - 1) / 2 + 0.5f, 0), Quaternion.identity));
        blinkingGameObjects[^1].transform.localScale = new Vector2(0.1f, gridHeight + 0.1f);
        blinkingGameObjects[^1].AddComponent<BoxCollider2D>();

        blinkingGameObjects.Add(Instantiate(frame, new Vector2(0, -(gridHeight - 1) / 2 - 0.5f), Quaternion.identity));
        blinkingGameObjects[^1].transform.localScale = new Vector2(gridWidth + 0.1f, 0.1f);
        blinkingGameObjects[^1].AddComponent<BoxCollider2D>();

        blinkingGameObjects.Add(Instantiate(frame, new Vector2(0, (gridHeight - 1) / 2 + 0.5f), Quaternion.identity));
        blinkingGameObjects[^1].transform.localScale = new Vector2(gridWidth + 0.1f, 0.1f);
        blinkingGameObjects[^1].AddComponent<BoxCollider2D>();
    }

    public bool IsStageClear()
    {
        int count = 0;

        List<SpriteRenderer> srs = new List<SpriteRenderer>();
        for (int i = 0; i < CLEAR.transform.GetChild(GameManager.GM.stageNumber).gameObject.transform.childCount; i++)
        {
            GameObject g = CLEAR.transform.GetChild(GameManager.GM.stageNumber).gameObject;
            if (g.transform.GetChild(i).gameObject.tag == "Grid")
            {
                srs.Add(g.transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>());
            }
        }

        for (int y = 0; y < gridHeight; y++)
            for (int x = 0; x < gridWidth; x++)
            {
                bool red = gridRenderers[x, y].color.r > 0 == srs[y * gridWidth + x].color.r > 0;
                bool green = gridRenderers[x, y].color.g > 0 == srs[y * gridWidth + x].color.g > 0;
                bool blue = gridRenderers[x, y].color.b > 0 == srs[y * gridWidth + x].color.b > 0;
                
                if (!(red && green && blue))
                {
                    count++;
                }
            }

        bool b = count == 0;
        if (GameManager.GM.stageNumber == 6 && count < 25)
            b = true;
        return b;
    }
}
