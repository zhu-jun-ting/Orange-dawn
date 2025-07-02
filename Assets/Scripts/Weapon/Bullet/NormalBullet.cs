using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBullet : GunBullet
{

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

        Destroy(gameObject); // Destroy immediately
    }
}
