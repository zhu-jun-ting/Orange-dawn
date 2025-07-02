using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeBullet : GunBullet
{
    [Header("Explode Bullet Settings")]
    public float destroyDelay = 0.1f; // Time to destroy after animation
    public GameObject explosionFx; // Assign in inspector if you want a custom explosion effect
    public float explosionScale = 1f; // Scale of the explosion effect
    public float explosionDamage = 10f; // Damage dealt by the explosion

    public override void OnCollisionEnter2D(Collision2D collision)
    {
        // Deal damage if trigger_tags match
        if (trigger_tags.Contains(collision.collider.tag))
        {
            PawnMaster pawnMaster = collision.gameObject.GetComponent<PawnMaster>();
            if (pawnMaster != null)
            {
                GameEvents.instance.HitPawn(att, pawnMaster, gameObject, GameEvents.DamageType.Normal, pawnMaster.gameObject.transform, hit_back, gun);
            }
        }

        // Play explosion effect and destroy immediately
        if (explosionFx != null)
        {
            GameObject explosion = Instantiate(explosionFx, transform.position, Quaternion.identity);
            ImpulseAOE impulseAOE = explosion.GetComponent<ImpulseAOE>();
            if (impulseAOE != null)
            {
                impulseAOE.maxDamage = explosionDamage;
            }
            CombatManager.PlayFx(explosion, transform.position, explosionScale, 1f);
        }
        ExplodeAndDestroy();
    }

    private void ExplodeAndDestroy()
    {
        Destroy(gameObject); // Destroy immediately
    }
}
