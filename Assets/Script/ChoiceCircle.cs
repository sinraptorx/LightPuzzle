using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceCircle : MonoBehaviour
{
    public GameObject target;

    void Start()
    {
        
    }

    void Update()
    {
        transform.position = target.transform.position;
    }
}
