using Shapes2D;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager GM { get; set; } // 窓口

    public static event Action FirstUpdate;
    public static event Action SecondUpdate;
    public static event Action ThirdUpdate;
    public static event Action FourthUpdate;

    public GameObject menu;
    public GameObject game;

    public GameObject StraightLinePrefab;
    public GameObject AnglePrefab;

    public GameCamera gameCamera;

    public bool educationalMode = false;

    public GameObject choseGameObject;

    public GameObject circle;
    public GameObject circlePrefab; // 光ワープ出口に丸をつけるため
    public List<GameObject> circles = new List<GameObject>();
    public GameObject wirePrefab;

    [SerializeField] private float rotateSpeed;

    public GameObject left;
    public GameObject right;
    public GameObject top;
    public GameObject bottom;
    public GameObject leftPrefab;
    public GameObject rightPrefab;
    public GameObject topPrefab;
    public GameObject bottomPrefab;

    public GameObject lightSourcePrefab;
    public GameObject lightEntrancePrefab;
    public GameObject lightExitPrefab;
    public GameObject reflectPrefab;
    public GameObject absorpPrefab;
    public GameObject glassPrefab;
    public GameObject prismPrefab;

    void Awake()
    {
        GM = this;
    }

    public float tmr = 0;
    void Update()
    {
        tmr += Time.deltaTime;
        if (choseGameObject != null)
        {
            if (choseGameObject.GetComponent<LightSource>() &&
                !(circles.Count > 0) &&
                Input.GetKeyDown(KeyCode.Space) &&
                choseGameObject.GetComponent<LightSource>().type == LightSource.Type.Entrance)
            {
                GameObject[] games = GameObject.FindGameObjectsWithTag("LightSource");
                foreach (GameObject g in games)
                {
                    LightSource ls = g.GetComponent<LightSource>();
                    if (ls.type == LightSource.Type.Exit)
                    {
                        circles.Add(Instantiate(circlePrefab, g.transform.position, Quaternion.identity));
                        circles[^1].GetComponent<ChoiceCircle>().target = g;
                    }
                }
            }

            choseGameObject.transform.SetAsLastSibling();

            circle.SetActive(true);
            circle.transform.position = choseGameObject.transform.position;
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                AudioManager.AM.Delete();
                if (choseGameObject.GetComponent<LightSource>())
                {
                    LightSource ls = choseGameObject.GetComponent<LightSource>();
                    switch (ls.type)
                    {
                        case LightSource.Type.Source:
                            lightSourceCount++;
                            break;

                        case LightSource.Type.Entrance:
                            lightEntranceCount++;
                            break;

                        case LightSource.Type.Exit:
                            lightExitCount++;
                            break;

                        default:
                            break;
                    }
                }
                else if (choseGameObject.GetComponent<Block>())
                {
                    Block b = choseGameObject.GetComponent<Block>();
                    switch (b.blockMaterial)
                    {
                        case Block.Mat.Reflection:
                            reflectCount++;
                            break;

                        case Block.Mat.Absorption:
                            absorpCount++;
                            break;

                        case Block.Mat.Refraction:
                            if (b.isPrism)
                            {
                                prismCount++;
                            }
                            else
                            {
                                glassCount++;
                            }
                            break;

                        default:
                            break;
                    }
                }

                Destroy(choseGameObject);
                choseGameObject = null;
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                choseGameObject.transform.rotation = Quaternion.Euler(0, 0, choseGameObject.transform.eulerAngles.z + rotateSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                choseGameObject.transform.rotation = Quaternion.Euler(0, 0, choseGameObject.transform.eulerAngles.z - rotateSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                rotateSpeed = 15.0f;
            }
            else
            {
                rotateSpeed = 90.0f;
            }
        }
        else
        {
            circle.SetActive(false);
            foreach(GameObject c in circles)
            {
                Destroy(c);
            }
            circles.Clear();
        }

        GameObject[] lines = GameObject.FindGameObjectsWithTag("TwoPointsLine");
        foreach (GameObject g in lines)
        {
            Destroy(g);
        }

        FirstUpdate?.Invoke();

        if (stageStarted)
        {
            int[] ints = new int[]
            {
                lightSourceCount,
                lightEntranceCount,
                lightExitCount,
                reflectCount,
                absorpCount,
                glassCount,
                prismCount,
            };
            for (int i = 0; i < 7; i++)
            {
                Text txt = game.transform.GetChild(i).gameObject.transform.GetChild(1).gameObject.GetComponent<Text>();
                txt.text = ints[i].ToString();
            }
        }
    }

    public float duration = 1.0f;
    public Color startColor = Color.green;
    public Color endColor = Color.red;
    private Color currentColor;
    private float elapsedTime;
    void LateUpdate()
    {
        SecondUpdate?.Invoke();
        ThirdUpdate?.Invoke();

        foreach (GameObject ga in GameObject.FindGameObjectsWithTag("LightSource").Concat(GameObject.FindGameObjectsWithTag("LightSourcePoint")))
        {
            LightSource ls = ga.GetComponent<LightSource>();
            LineRenderer lr = ga.GetComponent<LineRenderer>();
            if (ls.hitLightEntrance)
            {
                bool CONT = false;
                foreach (GameObject gaa in GameObject.FindGameObjectsWithTag("LightSource").Concat(GameObject.FindGameObjectsWithTag("LightSourcePoint")))
                {
                    LightSource lss = gaa.GetComponent<LightSource>();
                    if (lss.hitLs == ls.hitLs && ls.hitLsTime > lss.hitLsTime)
                        CONT = true;
                }

                if (CONT)
                    continue;

                if (ga.tag != "LightSourcePoint" ||  lr.positionCount > 2)
                {
                    int r = 0;
                    int g = 0;
                    int b = 0;
                    if (lr.startColor.r > 0)
                        r = 255;
                    if (lr.startColor.g > 0)
                        g = 255;
                    if (lr.startColor.b > 0)
                        b = 255;

                    if (ls.hitLs.exitLs != null)
                    {
                        ls.hitLs.exitLs.lineRenderer.startColor = new Color32((byte)r, (byte)g, (byte)b, 255);
                        ls.hitLs.exitLs.lineRenderer.endColor = new Color32((byte)r, (byte)g, (byte)b, 255);
                        ls.hitLs.exitLs.DrawLaser(new Color32((byte)r, (byte)g, (byte)b, 255), ls.hitLs.exitLs.gameObject.transform.position, ls.hitLs.exitLs.gameObject.transform.up);
                    }
                }
            }
        }

        FourthUpdate?.Invoke();

        elapsedTime += Time.deltaTime;
        float t = elapsedTime / duration;
        currentColor = Color.Lerp(startColor, endColor, t);

        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Double"))
        {
            g.GetComponent<LineRenderer>().startColor = currentColor;
            g.GetComponent<LineRenderer>().endColor = currentColor;
        }

        if (elapsedTime >= duration)
        {
            Color temp = startColor;
            startColor = endColor;
            endColor = temp;
            elapsedTime = 0;
        }

        GameObject[] lightSources = GameObject.FindGameObjectsWithTag("LightSource");
        GameObject[] lightSourcePoints = GameObject.FindGameObjectsWithTag("LightSourcePoint");

        GameObject[] synthesis = lightSources.Concat(lightSourcePoints)
    .OrderBy(g => g.transform.GetSiblingIndex())
    .ToArray();

        foreach (GameObject g in synthesis)
        {
            LineRenderer lineRenderer = g.GetComponent<LineRenderer>();
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                if (i == lineRenderer.positionCount - 1)
                    break;

                float posX1 = lineRenderer.GetPosition(i).x;
                float posX2 = lineRenderer.GetPosition(i + 1).x;
                float posY1 = lineRenderer.GetPosition(i).y;
                float posY2 = lineRenderer.GetPosition(i + 1).y;
                float xDif = posX2 - posX1;
                float yDif = posY2 - posY1;
                float xCeil = 100;
                float xDeg = xDif / xCeil;
                float yDeg = yDif / xCeil;

                for (int j = 0; j < xCeil; j++)
                {
                    Vector2 v = new Vector2(posX1 + xDeg * j, posY1 + yDeg * j);
                    GridManager.GM.Paint(v, lineRenderer.startColor);
                }
            }
        }

        if (stageNumber >= 0 && stageStarted && !stageClear && GridManager.GM.IsStageClear())
        {
            stageClear = true;

            if (choseGameObject != null)
            {
                if (choseGameObject.transform.childCount > 0 &&
                choseGameObject.GetComponent<Block>())
                {
                    ClickJudge cj = choseGameObject.transform.GetChild(0).gameObject.GetComponent<ClickJudge>();
                    cj.isFirst = true;
                }

                if (choseGameObject.GetComponent<LightSource>())
                {
                    choseGameObject.GetComponent<LightSource>().beChosen = false;
                    LightSource ls = choseGameObject.GetComponent<LightSource>();
                    ls.isFirst = true;
                    if (ls.wire != null)
                    {
                        ls.wire.GetComponent<Wire>().end.GetComponent<LightSource>().wired = false;
                        Destroy(ls.wire);
                        ls.wire = null;
                        ls.exitLs = null;
                    }
                }
                else if (choseGameObject.GetComponent<Block>())
                {
                    Block b = choseGameObject.GetComponent<Block>();
                    b.isFirst = true;
                }
                choseGameObject = null;
            }
        }
    }

    public int lightSourceCount;
    public int lightEntranceCount;
    public int lightExitCount;
    public int reflectCount;
    public int absorpCount;
    public int glassCount;
    public int prismCount;
    public void CreateObject(string name)
    {
        Vector3 v = gameCamera.gameObject.transform.position;
        v.z = 0;

        if (name == "LightSource" || name == "LightEntrance" || name == "Prism")
        {
            Vector3 temp = v;
            temp.x = Mathf.Clamp(v.x, -(GridManager.GM.gridWidth - 1) / 2 - 0.5f - 0.25f, (GridManager.GM.gridWidth - 1) / 2 + 0.5f + 0.25f);
            temp.y = Mathf.Clamp(v.y, -(GridManager.GM.gridHeight - 1) / 2 - 0.5f - 0.25f, (GridManager.GM.gridHeight - 1) / 2 + 0.5f + 0.25f);
            
            if (temp == v)
                return;
        }

        if (name == "LightExit")
        {
            Vector3 temp = v;
            temp.x = Mathf.Clamp(v.x, -(GridManager.GM.gridWidth - 1) / 2 - 0.5f + 0.25f, (GridManager.GM.gridWidth - 1) / 2 + 0.5f - 0.25f);
            temp.y = Mathf.Clamp(v.y, -(GridManager.GM.gridHeight - 1) / 2 - 0.5f + 0.25f, (GridManager.GM.gridHeight - 1) / 2 + 0.5f - 0.25f);

            if (temp != v)
                return;
        }

        foreach (GameObject g in GameObject.FindGameObjectsWithTag("LightSource"))
        {
            if (g.transform.position.x == v.x && g.transform.position.y == v.y)
                return;
        }

        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Block"))
        {
            if (g.transform.position.x == v.x && g.transform.position.y == v.y)
                return;
        }

        switch (name)
        {
            case "LightSource":
                if (lightSourceCount == 0)
                    break;

                lightSourceCount--;
                Instantiate(lightSourcePrefab, v, Quaternion.identity);
                AudioManager.AM.Place();
                break;

            case "LightEntrance":
                if (lightEntranceCount == 0)
                    break;

                lightEntranceCount--;
                Instantiate(lightEntrancePrefab, v, Quaternion.identity);
                AudioManager.AM.Place();
                break;

            case "LightExit":
                if (lightExitCount == 0)
                    break;

                lightExitCount--;
                Instantiate(lightExitPrefab, v, Quaternion.identity);
                AudioManager.AM.Place();
                break;

            case "Reflect":
                if (reflectCount == 0)
                    break;

                reflectCount--;
                Instantiate(reflectPrefab, v, Quaternion.identity);
                AudioManager.AM.Place();
                break;

            case "Absorp":
                if (absorpCount == 0)
                    break;

                absorpCount--;
                Instantiate(absorpPrefab, v, Quaternion.identity);
                AudioManager.AM.Place();
                break;

            case "Glass":
                if (glassCount == 0)
                    break;

                glassCount--;
                GameObject g = Instantiate(glassPrefab, v, Quaternion.identity);
                if (stageNumber == 6)
                {
                    g.transform.localScale = new Vector3(g.transform.localScale.x, 4.5f, g.transform.localScale.z);
                }
                AudioManager.AM.Place();
                break;

            case "Prism":
                if (prismCount == 0)
                    break;

                prismCount--;
                Instantiate(prismPrefab, v, Quaternion.identity);
                AudioManager.AM.Place();
                break;

            default:
                break;
        }
    }

    public bool stageStarted;
    public bool stageClear;
    public int stageNumber;

    public int[] gridWidthes;
    public int[] gridHeights;
    public float[] spaceLengthes;

    public void StageStart(int num)
    {
        AudioManager.AM.StageStart();

        if (num == 0)
        {
            gameCamera.sizeLimit = 100;

            stageClear = false;
            left = Instantiate(leftPrefab, Vector2.zero, Quaternion.identity);
            right = Instantiate(rightPrefab, Vector2.zero, Quaternion.identity);
            top = Instantiate(topPrefab, Vector2.zero, Quaternion.identity);
            bottom = Instantiate(bottomPrefab, Vector2.zero, Quaternion.identity);

            int stageNum = num - 1;
            GridManager.GM.StageStart(101, 101, 100);

            stageNumber = stageNum;

            lightSourceCount = 1000;
            lightEntranceCount = 1000;
            lightExitCount = 1000;
            reflectCount = 1000;
            absorpCount = 1000;
            glassCount = 1000;
            prismCount = 1000;
        }
        else
        {
            gameCamera.sizeLimit = 30;
            stageClear = false;
            left = Instantiate(leftPrefab, Vector2.zero, Quaternion.identity);
            right = Instantiate(rightPrefab, Vector2.zero, Quaternion.identity);
            top = Instantiate(topPrefab, Vector2.zero, Quaternion.identity);
            bottom = Instantiate(bottomPrefab, Vector2.zero, Quaternion.identity);

            int stageNum = num - 1;
            GridManager.GM.StageStart(gridWidthes[stageNum], gridHeights[stageNum], spaceLengthes[stageNum]);

            stageNumber = stageNum;

            lightSourceCount = 0;
            lightEntranceCount = 0;
            lightExitCount = 0;
            reflectCount = 0;
            absorpCount = 0;
            glassCount = 0;
            prismCount = 0;

            switch (num)
            {
                case 1:
                    lightSourceCount = 1;
                    lightEntranceCount = 1;
                    lightExitCount = 1;
                    reflectCount = 3;
                    absorpCount = 1;
                    break;

                case 2:
                    lightSourceCount = 3;
                    lightEntranceCount = 3;
                    lightExitCount = 3;
                    absorpCount = 3;
                    break;

                case 3:
                    lightSourceCount = 1;
                    lightEntranceCount = 1;
                    lightExitCount = 1;
                    reflectCount = 10;
                    absorpCount = 2;
                    prismCount = 1;
                    break;

                case 4:
                    lightSourceCount = 1;
                    lightEntranceCount = 1;
                    lightExitCount = 1;
                    reflectCount = 7;
                    absorpCount = 2;
                    prismCount = 1;
                    break;

                case 5:
                    lightSourceCount = 1;
                    lightEntranceCount = 3;
                    lightExitCount = 3;
                    reflectCount = 4;
                    absorpCount = 3;
                    prismCount = 1;
                    break;

                case 6:
                    lightSourceCount = 1;
                    lightEntranceCount = 1;
                    lightExitCount = 1;
                    reflectCount = 10;
                    absorpCount = 2;
                    prismCount = 2;
                    break;

                case 7:
                    lightSourceCount = 1;
                    lightEntranceCount = 1;
                    lightExitCount = 1;
                    reflectCount = 1;
                    absorpCount = 1;
                    glassCount = 1;
                    prismCount = 1;
                    break;

                case 8:
                    lightSourceCount = 7;
                    lightEntranceCount = 7;
                    lightExitCount = 7;
                    reflectCount = 5;
                    absorpCount = 5;
                    prismCount = 7;
                    break;

                case 9:
                    lightSourceCount = 3;
                    lightEntranceCount = 3;
                    lightExitCount = 3;
                    reflectCount = 10;
                    absorpCount = 2;
                    break;

                case 10:
                    lightSourceCount = 9;
                    lightEntranceCount = 9;
                    lightExitCount = 9;
                    reflectCount = 20;
                    absorpCount = 10;
                    prismCount = 16;
                    break;

                case 11:
                    lightSourceCount = 1;
                    lightEntranceCount = 1;
                    lightExitCount = 1;
                    reflectCount = 2;
                    glassCount = 3;
                    break;

                case 12:
                    lightSourceCount = 2;
                    lightEntranceCount = 4;
                    lightExitCount = 4;
                    reflectCount = 15;
                    absorpCount = 3;
                    glassCount = 1;
                    prismCount = 2;
                    break;

                default:
                    lightSourceCount = 100;
                    lightEntranceCount = 100;
                    lightExitCount = 100;
                    reflectCount = 100;
                    absorpCount = 100;
                    glassCount = 100;
                    prismCount = 100;
                    break;
            }
        }

        menu.SetActive(false);
        game.SetActive(true);
        GridManager.GM.CLEAR.SetActive(true);

        for (int i = 0; i < GridManager.GM.CLEAR.transform.childCount; i++)
        {
            GridManager.GM.CLEAR.transform.GetChild(i).gameObject.SetActive(false);
        }
        if (num != 0)
        {
            GridManager.GM.CLEAR.transform.GetChild(stageNumber).gameObject.SetActive(true);
        }

        gameCamera.size = 15;
        gameCamera.gameObject.GetComponent<Camera>().orthographicSize = gameCamera.size;
        gameCamera.pos = new Vector3(0, 0, gameCamera.gameObject.transform.position.z);
        gameCamera.gameObject.transform.position = new Vector3(0, 0, gameCamera.gameObject.transform.position.z);

        stageStarted = true;
    }

    public void StageEnd(bool b)
    {
        if (b)
            AudioManager.AM.StageStart();

        stageStarted = false;

        string[] tags = { "Wire", "LightSource", "Block", "Grid", "Frame", "Border" };
        foreach (string tag in tags)
            foreach (GameObject g in GameObject.FindGameObjectsWithTag(tag))
            {
                if (tag == "Grid" || tag == "Frame")
                    if (g.transform.parent != null && g.transform.parent.gameObject.transform.parent.gameObject == GridManager.GM.CLEAR)
                        continue;

                Destroy(g);
            }

        menu.SetActive(true);
        game.SetActive(false);
        GridManager.GM.CLEAR.SetActive(false);
        GridManager.GM.blinkingGameObjects.Clear();
    }
}
