using System.Collections.Generic;
using UnityEngine;

public class SlowAOE : ImpulseAOE
{
    [Header("Slow DOT Settings")]
    public float slowDamage = 0f;
    public float slowInterval = 0.5f;
    public float slowDuration = 2f;
    public bool slowStackable = false;
    public string slowFxName = "FxSlow";
    public float slowSpeedModifier = 0.5f; // Amount to slow the target by (0-1)

    public override void OnTriggerEnter2D(Collider2D other)
    {
        if (hitObjects.Contains(other.gameObject)) return;
        if (targetTags.Contains(other.tag))
        {
            float dist = Vector2.Distance(transform.position, other.transform.position);
            float damage = Mathf.Lerp(maxDamage, 0, dist / maxRadius);
            var pawn = other.GetComponent<PawnMaster>();
            if (pawn != null)
            {
                if (damage >= 1f) GameEvents.instance.HitPawn(damage, pawn, gameObject, GameEvents.DamageType.Normal, other.transform, 0f, null);
                var enemy = pawn as EnemyMaster;
                if (enemy != null)
                {
                    enemy.AddDot(EnemyMaster.DotType.Slow, slowDamage, slowInterval, slowDuration, slowFxName, slowStackable, _onBeginDot: (e) =>
                    {
                        e.moveSpeed *= slowSpeedModifier; // Apply slow effect
                    }, _onEndDot: (e) =>
                    {
                        // Remove slow effect
                        e.moveSpeed /= slowSpeedModifier; // Restore original speed
                    });
                }
            }
            hitObjects.Add(other.gameObject);
        }
    }
}
