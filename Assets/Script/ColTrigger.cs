using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColTrigger : MonoBehaviour
{
    public GameObject currentTriggeredGameObject;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Block") && !other.gameObject.GetComponent<Block>().liftable)
        {
            currentTriggeredGameObject = null;
        }
        else if (currentTriggeredGameObject == null && (other.CompareTag("LightSource") || other.CompareTag("Block")))
        {
            currentTriggeredGameObject = other.gameObject;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (currentTriggeredGameObject == null)
        {
            if (other.CompareTag("LightSource") || other.CompareTag("Block"))
            {
                if (other.CompareTag("Block") && !other.gameObject.GetComponent<Block>().liftable)
                    return;

                currentTriggeredGameObject = other.gameObject;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("LightSource") || other.CompareTag("Block"))
        {
            if (other.CompareTag("Block") && !other.gameObject.GetComponent<Block>().liftable)
                return;

            if (other.gameObject == currentTriggeredGameObject)
                currentTriggeredGameObject = null;
        }
    }
}
