using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBullet : GunBullet
{

    public override void OnCollisionEnter2D(Collision2D collision)
    {
        // Ignore if the collider's transform or any parent is in ignoreTransforms or tag is in ignoreTags
        if (IsIgnored(collision.transform) || ignoreTags.Contains(collision.collider.tag)) return;

        // Deal damage if trigger_tags match and layer is correct
        if (trigger_tags.Contains(collision.collider.tag) || ((1 << collision.gameObject.layer) & bounceLayers) != 0)
        {
            PawnMaster pawnMaster = collision.gameObject.GetComponent<PawnMaster>();
            if (pawnMaster != null)
            {
                GameEvents.instance.HitPawn(att, pawnMaster, gameObject, GameEvents.DamageType.Normal, pawnMaster.gameObject.transform, hit_back, gun);
            }
            Destroy(gameObject); // Destroy immediately
            if (explosionPrefab != null)
            {
                GameObject exp = ObjectPool.Instance.GetObject(explosionPrefab);
                exp.transform.position = transform.position;
            }
        }
    }


    public void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignore if the collider's transform or any parent is in ignoreTransforms or tag is in ignoreTags
        if (IsIgnored(collision.transform) || ignoreTags.Contains(collision.tag)) return;

        // Deal damage if trigger_tags match
        if (trigger_tags.Contains(collision.tag) || ((1 << collision.gameObject.layer) & bounceLayers) != 0)
        {
            PawnMaster pawnMaster = collision.gameObject.GetComponent<PawnMaster>();
            if (pawnMaster != null)
            {
                GameEvents.instance.HitPawn(att, pawnMaster, gameObject, GameEvents.DamageType.Normal, pawnMaster.gameObject.transform, hit_back, gun);
            }
            Destroy(gameObject); // Destroy immediately
            if (explosionPrefab != null)
            {
                GameObject exp = ObjectPool.Instance.GetObject(explosionPrefab);
                exp.transform.position = transform.position;
            }
        }
        
    }
}
