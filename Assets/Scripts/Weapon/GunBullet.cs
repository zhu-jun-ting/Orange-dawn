using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunBullet : MonoBehaviour
{
    [SerializeField] public float att;
    [SerializeField] private GameObject owner;
    public float speed;
    public GameObject explosionPrefab;
    new private Rigidbody2D rigidbody;
    public List<string> trigger_tags; 
    public float hit_back = 0.01f;
    public Gun gun; // the gun that fired this bullet, used for source of damage and other effects
    // all tags included gameobjects will trigger this bullet to hurt

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        // att = 5;
        // owner = GameObject.FindGameObjectWithTag("Player");
    }

    protected virtual void Start()
    {

    }

    public void SetSpeed(Vector2 direction)
    {
        rigidbody.linearVelocity = Normalize(direction) * speed;
    }

    public void SetSpeed(Vector2 direction, float speed)
    {
        rigidbody.linearVelocity = Normalize(direction) * speed;
    }

    public void SetOwner(GameObject owner_) {
        owner = owner_;
    }

    private Vector2 Normalize(Vector2 vec) {
        return vec / (vec.magnitude);
    }

    void Update()
    {

    }

    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (trigger_tags.Contains(other.tag))
        {
            if (other != null) {
                // Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                other.gameObject.GetComponent<IBuffable>().TakeDamage(att, GameEvents.DamageType.Normal, hit_back, owner.gameObject, gun); 
                GameObject exp = ObjectPool.Instance.GetObject(explosionPrefab);
                exp.transform.position = transform.position; 
                
            }
            ObjectPool.Instance.PushObject(gameObject);
        }
    }
    
}
