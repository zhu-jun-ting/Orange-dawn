using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class FragileWall : ItemMaster
{
    [Header("Fragile Wall Settings")]
    public Animator animator; // Assign in inspector or get in Awake
    public GameObject fxSmoke; // Optional: GameObject for smoke effect
    public Transform fxSmokePosition; // Optional: Transform for smoke effect position


    protected override void Awake()
    {
        base.Awake();
        if (animator == null)
            animator = GetComponent<Animator>();
        if (col2D != null)
            col2D.enabled = false;
    }

    public override void OnHit(Collision2D collision)
    {
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
}
