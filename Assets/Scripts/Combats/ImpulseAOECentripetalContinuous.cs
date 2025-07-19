using System.Collections.Generic;
using UnityEngine;

public class ImpulseAOECentripetalContinuous : ImpulseAOE
{
    [Header("Centripetal Force Settings (Continuous)")]
    public float centripetalForce = 20f;

    void FixedUpdate()
    {
        // Continuously pull all pawns in range toward the center
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, currentRadius);
        foreach (var hit in hits)
        {
            if (hit == null) continue;
            if (!targetTags.Contains(hit.tag)) continue;
            var pawn = hit.GetComponent<Rigidbody2D>();
            if (pawn != null)
            {
                Vector2 dir = ((Vector2)transform.position - pawn.position).normalized;
                float dist = Vector2.Distance(transform.position, pawn.position);
                float force = centripetalForce * Mathf.Clamp01(1f - dist / maxRadius);
                pawn.AddForce(dir * force, ForceMode2D.Force);
                // Healing callback: find PawnMaster and call
                var pawnMaster = hit.GetComponent<PawnMaster>();
                if (pawnMaster != null && onPawnDamaged != null)
                {
                    float damage = Mathf.Lerp(maxDamage, 0, dist / maxRadius);
                    if (damage >= 1f)
                        onPawnDamaged.Invoke(pawnMaster, damage);
                }
            }
        }
    }
}
