using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    public Vector2 pos = Vector2.zero;
    [SerializeField] private float speed;
    [SerializeField] private float expandSpeed;

    public float size = 15;
    public float sizeLimit;

    private float tmr = 0;
    private bool isSoundFirst = true;
    void LateUpdate()
    {
        if (GameManager.GM.stageStarted)
        {
            if (GameManager.GM.stageClear)
            {
                transform.position = Vector3.MoveTowards(
            transform.position,
            new Vector3(0, 0, transform.position.z),
            5.0f * Time.deltaTime
        );

                size += 5.0f * Time.deltaTime;
                GetComponent<Camera>().orthographicSize = size;

                if (size > sizeLimit && Vector3.Distance(transform.position, new Vector3(0, 0, transform.position.z)) < 0.3f)
                {
                    if (isSoundFirst)
                    {
                        AudioManager.AM.StageClear();
                        isSoundFirst = false;
                    }

                    tmr += Time.deltaTime;
                    if (tmr > 3.0f)
                    {
                        GameManager.GM.StageEnd(false);
                    }
                }

                if (size > sizeLimit)
                    size = sizeLimit;

            }
            else
            {
                Vector2 vec = Vector2.zero;

                if (Input.GetKey(KeyCode.W)) vec.y += 1;
                if (Input.GetKey(KeyCode.A)) vec.x -= 1;
                if (Input.GetKey(KeyCode.S)) vec.y -= 1;
                if (Input.GetKey(KeyCode.D)) vec.x += 1;

                vec = vec.normalized;

                pos += speed * vec * Time.deltaTime;
                pos.x = Mathf.Clamp(pos.x, GameManager.GM.left.transform.position.x + 0.25f, GameManager.GM.right.transform.position.x - 0.25f);
                pos.y = Mathf.Clamp(pos.y, GameManager.GM.bottom.transform.position.y + 0.25f, GameManager.GM.top.transform.position.y - 0.25f);

                transform.position = new Vector3(pos.x, pos.y, transform.position.z);

                if (Input.GetKey(KeyCode.Q))
                {
                    size += expandSpeed * Time.deltaTime;
                    if (size > sizeLimit)
                        size = sizeLimit;
                    GetComponent<Camera>().orthographicSize = size;
                }
                if (Input.GetKey(KeyCode.E))
                {
                    size -= expandSpeed * Time.deltaTime;
                    if (size < 2)
                        size = 2;
                    GetComponent<Camera>().orthographicSize = size;
                }
            }
        }
        else
        {
            tmr = 0;
            isSoundFirst = true;
            transform.position = transform.position = new Vector3(0, 0, transform.position.z);
        }
    }

}
