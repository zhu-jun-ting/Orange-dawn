using System.Collections.Generic;
using UnityEngine;

public class PoisonAOE : ImpulseAOE
{
    [Header("Poison DOT Settings")]
    public float poisonDamage = 3f;
    public float poisonInterval = 0.5f;
    public float poisonDuration = 3f;
    public bool poisonStackable = false;
    public string poisonFxName = "FxPoison";

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
                    enemy.AddDot(EnemyMaster.DotType.Poison, poisonDamage, poisonInterval, poisonDuration, poisonFxName, poisonStackable);
                }
            }
            hitObjects.Add(other.gameObject);
        }
    }
}
