using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpike : ItemMaster
{
    [Header("Spike Settings")]
    public float damage = 10f;
    public List<string> triggerTags = new List<string> { "Enemy" };
    public Animator animator; // Optional: for hit/destroy animation
    public GameObject fxHit; // Optional: FX on hit
    public Transform fxHitPosition; // Optional: FX spawn position
    private bool isTriggerEnabled = false;

    protected override void Awake()
    {
        base.Awake();
        if (animator == null)
            animator = GetComponent<Animator>();
        if (col2D != null)
            col2D.enabled = false; // Only enable after tween
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isTriggerEnabled || isDestroyed || isInvulnerable) return;
        if (triggerTags == null || triggerTags.Count == 0) return;
        if (!triggerTags.Contains(other.tag)) return;

        StartCoroutine(InvulnerabilityCoroutine());
        currentHits++;

        // Deal damage to enemy
        var pawn = other.GetComponent<PawnMaster>();
        if (pawn != null)
        {
            if (damage >= 1f) GameEvents.instance.HitPawn(damage, pawn, gameObject, GameEvents.DamageType.Normal, transform, 0f, null, "Trap");
        }

        // Play FX
        if (fxHit != null && fxHitPosition != null)
            CombatManager.PlayFx(fxHit, other.ClosestPoint(fxHitPosition != null ? fxHitPosition.position : transform.position), 1.2f);

        // Play hit animation
        if (animator != null)
            animator.Play("AniSpikeOnHit", 0, 0f);

        if (currentHits >= maxHits)
        {
            isDestroyed = true;
            if (animator != null)
                animator.Play("AniCommonOnDestory", 0, 0f);
            else
                DestroySelf();
        }
    }
}
