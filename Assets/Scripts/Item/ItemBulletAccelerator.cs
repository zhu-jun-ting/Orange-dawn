using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An item that accelerates bullets in its range by applying a centripetal force (like a gravity well),
/// then throws the bullet out with increased speed. Inherits from ItemMaster for common item logic.
/// </summary>
public class ItemBulletAccelerator : ItemMaster
{
    [Header("Accelerator Settings")]
    public float range = 5f;
    public float gravityStrength = 20f; // How strong the pull is
    public float initialGravityStrength = 10f; // Starting pull strength
    public float maxGravityStrength = 40f; // Max pull strength
    public float spinForce = 15f; // Tangential force to make bullet orbit
    public float outwardForce = 5f; // Outward force to make bullet spiral out
    public float releaseRadius = 7f; // When bullet is this far, release it
    public LayerMask bulletLayer;
    public List<string> bulletTags = new List<string> { "Bullet" };

    private HashSet<Rigidbody2D> bulletsInRange = new HashSet<Rigidbody2D>();
    private Dictionary<Rigidbody2D, float> bulletEntryTime = new Dictionary<Rigidbody2D, float>();
    private Dictionary<Rigidbody2D, float> bulletOrbitProgress = new Dictionary<Rigidbody2D, float>();
    private CircleCollider2D triggerCollider;

    protected override void Awake()
    {
        base.Awake();
        // Setup trigger collider
        triggerCollider = gameObject.AddComponent<CircleCollider2D>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = range;
    }

    protected virtual void FixedUpdate()
    {
        float now = Time.time;
        var toRelease = new List<Rigidbody2D>();
        foreach (var rb in bulletsInRange)
        {
            if (rb == null) continue;
            Vector2 toCenter = (Vector2)transform.position - rb.position;
            float dist = toCenter.magnitude;
            if (dist < 0.01f) continue;

            // Progress: 0 at entry, 1 at releaseRadius
            float progress = Mathf.Clamp01((dist - triggerCollider.radius) / (releaseRadius - triggerCollider.radius));
            bulletOrbitProgress[rb] = progress;

            // Gravity increases as bullet spirals out
            float gravity = Mathf.Lerp(initialGravityStrength, maxGravityStrength, progress);
            Vector2 gravityForce = toCenter.normalized * gravity;
            rb.AddForce(gravityForce);

            // Tangential (spin) force: perpendicular to toCenter
            Vector2 tangent = new Vector2(-toCenter.y, toCenter.x).normalized;
            rb.AddForce(tangent * spinForce);

            // Outward force to spiral out
            rb.AddForce(-toCenter.normalized * outwardForce * progress);

            // Release if outside releaseRadius
            if (dist >= releaseRadius)
            {
                toRelease.Add(rb);
            }
        }
        // Release bullets
        foreach (var rb in toRelease)
        {
            if (rb == null) continue;
            Vector2 fromCenter = (rb.position - (Vector2)transform.position).normalized;
            rb.linearVelocity = fromCenter * Mathf.Max(rb.linearVelocity.magnitude, maxGravityStrength);
            bulletsInRange.Remove(rb);
            bulletEntryTime.Remove(rb);
            bulletOrbitProgress.Remove(rb);
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsBullet(other)) return;
        var rb = other.attachedRigidbody;
        if (rb != null && !bulletsInRange.Contains(rb))
        {
            bulletsInRange.Add(rb);
            bulletEntryTime[rb] = Time.time;
            bulletOrbitProgress[rb] = 0f;
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (!IsBullet(other)) return;
        var rb = other.attachedRigidbody;
        if (rb != null && bulletsInRange.Contains(rb))
        {
            bulletsInRange.Remove(rb);
            bulletEntryTime.Remove(rb);
            bulletOrbitProgress.Remove(rb);
        }
    }

    protected bool IsBullet(Collider2D col)
    {
        if (((1 << col.gameObject.layer) & bulletLayer) == 0) return false;
        if (bulletTags == null || bulletTags.Count == 0) return true;
        return bulletTags.Contains(col.tag);
    }
}
