using UnityEngine;
using System.Collections.Generic;

public class ItemCloneBox : ItemMaster
{
    [Header("CloneBox Settings")]
    [Tooltip("Max random offset for clone position")] public float maxOffset = 0.5f;
    [Tooltip("Max angle deviation for clone direction")] public float maxAngleDeviation = 20f;

    public override void OnHit(Collision2D collision)
    {
        base.OnHit(collision);
        var bullet = collision.gameObject;
        var gunBullet = bullet.GetComponent<GunBullet>();
        if (gunBullet == null || !gunBullet.canClone) return;

        // Clone bullet
        GameObject clone = ObjectPool.Instance.GetObject(bullet, bullet.transform.position + (Vector3)GetRandomOffset(), bullet.transform.rotation);
        if (clone == null) return;

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        Rigidbody2D cloneRb = clone.GetComponent<Rigidbody2D>();
        if (rb != null && cloneRb != null)
        {
            Vector2 baseDir = rb.linearVelocity.normalized;
            float baseSpeed = rb.linearVelocity.magnitude;
            Vector2 cloneDir = Quaternion.Euler(0, 0, Random.Range(-maxAngleDeviation, maxAngleDeviation)) * baseDir;
            cloneRb.linearVelocity = cloneDir * baseSpeed;
        }
        // Set canClone to false for both
        gunBullet.canClone = false;
        var cloneGunBullet = clone.GetComponent<GunBullet>();
        if (cloneGunBullet != null) cloneGunBullet.canClone = false;
    }

    private Vector2 GetRandomOffset()
    {
        return new Vector2(Random.Range(-maxOffset, maxOffset), Random.Range(-maxOffset, maxOffset));
    }
}