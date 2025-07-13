using UnityEngine;

public class ItemBomb : ItemMaster
{
    [Header("Bomb Settings")]
    [Tooltip("Explosion prefab to instantiate on detonation")]
    public GameObject explosionPrefab;
    [Tooltip("Maximum damage dealt at the center of the explosion")]
    public float maxDamage = 20f;
    [Tooltip("Explosion radius")]
    public float radius = 3f;

    public override void OnHit(Collision2D collision)
    {
        if (isDestroyed) return;
        base.OnHit(collision);
        // Instantly destroy self and trigger explosion
        Explode();
        Destroy(gameObject);
    }

    private void Explode()
    {
        if (explosionPrefab != null)
        {
            var explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            // Optionally pass parameters to the explosion script if it has one
            var expScript = explosion.GetComponent<ImpulseAOE>();
            if (expScript != null)
            {
                expScript.maxDamage = maxDamage;
                expScript.maxRadius = radius;
            }
        }
        // Optionally: deal damage to nearby objects here if not handled by the explosion prefab
    }
}
