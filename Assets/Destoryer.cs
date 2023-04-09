using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destoryer : MonoBehaviour
{
    public float wait_time;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, wait_time);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
