


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunBullet : MonoBehaviour, IColliderHandler
{

    [Header("Direction Settings")]
    public bool isDirectional = false; // If true, bullet rotates to face its velocity (x=1 is forward)
    [SerializeField] public float att;
    [SerializeField] private GameObject owner;
    public float speed;
    public GameObject explosionPrefab;

    new private Rigidbody2D rigidbody;
    public List<string> trigger_tags; 
    public List<string> bounce_tags; 
    public float bounce_randomness = 10f; // how much angle the bullet can randomly bounce off walls, 0 means no randomness
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

    // List of transforms to ignore for collision/trigger (including their children)
    protected HashSet<Transform> ignoreTransforms = new HashSet<Transform>();
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
    protected HashSet<string> ignoreTags = new HashSet<string>();
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
        if (pawnMaster != null) GameEvents.instance.HitPawn(AoeDamage, pawnMaster, gameObject, GameEvents.DamageType.Aoe, pawnMaster.gameObject.transform, 0f, null, "AOE");
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
        if (pawnMaster != null) GameEvents.instance.HitPawn(AoeDamage, pawnMaster, gameObject, GameEvents.DamageType.Aoe, pawnMaster.gameObject.transform, 0f, null, "AOE");
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
            // If the collided object's layer is "Wall", trigger GameEvents.HitWall
            if (LayerMask.LayerToName(collision.collider.gameObject.layer) == "Wall" && GameEvents.instance != null)
            {
                GameEvents.instance.HitWall(this, collision.contacts[0].point, collision.collider.gameObject);
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
