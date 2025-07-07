using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ItemLightningTower : ItemMaster, IColliderHandler
{
    [Header("Damage Stats")]
    public float aoeDamage = 10f;
    public float aoeRange = 2f;

    [Header("Lightning Tower Settings")]
    public List<string> triggerTags = new List<string> { "Enemy" };
    public Animator animator;
    public GameObject fxLightningPrefab; // Assign FxLightning prefab
    public GameObject fxSpawnSmoke; // Assign spawn smoke prefab
    public Transform fxSpawnSmokePos; // Where to play spawn smoke
    public float attackRange = 6f;
    public float fadeInDuration = 0.5f;
    public float hitStretch = 1.05f;
    public float hitStretchDuration = 0.15f;
    public Collider2D rangeCollider; // Assign a trigger collider for detection
    private List<Transform> targetsInRange = new List<Transform>();
    private bool isTriggerEnabled = false;

    protected override void Awake()
    {
        base.Awake();
        if (animator == null)
            animator = GetComponent<Animator>();
        if (col2D != null)
            col2D.enabled = false;
        if (rangeCollider != null)
        {
            rangeCollider.enabled = true;
            rangeCollider.isTrigger = true; // Ensure range collider is a trigger
            ColliderToHandle handle = rangeCollider.gameObject.GetComponent<ColliderToHandle>();
            if (handle != null)
            {
                handle.ChangeColliderRange(attackRange);
            }
        }
    }

    protected override void Start()
    {
        base.Start();
        // Fade in and play spawn smoke
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
            sr.DOFade(1f, fadeInDuration).OnComplete(() => {
                if (col2D != null) col2D.enabled = true;
                isTriggerEnabled = true;
            });
        }
        else
        {
            if (col2D != null) col2D.enabled = true;
            isTriggerEnabled = true;
        }
        if (fxSpawnSmoke != null && fxSpawnSmokePos != null)
            CombatManager.PlayFx(fxSpawnSmoke, fxSpawnSmokePos.position, 1.2f);
    }

    public void HandleTriggerEnter2D(Collider2D other)
    {
        if (!isTriggerEnabled || isDestroyed) return;
        if (triggerTags.Contains(other.tag))
        {
            if (!targetsInRange.Contains(other.transform))
                targetsInRange.Add(other.transform);
        }
    }

    public void HandleTriggerExit2D(Collider2D other)
    {
        if (targetsInRange.Contains(other.transform))
            targetsInRange.Remove(other.transform);
    }

    // ItemMaster handles collision filtering and hit counting. Only custom logic here.
    public override void OnHit(Collision2D collision)
    {
        if (isDestroyed) return;
        // If hit by bullet, attack
        if (collision.gameObject.GetComponent<GunBullet>() != null)
        {
            PerformAttack();
            // Stretch feedback
            transform.DOKill();
            transform.DOScaleY(hitStretch, hitStretchDuration).SetLoops(2, LoopType.Yoyo);
        }
        // Destruction is handled by ItemMaster
    }

    public void HandleCollisionEnter2D(Collision2D collision) { }
    public void HandleCollisionExit2D(Collision2D collision) { }

    public void PerformAttack()
    {
        if (targetsInRange.Count == 0 || fxLightningPrefab == null) return;
        // Pick a random target
        Transform target = targetsInRange[Random.Range(0, targetsInRange.Count)];
        // Play lightning FX at target
        GameObject fx = Instantiate(fxLightningPrefab, target.position, Quaternion.identity);
        FxLightning fxScript = fx.GetComponentInChildren<FxLightning>();
        if (fxScript != null)
        {
            fxScript.SpawnAt(target.position, aoeRange, aoeDamage);
        }
        Destroy(fx, 2f); // Destroy FX after 2 seconds
    }

    // DestroySelf is inherited from ItemMaster
}

// (FxLightning is now a separate script. See FxLightning.cs)
