using System.Collections.Generic;
using UnityEngine;

public class ItemBulletCloner : ItemMaster
{
    [Header("Bullet Cloner Settings")]
    public int cloneAmount = 2; // Number of clones per bullet
    public float triggerRadius = 2f; // Radius to trigger cloning
    public float cloneAngleSpread = 15f; // Degrees of spread for clones
    public LayerMask bulletLayer;
    public List<string> bulletTags = new List<string> { "Bullet" };

    private HashSet<GameObject> alreadyCloned = new HashSet<GameObject>();
    private CircleCollider2D triggerCollider;

    protected override void Awake()
    {
        base.Awake();
        triggerCollider = gameObject.AddComponent<CircleCollider2D>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = triggerRadius; // Set as needed
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsBullet(other)) return;
        if (alreadyCloned.Contains(other.gameObject)) return; // Prevent multiple clones of the same bullet
        var bullet = other.gameObject;
        if (bullet.GetComponent<GunBullet>() != null && !bullet.GetComponent<GunBullet>().canClone) return; // Skip if bullet cannot be cloned

        alreadyCloned.Add(other.gameObject); // Mark this bullet as cloned to prevent further cloning
        var cd = bullet.GetComponent<BulletCloneCooldown>();
        if (cd == null)
        {
            cd = bullet.AddComponent<BulletCloneCooldown>();
            cd.cooldown = 1f; // default cooldown
        }
        int clonerId = this.GetInstanceID();
        if (!cd.CanClone(clonerId)) return;
        cd.SetCloned(clonerId);
        CloneBullet(bullet);
    }

    private void CloneBullet(GameObject bullet)
    {
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb == null) return;
        Vector2 baseDir = rb.linearVelocity.normalized;
        float baseSpeed = rb.linearVelocity.magnitude;
        float angleStep = cloneAmount > 1 ? cloneAngleSpread / (cloneAmount - 1) : 0f;
        float startAngle = -cloneAngleSpread / 2f;
        for (int i = 0; i < cloneAmount; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector2 dir = Quaternion.Euler(0, 0, angle) * baseDir;
            GameObject clone = Instantiate(bullet, bullet.transform.position, bullet.transform.rotation);
            // Mark as cloned so it won't be cloned again
            if (clone.GetComponent<BulletClonedFlag>() == null)
                clone.AddComponent<BulletClonedFlag>();
            if (clone.GetComponent<BulletCloneCooldown>() == null)
            {
                clone.AddComponent<BulletCloneCooldown>();
                var cd = clone.GetComponent<BulletCloneCooldown>();
                cd.cooldown = 1f; // default cooldown
                cd.SetCloned(this.GetInstanceID()); // Mark this cloner as cloned
            }
            // Set velocity
            var cloneRb = clone.GetComponent<Rigidbody2D>();
            if (cloneRb != null)
                cloneRb.linearVelocity = dir * baseSpeed;
        }
    }

    private bool IsBullet(Collider2D col)
    {
        if (((1 << col.gameObject.layer) & bulletLayer) == 0) return false;
        if (bulletTags == null || bulletTags.Count == 0) return true;
        return bulletTags.Contains(col.tag);
    }
}

// Helper component to flag bullets that are already clones (legacy, not used)
public class BulletClonedFlag : MonoBehaviour { }

// Helper component to track clone cooldown per bullet
public class BulletCloneCooldown : MonoBehaviour
{
    // Universal last clone time for all cloners
    public float lastCloneTime = -999f;
    public float cooldown = 1f;
    // Track which cloner IDs have already cloned this bullet
    private HashSet<int> clonedByClonerIds = new HashSet<int>();

    public bool CanClone(int clonerId)
    {
        // Only allow if this cloner hasn't cloned this bullet yet
        return !clonedByClonerIds.Contains(clonerId) && Time.time - lastCloneTime >= cooldown;
    }
    public void SetCloned(int clonerId)
    {
        clonedByClonerIds.Add(clonerId);
        lastCloneTime = Time.time;
    }
}
