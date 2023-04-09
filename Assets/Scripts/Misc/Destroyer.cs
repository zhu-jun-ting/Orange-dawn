using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyer : MonoBehaviour
{

    [Header("parameters")]
    public float waitSecondsToDestory;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, waitSecondsToDestory);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
