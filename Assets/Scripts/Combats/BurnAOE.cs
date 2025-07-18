using System.Collections.Generic;
using UnityEngine;

public class BurnAOE : ImpulseAOE
{
    [Header("Burn DOT Settings")]
    public float burnDamage = 5f;
    public float burnInterval = 0.5f;
    public float burnDuration = 2f;
    public bool burnStackable = false;
    public string burnFxName = "FxBurn";

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
                    enemy.AddDot(EnemyMaster.DotType.Burn, burnDamage, burnInterval, burnDuration, burnFxName, burnStackable);
                }
            }
            hitObjects.Add(other.gameObject);
        }
    }
}
