using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ItemLightningTower : MonoBehaviour, IColliderHandler
{
    [Header("Damage Stats")]
    public float aoeDamage = 10f;
    public float aoeRange = 2f;

    [Header("Lightning Tower Settings")]
    public int maxHitPoints = 3;
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

    private int currentHits = 0;
    private bool isDestroyed = false;
    private Collider2D col2D;
    private List<Transform> targetsInRange = new List<Transform>();
    private bool isTriggerEnabled = false;

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        col2D = GetComponent<Collider2D>();
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

    void Start()
    {
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

    void OnCollisionEnter2D(Collision2D collision)
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
        // Take damage
        currentHits++;
        if (currentHits >= maxHitPoints && !isDestroyed)
        {
            isDestroyed = true;
            if (animator != null)
                animator.Play("AniCommonOnDestory", 0, 0f);
            else
                DestroySelf();
        }
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

    // Called by animation event or fallback
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}

// (FxLightning is now a separate script. See FxLightning.cs)
