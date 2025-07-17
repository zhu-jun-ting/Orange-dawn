using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class ItemHealingGround : ItemMaster
{
    [Header("PoisonGround Settings")]
    [Tooltip("Damage dealt to pawns per tick")]
    public float damagePerTick = 5f;
    [Tooltip("Seconds between damage ticks")]
    public float tickInterval = 1f;
    public List<string> triggerTags = new List<string> { "Player", "NPC" };
    public SpriteRenderer groundRenderer;

    private HashSet<PawnMaster> pawnsInArea = new HashSet<PawnMaster>();
    private float timer = 0f;
    private float lifeTimer = 0f;
    private Collider2D triggerCollider;

    protected override void Awake()
    {
        base.Awake();
        triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    protected override void Start()
    {
        // Don't call base.Start() to avoid spawn FX/collider enable
        timer = 0f;
        lifeTimer = 0f;
        if (groundRenderer != null)
        {
            var c = groundRenderer.color;
            c.a = 0f;
            groundRenderer.color = c;
            // Use DOTween for fade in (SpriteRenderer needs to animate its color)
            DG.Tweening.DOTween.To(
                () => groundRenderer.color.a,
                a => {
                    var col = groundRenderer.color;
                    col.a = a;
                    groundRenderer.color = col;
                },
                1f, 0.5f
            );
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (lifetime > 0f)
        {
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= lifetime)
            {
                Destroy(gameObject);
                return;
            }
        }
        if (timer >= tickInterval)
        {
            timer = 0f;
            DealPoisonDamage();
        }
    }

    private void DealPoisonDamage()
    {
        foreach (var pawn in pawnsInArea)
        {
            if (pawn != null && damagePerTick > 0f && triggerTags.Contains(pawn.gameObject.tag))
            {
                // GameEvents.instance.HitPawn(damagePerTick, pawn, gameObject, GameEvents.DamageType.DotDamage, transform, 0f, null);
                GameEvents.instance?.HealPawn(damagePerTick, pawn, gameObject, transform);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;
        if (breakableByTags != null && triggerTags.Contains(other.tag))
        {
            var pawn = other.GetComponent<PawnMaster>();
            if (pawn != null)
            {
                pawnsInArea.Add(pawn);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == null) return;
        if (breakableByTags != null && breakableByTags.Contains(other.tag))
        {
            var pawn = other.GetComponent<PawnMaster>();
            if (pawn != null && pawnsInArea.Contains(pawn))
            {
                pawnsInArea.Remove(pawn);
            }
        }
    }
}
