using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class ItemMaster : MonoBehaviour
{
    [Header("Item Common")]
    public int maxHits = 3;
    public int lifetime = -1; // -1 means no lifetime limit, otherwise it's in seconds
    public float invulnerabilityDuration = 1f; // Duration of invulnerability after hit
    public List<string> breakableByTags = new List<string> { "Bullet" }; // Tags that can break this item

    [Header("Item Visuals")]
    public string spawnFxName = "FxSpawn"; // Name of the spawn effect
    public float spawnFxScale = 3f; // Scale of the spawn effect

    protected int currentHits = 0;
    protected bool isDestroyed = false;
    protected Collider2D col2D;
    protected bool isInvulnerable = false;
    private Vector3 originalScale;

    protected virtual void Awake()
    {
        col2D = GetComponent<Collider2D>();
        originalScale = transform.localScale;
    }

    protected virtual void Start()
    {
        // Play spawn FX
        if (!string.IsNullOrEmpty(spawnFxName))
        {
            CombatManager.PlayFx(spawnFxName, transform.position, spawnFxScale);
        }

        // Animate: scale up, then shrink to normal, and fade in all SpriteRenderers
        float fadeDuration = 0.3f;
        float scaleDuration = 0.25f;
        float popScale = 1.25f;
        // Get all SpriteRenderers in self and children
        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in spriteRenderers)
        {
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
        }
        transform.localScale = originalScale * 0.7f;
        // Disable collider during spawn tween
        if (col2D != null) col2D.enabled = false;
        DG.Tweening.Sequence seq = DG.Tweening.DOTween.Sequence();
        seq.Append(transform.DOScale(originalScale * popScale, scaleDuration * 0.7f));
        seq.Append(transform.DOScale(originalScale, scaleDuration * 0.6f));
        foreach (var sr in spriteRenderers)
        {
            seq.Join(sr.DOFade(1f, fadeDuration));
        }
        // Enable collider after tween ends
        seq.OnComplete(() => { if (col2D != null) col2D.enabled = true; });

        if (lifetime > 0)
        {
            Destroy(gameObject, lifetime);
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDestroyed) return;
        if (breakableByTags == null || breakableByTags.Count == 0) return;
        if (!breakableByTags.Contains(collision.collider.tag)) return;
        if (isInvulnerable) return;

        StartCoroutine(InvulnerabilityCoroutine());

        currentHits++;
        if (currentHits < maxHits)
        {
            // Play hit animation
            // TODO: add hit feedback here
        }

        OnHit(collision);

        if (currentHits >= maxHits)
        {
            isDestroyed = true;
            if (GameEvents.instance != null)
            {
                GameEvents.instance.DestroyObject(transform);
            }
            OnItemDestroyed(collision);
            Destroy(gameObject);
        }

    }

    public virtual void OnHit(Collision2D collision)
    {
        // override this method to add custom hit behavior
    }

    public virtual void OnItemDestroyed(Collision2D collision)
    {
        // override this method to add custom destroy behavior
    }

    public System.Collections.IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
    }

    // This method should be called by an Animation Event at the end of the "FragileWallOnDestroy" animation
    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    public virtual void OnDestroy()
    {
        // Find all SpriteRenderers in self and children
        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        float fadeDuration = 0.5f;

        if (spriteRenderers.Length > 0)
        {
            DG.Tweening.Sequence seq = DG.Tweening.DOTween.Sequence();
            foreach (var sr in spriteRenderers)
            {
                seq.Join(sr.DOFade(0f, fadeDuration));
            }
            seq.OnComplete(() => Destroy(gameObject));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Show a local info tip above this item
    public void ShowTip(string tip)
    {
        if (GameEvents.instance != null)
        {
            // Add a small vertical padding (e.g., 0.5 units above the item)
            Vector2 pos = (Vector2)transform.position + new Vector2(0, 0.5f);
            GameEvents.instance.ShowMessage(tip, GameEvents.MessageType.LocalInfo, pos);
        }
    }
}
