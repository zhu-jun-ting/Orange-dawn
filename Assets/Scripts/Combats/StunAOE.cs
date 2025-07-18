using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class StunAOE : ImpulseAOE
{
    [Header("Stun DOT Settings")]
    public float stunDamage = 0f;
    public float stunInterval = 0.5f;
    public float stunDuration = 1f;
    public bool stunStackable = false;
    public string stunFxName = "FxShock";
    private float originalSpeed = 1f;

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
                    enemy.AddDot(EnemyMaster.DotType.Shock, stunDamage, stunInterval, stunDuration, stunFxName, stunStackable, _onBeginDot: (e) =>
                    {
                        originalSpeed = e.moveSpeed; // Store original speed
                        e.moveSpeed = 0f; // Apply slow effect
                    }, _onEndDot: (e) =>
                    {
                        // Remove slow effect
                        e.moveSpeed = originalSpeed; // Restore original speed
                    });
                }
            }
            hitObjects.Add(other.gameObject);
        }
    }
}
