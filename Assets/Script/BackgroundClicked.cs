using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundClicked : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (GameManager.GM.choseGameObject != null)
        {
            if (GameManager.GM.choseGameObject.transform.childCount > 0 &&
                GameManager.GM.choseGameObject.GetComponent<Block>())
            {
                ClickJudge cj = GameManager.GM.choseGameObject.transform.GetChild(0).gameObject.GetComponent<ClickJudge>();
                cj.isFirst = true;
            }

            if (GameManager.GM.choseGameObject.GetComponent<LightSource>())
            {
                GameManager.GM.choseGameObject.GetComponent<LightSource>().beChosen = false;
                LightSource ls = GameManager.GM.choseGameObject.GetComponent<LightSource>();
                ls.isFirst = true;
                if (ls.wire != null && GameManager.GM.circles.Count > 0)
                {
                    ls.wire.GetComponent<Wire>().end.GetComponent<LightSource>().wired = false;
                    Destroy(ls.wire);
                    ls.wire = null;
                    ls.exitLs = null;
                }
            }
            else if (GameManager.GM.choseGameObject.GetComponent<Block>())
            {
                Block b = GameManager.GM.choseGameObject.GetComponent<Block>();
                b.isFirst = true;
            }
            GameManager.GM.choseGameObject = null;
        }
    }
}
