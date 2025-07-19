using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunBullet : MonoBehaviour, IColliderHandler
{

    [Header("Direction Settings")]
    public bool isDirectional = false; // If true, bullet rotates to face its velocity (x=1 is forward)
    public bool canClone = true; // If true, bullet can be cloned by cloners (like the Bullet Cloner item)
    [SerializeField] public float att;
    [SerializeField] private GameObject owner;
    public float critChance = 0.05f; // Chance to crit, 0 means no crit chance
    public float critDamage = 1.5f; // Damage multiplier on crit, 1 means no extra damage
    public int penetrate = 0; // number of enemies a bullet can penetrate, 0 means no penetration
    public float speed;
    public float speedDamageModifier = 1f; // You can expose this as a public variable if needed
    public GameObject explosionPrefab;

    new private Rigidbody2D rigidbody;
    public List<string> trigger_tags; 
    public List<string> bounce_tags; 
    public float bounce_randomness = 10f; // how much angle the bullet can randomly bounce off walls, 0 means no randomness
    public float bounce_speed_modifier = 3f; // speed of the bullet when it bounces off walls
    public float inertia = 0f; // how much inertia the bullet has, 0 means no inertia
    public Transform Aoe;
    public float AoeDamage = 5f;
    public float lifetime = 10f; // how long the bullet lasts before it is destroyed

    public float hit_back = 5f;
    public Gun gun; // the gun that fired this bullet, used for source of damage and other effects
    // all tags included gameobjects will trigger this bullet to hurt
    [Tooltip("Layers that the bullet will bounce off like pinball")]
    public LayerMask bounceLayers;

    private Collider2D _collider2D;
    private float lastBounceTime = -1f;
    private const float bounceCooldown = 0.5f;

    [Header ("Ignore Settings")]
    // List of transforms to ignore for collision/trigger (including their children)
    protected HashSet<Transform> ignoreTransforms = new HashSet<Transform>();
    // Tags to ignore for collision/trigger events
    // Use HashSet for fast lookup and avoid duplicates
    public List<string> ignoreTags = new List<string>() { "Bullet" }; // Tags to ignore for collision/trigger events

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        _collider2D = GetComponent<Collider2D>();
        if (_collider2D != null)
        {
            _collider2D.enabled = false;
            StartCoroutine(EnableColliderAfterDelay(0.03f));
        }
    }

    private System.Collections.IEnumerator EnableColliderAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_collider2D != null)
            _collider2D.enabled = true;
    }

    protected virtual void Start()
    {
        SetAoe(false); // Disable AOE by default
        Destroy(gameObject, lifetime); // Destroy the bullet after 15 seconds if not used
    }

    /// <summary>
    /// Add a transform (and all its children) to be ignored by this bullet for collision and trigger events.
    /// </summary>
    public void AddIgnore(Transform t)
    {
        if (t == null) return;
        ignoreTransforms.Add(t);
        foreach (Transform child in t.GetComponentsInChildren<Transform>(true))
        {
            ignoreTransforms.Add(child);
        }
    }

    /// <summary>
    /// Add a tag (or comma/semicolon/space separated tags) to be ignored by this bullet for collision and trigger events.
    /// </summary>
    public void AddIgnore(string tags)
    {
        if (string.IsNullOrWhiteSpace(tags)) return;
        var split = tags.Split(new char[] { ',', ';', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (var tag in split)
        {
            ignoreTags.Add(tag.Trim());
        }
    }
    
    public void SetAoe(bool active)
    {
        if (Aoe != null)
        {
            Aoe.gameObject.SetActive(active);
        }
    }

    public void SetAoeSize(float size)
    {
        if (Aoe != null)
        {
            Aoe.localScale = new Vector3(size, size, 1f);
        }
    }

    public bool IsAoeActive()
    {
        return Aoe != null && Aoe.gameObject.activeSelf;
    }

    public void HandleTriggerEnter2D(Collider2D other)
    {
        // Ignore if the collider's transform or any parent is in ignoreTransforms or tag is in ignoreTags
        if (IsIgnored(other.transform) || ignoreTags.Contains(other.tag)) return;
        // Handle collision enter
        PawnMaster pawnMaster = other.gameObject.GetComponent<PawnMaster>();
        if (pawnMaster != null && AoeDamage >= 1f) GameEvents.instance.HitPawn(AoeDamage, pawnMaster, gameObject, GameEvents.DamageType.Aoe, pawnMaster.gameObject.transform, 0f, null, "AOE");
    }

    public void HandleTriggerExit2D(Collider2D other)
    {
        // Handle trigger exit
    }

    public void HandleCollisionEnter2D(Collision2D collision)
    {
        // Ignore if the collider's transform or any parent is in ignoreTransforms or tag is in ignoreTags
        if (IsIgnored(collision.transform) || ignoreTags.Contains(collision.collider.tag)) return;
        // Handle collision enter
        PawnMaster pawnMaster = collision.gameObject.GetComponent<PawnMaster>();
        if (pawnMaster != null && AoeDamage >= 1f) GameEvents.instance.HitPawn(AoeDamage, pawnMaster, gameObject, GameEvents.DamageType.Aoe, pawnMaster.gameObject.transform, 0f, null, "AOE");
    }

    // Helper to check if a transform or any of its parents is in ignoreTransforms
    protected bool IsIgnored(Transform t)
    {
        while (t != null)
        {
            if (ignoreTransforms.Contains(t)) return true;
            t = t.parent;
        }
        return false;
    }


    public void HandleCollisionExit2D(Collision2D collision)
    {
        // Handle collision exit
    }

    public virtual void OnCollisionEnter2D(Collision2D collision)
    {
        // Ignore if the collider's transform or any parent is in ignoreTransforms or tag is in ignoreTags
        if (IsIgnored(collision.transform) || ignoreTags.Contains(collision.collider.tag)) return;

        // Check if the collision is with a trigger tag to deal damage
        if (trigger_tags.Contains(collision.collider.tag))
        {
            if (collision != null)
            {
                GameObject exp = ObjectPool.Instance.GetObject(explosionPrefab);
                exp.transform.position = transform.position;

                PawnMaster pawnMaster = collision.gameObject.GetComponent<PawnMaster>();
                if (pawnMaster != null)
                {
                    if (owner == PlayerController.instance.gameObject && GameEvents.OnModifyDamage != null)
                    {
                        att = GameEvents.OnModifyDamage(att);
                    }

                    // Calculate relative velocity along the direction of impact
                    Rigidbody2D otherRb = collision.collider.attachedRigidbody;
                    Vector2 myVelocity = rigidbody != null ? rigidbody.linearVelocity : Vector2.zero;
                    Vector2 otherVelocity = otherRb != null ? otherRb.linearVelocity : Vector2.zero;
                    Vector2 relativeVelocity = myVelocity - otherVelocity;

                    // Use the collision normal to get the component of relative velocity in the direction of impact
                    Vector2 collisionNormal = collision.contacts.Length > 0 ? collision.contacts[0].normal : Vector2.zero;
                    float impactSpeed = Vector2.Dot(relativeVelocity, -collisionNormal); // negative because normal points out of the surface
                    var speedDamage = Math.Abs(impactSpeed * speedDamageModifier);

                    if (att >= 1f)
                    {
                        float finalDamage = att + speedDamage;
                        bool isCrit = UnityEngine.Random.value < critChance;
                        if (isCrit)
                        {
                            finalDamage = att * critDamage + speedDamage;
                            GameEvents.instance.HitPawn(finalDamage, pawnMaster, owner, GameEvents.DamageType.Crit, pawnMaster.gameObject.transform, hit_back, gun);
                        }
                        else
                        {
                            GameEvents.instance.HitPawn(finalDamage, pawnMaster, owner, GameEvents.DamageType.Normal, pawnMaster.gameObject.transform, hit_back, gun);
                        }
                        
                        // att and speedDamage are combined? 

                        // --- Spawn debris based on relative velocity ---
                        float impactStrength = relativeVelocity.magnitude;
                        int particleCount = Mathf.Clamp(Mathf.RoundToInt(impactStrength * 2f), 5, 20);
                        float scatterForce = Mathf.Clamp(impactStrength, 1f, 10f);
                        DebriManager.ScatterPixels(
                            collision.contacts.Length > 0 ? collision.contacts[0].point : (Vector2)collision.transform.position,
                            -collisionNormal,
                            particleCount: particleCount,
                            scatterForce: scatterForce
                        );
                    }
                    
                }
            }
        }

        // // Check for bounce by tag or by layer
        bool shouldBounce = bounce_tags.Contains(collision.collider.tag) || ((bounceLayers.value & (1 << collision.collider.gameObject.layer)) != 0);
        float now = Time.time;
        if (shouldBounce && (now - lastBounceTime > bounceCooldown))
        {
            // If the collided object's layer is "Wall" or "Object", trigger GameEvents.HitWall
            string layerName = LayerMask.LayerToName(collision.collider.gameObject.layer);
            if ((layerName == "Wall" || layerName == "Object") && GameEvents.instance != null)
            {
                GameEvents.instance.HitWall(this, collision.contacts[0].point, collision.collider.gameObject);

                // --- Spawn debris based on relative velocity ---
                Rigidbody2D otherRb = collision.collider.attachedRigidbody;
                Vector2 myVelocity = rigidbody != null ? rigidbody.linearVelocity : Vector2.zero;
                Vector2 otherVelocity = otherRb != null ? otherRb.linearVelocity : Vector2.zero;
                Vector2 relativeVelocity = myVelocity - otherVelocity;
                Vector2 collisionNormal = collision.contacts.Length > 0 ? collision.contacts[0].normal : Vector2.zero;
                // Debris scatter direction: away from wall (along normal)
                Vector2 scatterDir = -collisionNormal;
                float impactStrength = relativeVelocity.magnitude;
                // Optionally scale particle count/force by impact
                int particleCount = Mathf.Clamp(Mathf.RoundToInt(impactStrength * 2f), 5, 20);
                float scatterForce = Mathf.Clamp(impactStrength, 1f, 10f);
                DebriManager.ScatterPixels(
                    collision.contacts[0].point,
                    scatterDir,
                    particleCount: particleCount,
                    scatterForce: scatterForce
                );
            }
        }
    }

    public void SetSpeed(Vector2 direction)
    {
        if (direction == Vector2.zero || float.IsNaN(direction.x) || float.IsNaN(direction.y))
        {
            rigidbody.linearVelocity = Vector2.zero;
            return;
        }
        rigidbody.linearVelocity = Normalize(direction) * speed;
    }

    public void SetSpeed(Vector2 direction, float speed)
    {
        if (direction == Vector2.zero || float.IsNaN(direction.x) || float.IsNaN(direction.y))
        {
            rigidbody.linearVelocity = Vector2.zero;
            return;
        }
        rigidbody.linearVelocity = Normalize(direction) * speed;
    }

    public void SetOwner(GameObject owner_) {
        owner = owner_;
    }

    private Vector2 Normalize(Vector2 vec) {
        float mag = vec.magnitude;
        if (mag == 0f || float.IsNaN(mag))
            return Vector2.zero;
        return vec / mag;
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        if (isDirectional && rigidbody != null)
        {
            Vector2 vel = rigidbody.linearVelocity;
            if (vel.sqrMagnitude > 0.001f)
            {
                float angle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        // Apply inertia damping every physics step
        if (inertia > 0f && rigidbody.linearVelocity.magnitude > 0.01f)
        {
            rigidbody.linearVelocity *= Mathf.Clamp01(1f - inertia * Time.fixedDeltaTime);
        }
        // Destroy the bullet if it is not moving
        if (rigidbody.linearVelocity.sqrMagnitude < 0.01f)
        {
            Destroy(gameObject);
            if (explosionPrefab != null)
            {
                GameObject exp = ObjectPool.Instance.GetObject(explosionPrefab);
                exp.transform.position = transform.position;
            }
        }
    }
}
