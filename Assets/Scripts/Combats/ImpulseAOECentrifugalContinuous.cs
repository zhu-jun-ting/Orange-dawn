using System.Collections.Generic;
using UnityEngine;

public class ImpulseAOECentrifugalContinuous : ImpulseAOE
{
    [Header("Centrifugal Force Settings (Continuous)")]
    public float centrifugalForce = 20f;

    void FixedUpdate()
    {
        // Continuously push all pawns in range away from the center
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, currentRadius);
        foreach (var hit in hits)
        {
            if (hit == null) continue;
            if (!targetTags.Contains(hit.tag)) continue;
            var pawn = hit.GetComponent<Rigidbody2D>();
            if (pawn != null)
            {
                Vector2 dir = (pawn.position - (Vector2)transform.position).normalized;
                float dist = Vector2.Distance(transform.position, pawn.position);
                float force = centrifugalForce * Mathf.Clamp01(1f - dist / maxRadius);
                pawn.AddForce(dir * force, ForceMode2D.Force);
            }
        }
    }
}
