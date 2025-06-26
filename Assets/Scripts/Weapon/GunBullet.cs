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
    public List<string> bounce_tags; 
    public float bounce_randomness = 10f; // how much angle the bullet can randomly bounce off walls, 0 means no randomness
    public float inertia = 0f; // how much inertia the bullet has, 0 means no inertia

    public float hit_back = 5f;
    public Gun gun; // the gun that fired this bullet, used for source of damage and other effects
    // all tags included gameobjects will trigger this bullet to hurt
    [Tooltip("Layers that the bullet will bounce off like pinball")]
    public LayerMask bounceLayers;

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
        // if (trigger_tags.Contains(other.tag))
        // {
        //     if (other != null)
        //     {
        //         // Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        //         // other.gameObject.GetComponent<IBuffable>().TakeDamage(att, GameEvents.DamageType.Normal, hit_back, owner.gameObject, gun);
        //         GameObject exp = ObjectPool.Instance.GetObject(explosionPrefab);
        //         exp.transform.position = transform.position;

        //         PawnMaster pawnMaster = other.gameObject.GetComponent<PawnMaster>();
        //         if (pawnMaster != null) 
        //         {
        //             if (owner == PlayerController.instance.gameObject && GameEvents.OnModifyDamage != null) 
        //             {
        //                 att = GameEvents.OnModifyDamage(att); 
        //             } 
        //             GameEvents.instance.HitPawn(att, pawnMaster, gameObject, GameEvents.DamageType.Normal, pawnMaster.gameObject.transform, hit_back, gun);
        //         } 
        //     }
        // }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision is with a trigger tag to deal damage
        if (trigger_tags.Contains(collision.collider.tag))
        {
            if (collision != null)
            {
                // Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                // other.gameObject.GetComponent<IBuffable>().TakeDamage(att, GameEvents.DamageType.Normal, hit_back, owner.gameObject, gun);
                GameObject exp = ObjectPool.Instance.GetObject(explosionPrefab);
                exp.transform.position = transform.position;

                PawnMaster pawnMaster = collision.gameObject.GetComponent<PawnMaster>();
                if (pawnMaster != null) 
                {
                    if (owner == PlayerController.instance.gameObject && GameEvents.OnModifyDamage != null) 
                    {
                        att = GameEvents.OnModifyDamage(att); 
                    } 
                    GameEvents.instance.HitPawn(att, pawnMaster, gameObject, GameEvents.DamageType.Normal, pawnMaster.gameObject.transform, hit_back, gun);
                } 
            }
        }

        // Check for bounce by tag or by layer
        bool shouldBounce = bounce_tags.Contains(collision.collider.tag) || ((bounceLayers.value & (1 << collision.collider.gameObject.layer)) != 0);
        if (shouldBounce)
        {
            Vector2 normal = collision.contacts[0].normal;
            Vector2 incoming = rigidbody.linearVelocity;
            float speed = incoming.magnitude;
            float angle = Random.Range(-bounce_randomness, bounce_randomness);
            Vector2 reflected = Vector2.Reflect(incoming, normal);
            reflected = Quaternion.Euler(0, 0, angle) * reflected;
            if (inertia > 0f)
                speed *= Mathf.Clamp01(1f - inertia * Time.fixedDeltaTime);
            rigidbody.linearVelocity = reflected.normalized * speed;
        }
    }

    void FixedUpdate()
    {
        // Apply inertia damping every physics step
        if (inertia > 0f && rigidbody.linearVelocity.magnitude > 0.01f)
        {
            rigidbody.linearVelocity *= Mathf.Clamp01(1f - inertia * Time.fixedDeltaTime);
        }
    }
}
