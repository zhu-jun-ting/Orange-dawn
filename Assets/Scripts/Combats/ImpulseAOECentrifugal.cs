using System.Collections.Generic;
using UnityEngine;

public class ImpulseAOECentrifugal : ImpulseAOE
{

    [Header("Centrifugal Force Settings")]
    public float centrifugalForce = 20f;

    private HashSet<Rigidbody2D> forceAppliedTo = new HashSet<Rigidbody2D>();

    void FixedUpdate()
    {
        // Find all pawns in range and push them away from the center, but only once per pawn
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, currentRadius);
        foreach (var hit in hits)
        {
            if (hit == null) continue;
            if (!targetTags.Contains(hit.tag)) continue;
            var pawn = hit.GetComponent<Rigidbody2D>();
            if (pawn != null && !forceAppliedTo.Contains(pawn))
            {
                Vector2 dir = (pawn.position - (Vector2)transform.position).normalized;
                float dist = Vector2.Distance(transform.position, pawn.position);
                float force = centrifugalForce * Mathf.Clamp01(1f - dist / maxRadius);
                pawn.AddForce(dir * force, ForceMode2D.Impulse);
                forceAppliedTo.Add(pawn);
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
