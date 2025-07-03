using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCrossBow : MonoBehaviour, IColliderHandler
{
	[Header("CrossBow Settings")]
	public int maxHitPoints = 3;
	public float shootDamage = 2f;
	public float shootSpeed = 1.5f; // seconds per shot
	public float shootSpeedBoost = 0.5f; // faster shoot speed when hit
	public float shootBoostDuration = 2f;
	public GameObject bulletPrefab; // Assign NormalBullet prefab
	public float bulletSpeed = 10f;
	public List<string> triggerTags = new List<string> { "Enemy" };
	public Animator animator;
	public Transform shootPoint; // Where bullet spawns

	private int currentHits = 0;
	private bool isDestroyed = false;
	private float shootTimer = 0f;
	private float currentShootSpeed;
	private float shootBoostTimer = 0f;
	private List<Transform> targetsInRange = new List<Transform>();

	void Awake()
	{
		if (animator == null)
			animator = GetComponent<Animator>();
		currentShootSpeed = shootSpeed;
	}

	void Update()
	{
		if (isDestroyed) return;

		// Handle shoot speed boost timer
		if (shootBoostTimer > 0f)
		{
			shootBoostTimer -= Time.deltaTime;
			if (shootBoostTimer <= 0f)
			{
				currentShootSpeed = shootSpeed;
				if (animator != null) animator.speed = 1f;
			}
		}

		// Remove null or dead targets
		targetsInRange.RemoveAll(t => t == null);

		// Attack logic
		if (targetsInRange.Count > 0)
		{
			shootTimer += Time.deltaTime;
			if (shootTimer >= currentShootSpeed)
			{
				shootTimer = 0f;
				Transform target = GetNearestTarget();
				if (target != null)
				{
					ShootAt(target);
				}
			}
		}
		else
		{
			shootTimer = 0f;
			// Remain idle
		}
	}

	private Transform GetNearestTarget()
	{
		float minDist = float.MaxValue;
		Transform nearest = null;
		foreach (var t in targetsInRange)
		{
			if (t == null) continue;
			float dist = (t.position - transform.position).sqrMagnitude;
			if (dist < minDist)
			{
				minDist = dist;
				nearest = t;
			}
		}
		return nearest;
	}

	private void ShootAt(Transform target)
	{
		if (bulletPrefab == null || target == null) return;
		Vector2 shootOrigin = shootPoint != null ? (Vector2)shootPoint.position : (Vector2)transform.position;
		Vector2 targetPos = (Vector2)target.position;
		Vector2 dir = (targetPos - shootOrigin).normalized;
		float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
		// Rotate the crossbow so its +Y axis faces the target
		transform.rotation = Quaternion.Euler(0, 0, -angle);

		GameObject bullet = Instantiate(bulletPrefab, shootOrigin, Quaternion.identity);
		var normalBullet = bullet.GetComponent<NormalBullet>();
        if (normalBullet != null)
        {
            normalBullet.att = shootDamage;
            normalBullet.SetSpeed(dir, bulletSpeed);
            normalBullet.hit_back = 0f;
            normalBullet.SetOwner(gameObject); // Set owner to this crossbow
            normalBullet.trigger_tags = triggerTags; // Ensure it can hit enemies
            normalBullet.AddIgnore(transform); // Ignore self
		}
        else
        {
            // fallback: set rigidbody velocity
            var rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = dir * bulletSpeed;
        }
		if (animator != null) animator.Play("AniCrossBowShoot", 0, 0f);
	}

	public void HandleTriggerEnter2D(Collider2D other)
	{
		if (isDestroyed) return;
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
        // If hit by bullet, speed up shoot speed for a while
        if (collision.gameObject.GetComponent<GunBullet>() != null)
        {
            currentShootSpeed = shootSpeedBoost;
            shootBoostTimer = shootBoostDuration;
            if (animator != null) animator.speed = 2f;
            if (animator != null) animator.Play("AniCrossBowShoot", 0, 0f);
		}
		// Take damage if needed
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

	public void HandleCollisionEnter2D(Collision2D collision)
	{
		// Not needed
	}

	public void HandleCollisionExit2D(Collision2D collision)
	{
		// Not needed
	}

	// Call by animation event or fallback
	public void DestroySelf()
	{
		Destroy(gameObject);
	}
}
