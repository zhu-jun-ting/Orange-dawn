using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    [Header("params")]
    public List<string> trigger_tags; 
    // all tags included gameobjects will trigger this bullet to hurt

    public float damage;
    // the damage deal to the gameobject that can take damage

    public Vector2 direction;
    // the direction bullet is going to fly

    public float speed;


    // internal vars

    private Rigidbody2D rb;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, 10f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        rb.linearVelocity = direction * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (trigger_tags.Contains(other.tag)) {
            // TODO: adjust the damagetype
            if (other.gameObject.GetComponent<IBuffable>() != null) {
                other.gameObject.GetComponent<IBuffable>().TakeDamage(damage, GameEvents.DamageType.Normal, 0f, transform);
            }
            Destroy(gameObject);
        }
    }
}
