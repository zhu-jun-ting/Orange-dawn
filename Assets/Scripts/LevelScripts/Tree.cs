using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    void OnCollisionEnter(Collision c) {
        
        float force = 3f;
        Vector3 dir = c.contacts[0].point - transform.position;
        dir = -dir.normalized;
        
        GetComponent<Rigidbody>().AddForce( dir * force );

    }
}
