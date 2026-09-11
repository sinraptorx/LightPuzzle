using Shapes2D;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class LightSource : MonoBehaviour
{
    public enum Type
    {
        Source,
        Entrance,
        Exit,
    }
    public Type type;
    public LightSource exitLs; // 種類が入口だった場合のみ使用

    //

    public GameObject lightSourcePoint;
    public bool isLightSourcePoint;

    public LineRenderer lineRenderer;
    public List<GameObject> straightLines = new List<GameObject>();
    public List<GameObject> angles = new List<GameObject>();

    public Color lightColor;
    private bool lightEmitted = false;

    public float maxDistance = 15f;
    public int maxReflections = 3;

    public float eta;
    [SerializeField] private float refractiveIndex;
    private bool lightInAir = true;
    private bool totalReflection = false;

    private int straightLineNum;
    private int angleNum;
    private float destroyTmr = 0;

    [SerializeField] private GameObject twoPointsLine;
    private Vector2 lastDirection; // 重なっている光を統合させるため
    private Vector2 lastHitPoint;
    private Color32 lastCol32;
    private bool isChanged = false;

    void Start()
    {
        if (lightColor.r > 0)
            refractiveIndex = 1.5f;
        else if (lightColor.g > 0)
            refractiveIndex = 1.75f;
        else if (lightColor.b > 0)
            refractiveIndex = 2.0f;
        else if (lightColor.r > 0 && lightColor.g > 0 && lightColor.b > 0)
            refractiveIndex = 1.75f;
    }

    public float duration = 2.0f;
    public Color startColor = Color.green;
    public Color endColor = Color.red;
    private Color currentColor;
    private float elapsedTime;
    void First()
    {
        hitLightEntrance = false;
        hitLs = null;

        if (GetComponent<Shape>())
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
        }

        if (isLightSourcePoint)
        {
            Destroy(gameObject);
        }
        else
        {
            lightInAir = true;
        }

        destroyTmr += Time.deltaTime;
        if (!GameManager.GM.educationalMode)
            destroyTmr = 10;
        if (destroyTmr > 10)
        {
            foreach (GameObject s in straightLines)
            {
                Destroy(s);
            }
            foreach (GameObject a in angles)
            {
                Destroy(a);
            }
            straightLines.Clear();
            angles.Clear();
            destroyTmr = 0;
        }

        foreach (GameObject s in straightLines)
        {
            Destroy(s);
            //s.SetActive(false);
        }
        foreach (GameObject a in angles)
        {
            Destroy(a);
            //a.SetActive(false);
        }

        straightLineNum = 0;
        angleNum = 0;

        lineRenderer.positionCount = 0;

        if (type != Type.Source)
        {
            lightColor = Color.black;
        }
        lightEmitted = false;
    }

    void Second()
    {
        if (!isLightSourcePoint)
        {
            if (type == Type.Source)
            {
                DrawLaser(lightColor, transform.position, transform.up);
            }
        }
    }

    void Third()
    {
        if (isChanged)
        {
            GameObject[] points = GameObject.FindGameObjectsWithTag("LightSourcePoint");

            foreach (GameObject point in points)
            {
                if (gameObject != point)
                {
                    if (gameObject.GetInstanceID() < point.GetInstanceID())
                    {
                        LightSource ls = point.GetComponent<LightSource>();
                        if (ls == null) continue;

                        float dis = Vector2.Distance(lastHitPoint, ls.lastHitPoint);
                        float similarity = Vector2.Dot(lastDirection, ls.lastDirection);

                        if (dis < 0.15f && similarity > 0.9995f)
                        {
                            byte r = (byte)(lastCol32.r > 0 || ls.lastCol32.r > 0 ? 255 : 0);
                            byte g = (byte)(lastCol32.g > 0 || ls.lastCol32.g > 0 ? 255 : 0);
                            byte b = (byte)(lastCol32.b > 0 || ls.lastCol32.b > 0 ? 255 : 0);

                            GameObject line = Instantiate(twoPointsLine, Vector2.zero, Quaternion.identity);

                            line.GetComponent<LineRenderer>().positionCount = 1;
                            line.GetComponent<LineRenderer>().SetPosition(0, lineRenderer.GetPosition(0));
                            line.GetComponent<LineRenderer>().positionCount = 2;
                            line.GetComponent<LineRenderer>().SetPosition(1, lineRenderer.GetPosition(1));

                            line.GetComponent<LineRenderer>().startColor = new Color(lineRenderer.startColor.r, lineRenderer.startColor.g, lineRenderer.startColor.b, 1);
                            line.GetComponent<LineRenderer>().endColor = new Color(lineRenderer.endColor.r, lineRenderer.endColor.g, lineRenderer.endColor.b, 1);
                            line.GetComponent<LineRenderer>().sortingLayerName = "Foreground";
                            line.GetComponent<LineRenderer>().sortingOrder = 31;

                            Color32 combinedColor = new Color32(r, g, b, 255);
                            lineRenderer.startColor = combinedColor;
                            lineRenderer.endColor = combinedColor;

                            ls.lineRenderer.positionCount = 2;
                        }
                    }
                }
            }
        }
    }

    void Awake()
    {
        Physics2D.queriesStartInColliders = false;
    }

    private void OnEnable()
    {
        GameManager.FirstUpdate += First;
        GameManager.SecondUpdate += Second;
        GameManager.ThirdUpdate += Third;
    }

    private void OnDisable()
    {
        GameManager.FirstUpdate -= First;
        GameManager.SecondUpdate -= Second;
        GameManager.ThirdUpdate -= Third;
    }

    public float hitLsTime;
    public LightSource hitLs;
    public bool hitLightEntrance;
    public void DrawLaser(Color32 color, Vector2 origin, Vector2 direction)
    {
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, origin);

        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.numCornerVertices = 4;

        lineRenderer.sortingLayerName = "Foreground";
        lineRenderer.sortingOrder = 30;
        totalReflection = false;

        void Reflect(RaycastHit2D hit)
        {
            if (GameManager.GM.educationalMode)
            {
                Vector2 tangent = new Vector2(hit.normal.y, -hit.normal.x);
                for (int j = 0; j < 2; j++)
                {
                    GameObject game = null;
                    if (straightLines.Count >= straightLineNum)
                    {
                        game = Instantiate(GameManager.GM.StraightLinePrefab, Vector2.zero, Quaternion.Euler(0, 0, 0));
                        straightLines.Add(game);
                    }
                    else
                    {
                        game = straightLines[straightLineNum];
                        game.SetActive(true);
                    }
                    straightLineNum++;

                    LineRenderer lr = game.GetComponent<LineRenderer>();
                    lr.sortingLayerName = "Foreground";
                    lr.sortingOrder = 21;
                    if (j == 0)
                    {
                        lr.positionCount = 1;
                        lr.SetPosition(0, hit.point + -hit.normal * 0.25f);
                        lr.positionCount++;
                        lr.SetPosition(1, hit.point + hit.normal * 0.25f);
                    }
                    else if (j == 1)
                    {
                        lr.positionCount = 1;
                        lr.SetPosition(0, hit.point + -tangent * 0.25f);
                        lr.positionCount++;
                        lr.SetPosition(1, hit.point + tangent * 0.25f);
                    }
                }

                for (int j = 0; j < 2; j++)
                {
                    GameObject game = null;
                    if (angles.Count >= angleNum)
                    {
                        game = Instantiate(GameManager.GM.AnglePrefab, hit.point, Quaternion.Euler(0, 0, 0));
                        angles.Add(game);
                    }
                    else
                    {
                        game = angles[angleNum];
                        game.SetActive(true);
                        game.transform.position = hit.point;
                    }
                    angleNum++;

                    Circle c = game.GetComponent<Circle>();
                    if (j == 0)
                    {
                        c.GenerateSectorBetweenVectors(-direction, hit.normal, Color.red);
                    }
                    else if (j == 1)
                    {
                        c.GenerateSectorBetweenVectors(hit.normal, Vector2.Reflect(direction, hit.normal), Color.blue);
                    }
                }
            }
            direction = Vector2.Reflect(direction, hit.normal);
            origin = hit.point + direction * 0.01f;
        }

        for (int i = 0; i < maxReflections; i++)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, maxDistance);
            float closestDis = float.MaxValue;
            RaycastHit2D hit = new RaycastHit2D();
            foreach (RaycastHit2D h in hits)
            {
                if (h.collider.gameObject.tag == "Click")
                    continue;

                if (h.collider.gameObject.tag == "LightSource" &&
                    h.collider.gameObject.GetComponent<LightSource>().type != LightSource.Type.Entrance)
                    continue;

                if (h.collider.gameObject == gameObject)
                    continue;

                float dis = Vector2.Distance(h.point, origin);
                if (dis < closestDis)
                {
                    closestDis = dis;
                    hit = h;
                }
            }

            if (hit.collider != null)
            {
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, hit.point);

                if (hit.collider.gameObject.tag == "Block")
                {
                    Block.Mat material = hit.collider.gameObject.GetComponent<Block>().blockMaterial;

                    switch (material)
                    {
                        case Block.Mat.Reflection: // 反射
                            Reflect(hit);
                            break;
                        case Block.Mat.Absorption: // 吸収
                            break;
                        case Block.Mat.Refraction: // 屈折
                            if (hit.collider.gameObject.GetComponent<Block>().isPrism && lightInAir)
                            {
                                if (!totalReflection)
                                {
                                    if (lightInAir)
                                    {
                                        lightInAir = false;
                                    }
                                    else
                                    {
                                        lightInAir = true;
                                    }
                                }

                                bool red = color.r > 0;
                                bool green = color.g > 0;
                                bool blue = color.b > 0;

                                for (int k = 0; k < 3; k++)
                                {
                                    bool draw = false;
                                    Color lightCol = new Color();
                                    if (k == 0 && red)
                                    {
                                        draw = true;
                                        lightCol = Color.red;
                                        eta = 1.0f / 1.5f;
                                    }
                                    if (k == 1 && green)
                                    {
                                        draw = true;
                                        lightCol = Color.green;
                                        eta = 1.0f / 1.75f;
                                    }
                                    if (k == 2 && blue)
                                    {
                                        draw = true;
                                        lightCol = Color.blue;
                                        eta = 1.0f / 2.0f;
                                    }
                                    if (draw)
                                    {
                                        if (TryRefract(direction, hit.normal, eta, out Vector2 refractedDir))
                                        {
                                            if (GameManager.GM.educationalMode)
                                            {
                                                Vector2 tangent = new Vector2(hit.normal.y, -hit.normal.x);
                                                for (int j = 0; j < 2; j++)
                                                {
                                                    GameObject game = null;
                                                    if (straightLines.Count >= straightLineNum)
                                                    {
                                                        game = Instantiate(GameManager.GM.StraightLinePrefab, Vector2.zero, Quaternion.Euler(0, 0, 0));
                                                        straightLines.Add(game);
                                                    }
                                                    else
                                                    {
                                                        game = straightLines[straightLineNum];
                                                        game.SetActive(true);
                                                    }
                                                    straightLineNum++;

                                                    LineRenderer lr = game.GetComponent<LineRenderer>();
                                                    lr.sortingLayerName = "Foreground";
                                                    lr.sortingOrder = 21;
                                                    if (j == 0)
                                                    {
                                                        lr.positionCount = 1;
                                                        lr.SetPosition(0, hit.point + -hit.normal * 0.25f);
                                                        lr.positionCount++;
                                                        lr.SetPosition(1, hit.point + hit.normal * 0.25f);
                                                    }
                                                    else if (j == 1)
                                                    {
                                                        lr.positionCount = 1;
                                                        lr.SetPosition(0, hit.point + -tangent * 0.25f);
                                                        lr.positionCount++;
                                                        lr.SetPosition(1, hit.point + tangent * 0.25f);
                                                    }
                                                }

                                                for (int j = 0; j < 2; j++)
                                                {
                                                    GameObject game = null;
                                                    if (angles.Count >= angleNum)
                                                    {
                                                        game = Instantiate(GameManager.GM.AnglePrefab, hit.point, Quaternion.Euler(0, 0, 0));
                                                        angles.Add(game);
                                                    }
                                                    else
                                                    {
                                                        game = angles[angleNum];
                                                        game.SetActive(true);
                                                        game.transform.position = hit.point;
                                                    }
                                                    angleNum++;

                                                    Circle c = game.GetComponent<Circle>();
                                                    if (j == 0)
                                                    {
                                                        c.GenerateSectorBetweenVectors(-direction, hit.normal, Color.red);
                                                    }
                                                    else if (j == 1)
                                                    {
                                                        c.GenerateSectorBetweenVectors(-hit.normal, refractedDir, Color.blue);
                                                    }
                                                }
                                            }
                                            GameObject gameObject = Instantiate(lightSourcePoint, Vector2.zero, Quaternion.identity);
                                            LightSource ls = gameObject.GetComponent<LightSource>();

                                            ls.isLightSourcePoint = true;
                                            ls.lightInAir = false;
                                            Vector2 spawnPoint = hit.point + refractedDir * 0.01f;
                                            ls.lightColor = lightCol;

                                            ls.DrawLaser(lightCol, spawnPoint, refractedDir);
                                        }
                                    }
                                }
                                goto BREAK;
                            }
                            else
                            {

                                if (!totalReflection)
                                {
                                    if (lightInAir)
                                    {
                                        eta = 1.0f / refractiveIndex;
                                        lightInAir = false;
                                    }
                                    else
                                    {
                                        eta = refractiveIndex / 1.0f;
                                        lightInAir = true;
                                    }
                                }
                                if (TryRefract(direction, hit.normal, eta, out Vector2 refractedDir))
                                {
                                    if (GameManager.GM.educationalMode)
                                    {
                                        Vector2 tangent = new Vector2(hit.normal.y, -hit.normal.x);
                                        for (int j = 0; j < 2; j++)
                                        {
                                            GameObject game = null;
                                            if (straightLines.Count >= straightLineNum)
                                            {
                                                game = Instantiate(GameManager.GM.StraightLinePrefab, Vector2.zero, Quaternion.Euler(0, 0, 0));
                                                straightLines.Add(game);
                                            }
                                            else
                                            {
                                                game = straightLines[straightLineNum];
                                                game.SetActive(true);
                                            }
                                            straightLineNum++;

                                            LineRenderer lr = game.GetComponent<LineRenderer>();
                                            lr.sortingLayerName = "Foreground";
                                            lr.sortingOrder = 21;
                                            if (j == 0)
                                            {
                                                lr.positionCount = 1;
                                                lr.SetPosition(0, hit.point + -hit.normal * 0.25f);
                                                lr.positionCount++;
                                                lr.SetPosition(1, hit.point + hit.normal * 0.25f);
                                            }
                                            else if (j == 1)
                                            {
                                                lr.positionCount = 1;
                                                lr.SetPosition(0, hit.point + -tangent * 0.25f);
                                                lr.positionCount++;
                                                lr.SetPosition(1, hit.point + tangent * 0.25f);
                                            }
                                        }

                                        for (int j = 0; j < 2; j++)
                                        {
                                            GameObject game = null;
                                            if (angles.Count >= angleNum)
                                            {
                                                game = Instantiate(GameManager.GM.AnglePrefab, hit.point, Quaternion.Euler(0, 0, 0));
                                                angles.Add(game);
                                            }
                                            else
                                            {
                                                game = angles[angleNum];
                                                game.SetActive(true);
                                                game.transform.position = hit.point;
                                            }
                                            angleNum++;

                                            Circle c = game.GetComponent<Circle>();
                                            if (j == 0)
                                            {
                                                c.GenerateSectorBetweenVectors(-direction, hit.normal, Color.red);
                                            }
                                            else if (j == 1)
                                            {
                                                c.GenerateSectorBetweenVectors(-hit.normal, refractedDir, Color.blue);
                                            }
                                        }
                                    }
                                    direction = refractedDir;
                                    origin = hit.point - hit.normal * 0.03f;
                                    totalReflection = false;
                                }
                                else // 全反射
                                {
                                    Reflect(hit);
                                    totalReflection = true;
                                }

                                if (isLightSourcePoint)
                                {
                                    lastCol32 = color;
                                    lastDirection = direction;
                                    lastHitPoint = hit.point;
                                    isChanged = true;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    continue;

                BREAK:
                    break;
                }
                else if (hit.collider.gameObject.tag == "LightSource")
                {
                    LightSource ls = hit.collider.gameObject.GetComponent<LightSource>();
                    if (ls.type == Type.Entrance)
                    {
                        if (!(hitLs != null && hitLs == ls))
                            hitLsTime = GameManager.GM.tmr;

                        hitLs = ls;
                        hitLightEntrance = true;
                    }
                    break;
                }
            }
            else
            {
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, origin + direction * maxDistance);
                break;
            }
        }
    }

    private bool TryRefract(Vector2 incomingDir, Vector2 normal, float eta, out Vector2 refractedDir)
    {
        float cosI = Vector2.Dot(-incomingDir, normal);

        float k = 1.0f - eta * eta * (1.0f - cosI * cosI);

        if (k < 0.0f)
        {
            refractedDir = Vector2.zero;
            return false;
        }

        refractedDir = eta * incomingDir + (eta * cosI - Mathf.Sqrt(k)) * normal;
        return true;
    }

    private Vector3 offset;
    public GameObject wire; // 入口と出口を視覚的に結び付ける紐

    public bool wired = false;

    public bool isFirst = true;
    public bool beChosen = false;
    public Vector3 initialPos;

    private void OnMouseDown()
    {
        if (GameManager.GM.stageClear)
            return;

        initialPos = transform.position;
        beChosen = true;

        bool b = true;
        if (GameManager.GM.choseGameObject != null)
        {
            if (GameManager.GM.circles.Count > 0 && type == Type.Exit)
            {
                if (wired)
                {
                    GameManager.GM.choseGameObject = null;
                    return;
                }

                LightSource ls = GameManager.GM.choseGameObject.GetComponent<LightSource>();
                if (ls.wire != null)
                {
                    ls.wire.GetComponent<Wire>().end.GetComponent<LightSource>().wired = false;
                    Destroy(ls.wire);
                }

                AudioManager.AM.Connect();
                wired = true;
                GameObject w = Instantiate(GameManager.GM.wirePrefab, Vector2.zero, Quaternion.identity);
                ls.wire = w;
                ls.wire.GetComponent<Wire>().start = GameManager.GM.choseGameObject;
                ls.wire.GetComponent<Wire>().end = gameObject;

                ls.exitLs = this;
                ls.isFirst = true;
                GameManager.GM.choseGameObject = null;
                b = false;
            }
            else
            {
                Vector3 mouseWorldPos = GetMouseWorldPos();
                offset = transform.position - mouseWorldPos;
            }
        }
        if (b)
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
        if (!beChosen || isFirst)
            return;

        Vector3 newPosition = GetMouseWorldPos() + offset;

        newPosition.z = transform.position.z;

        if (type == Type.Exit)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, -(GridManager.GM.gridWidth - 1) / 2 - 0.25f, (GridManager.GM.gridWidth - 1) / 2 + 0.25f);
            newPosition.y = Mathf.Clamp(newPosition.y, -(GridManager.GM.gridHeight - 1) / 2 - 0.25f, (GridManager.GM.gridHeight - 1) / 2 + 0.25f);
        }
        else
        {
            newPosition.x = Mathf.Clamp(newPosition.x, GameManager.GM.left.transform.position.x + 0.25f, GameManager.GM.right.transform.position.x - 0.25f);
            newPosition.y = Mathf.Clamp(newPosition.y, GameManager.GM.bottom.transform.position.y + 0.25f, GameManager.GM.top.transform.position.y - 0.25f);

            if (newPosition.x > -(GridManager.GM.gridWidth - 1) / 2 - 0.75f &&
                newPosition.x < (GridManager.GM.gridWidth - 1) / 2 + 0.75f &&
                newPosition.y > -(GridManager.GM.gridHeight - 1) / 2 - 0.75f &&
                newPosition.y < (GridManager.GM.gridHeight - 1) / 2 + 0.75f)
                return;
        }

        if (newPosition == initialPos)
            return;

        initialPos = newPosition;
        transform.position = newPosition;
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = -GameManager.GM.gameCamera.gameObject.transform.position.z;
        return GameManager.GM.gameCamera.gameObject.GetComponent<Camera>().ScreenToWorldPoint(mouseScreenPos);
    }

    private void OnDestroy()
    {
        foreach (GameObject s in straightLines)
        {
            Destroy(s);
        }
        foreach (GameObject a in angles)
        {
            Destroy(a);
        }
    }

}
