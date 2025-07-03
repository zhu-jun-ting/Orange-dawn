using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class FragileWall : MonoBehaviour
{
    [Header("Fragile Wall Settings")]
    public int maxHits = 3;
    public float invulnerabilityDuration = 1f; // Duration of invulnerability after hit
    public List<string> breakableByTags; // Tags that can break this wall
    public Animator animator; // Assign in inspector or get in Awake
    public GameObject fxSmoke; // Optional: GameObject for smoke effect
    public Transform fxSmokePosition; // Optional: Transform for smoke effect position

    private int currentHits = 0;
    private bool isDestroyed = false;
    private Collider2D col2D;
    private bool isInvulnerable = false;

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        col2D = GetComponent<Collider2D>();
        if (col2D != null)
            col2D.enabled = false;
    }

    void Start()
    {

    }

    // Called when DOTween jump is complete
    public void OnTweenComplete()
    {
        if (col2D != null) col2D.enabled = true;
        if (fxSmoke != null && fxSmokePosition != null) CombatManager.PlayFx(fxSmoke, fxSmokePosition.position, 1.8f);
        // Add any additional logic here if needed
    }

    private void OnCollisionEnter2D(Collision2D collision)
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
            if (animator != null)
                animator.Play("FragileWallOnHit", 0, 0f);
        }
        else
        {
            // Play destroy animation and destroy after animation ends
            isDestroyed = true;
            if (animator != null)
                animator.Play("FragileWallOnDestroy", 0, 0f);
            // Wait for animation event to call DestroySelf()
        }
    }

    private System.Collections.IEnumerator InvulnerabilityCoroutine()
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
}
