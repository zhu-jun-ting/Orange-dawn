using UnityEngine;
using System.Collections.Generic;

public class ItemPoisonGround : ItemMaster
{
    [Header("PoisonGround Settings")]
    [Tooltip("Damage dealt to pawns per tick")]
    public float damagePerTick = 5f;
    [Tooltip("Seconds between damage ticks")]
    public float tickInterval = 1f;

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
            if (pawn != null && damagePerTick > 0f)
            {
                GameEvents.instance.HitPawn(damagePerTick, pawn, gameObject, GameEvents.DamageType.DotDamage, transform, 0f, null);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;
        if (breakableByTags != null && breakableByTags.Contains(other.tag))
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
