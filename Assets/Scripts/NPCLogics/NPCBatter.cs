using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCBatter : NPCMaster, IColliderHandler
{
    [Header("Batter Stats")]
    public float hitBackEnemy = 10f;
    public float hitBackBullet = 10f;
    public ColliderToHandle colliderToHandle;
    public Transform batSpawnPoint; // Assign in inspector as child object or set in code
    public GameObject batSword; // Assign in inspector as child object or set in code


    private Bat batScript;
    private Transform target;
    private IEnumerator attackCoroutine;
    private bool isAttacking = false;
    private readonly List<Collider2D> targetsInRange = new List<Collider2D>();

    public override void Awake()
    {
        base.Awake();
        curHP = maxHP;
        if (batSword != null)
        {
            batSword.gameObject.SetActive(false);
        }
    }

    public override void Start()
    {
        base.Start();
        colliderToHandle.ChangeColliderRange(detectorRange);
        colliderToHandle.SetHandlerObject(this.gameObject);
        // Find Bat script in batSpawnPoint children and set its attack
        if (batSpawnPoint != null)
        {
            batScript = batSpawnPoint.GetComponentInChildren<Bat>(true);
            if (batScript != null)
            {
                batScript.attackPower = base.damage;
            }
        }
    }

    public void HandleTriggerEnter2D(Collider2D other)
    {
        if (!triggerTags.Contains(other.tag)) return;
        if (!targetsInRange.Contains(other))
        {
            targetsInRange.Add(other);
            if (state == State.Idle)
            {
                target = other.transform;
                ChangeState(State.Attacking);
            }
        }
    }

    public void HandleTriggerExit2D(Collider2D other)
    {
        if (!triggerTags.Contains(other.tag)) return;
        if (targetsInRange.Contains(other))
        {
            targetsInRange.Remove(other);
            if (targetsInRange.Count == 0)
            {
                ChangeState(State.Idle);
            }
            else if (target != null && target.Equals(other.transform))
            {
                // Pick a new target from those still in range
                target = targetsInRange[0]?.transform;
            }
        }
    }

    public void HandleCollisionEnter2D(Collision2D collision)
    {
        // Implement logic if needed, or leave empty if not used
    }

    public void HandleCollisionExit2D(Collision2D collision)
    {
        // Implement logic if needed, or leave empty if not used
    }

    private float lastStateChangeTime = -1f;
    private const float stateChangeCooldown = 1f;

    public override void ChangeState(State s)
    {
        if (s == state) return;
        if (Time.time - lastStateChangeTime < stateChangeCooldown) return;
        lastStateChangeTime = Time.time;

        base.ChangeState(s);
        if (s == State.Idle)
        {
            target = null;
            isAttacking = false;
            if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        }
        else if (s == State.Attacking)
        {
            isAttacking = true;
            if (attackCoroutine != null) StopCoroutine(attackCoroutine);
            attackCoroutine = AttackLoop();
            StartCoroutine(attackCoroutine);
        }
    }

    private IEnumerator AttackLoop()
    {
        while (isAttacking)
        {
            if (targetsInRange.Count == 0 || target == null)
            {
                ChangeState(State.Idle);
                yield break;
            }
            // Rotate bat to face target
            Vector2 dir = (target.position - transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (batSpawnPoint != null)
                batSpawnPoint.rotation = Quaternion.Euler(0, 0, angle);
            // Activate bat (bat will auto deactivate after attack)
            if (batSword != null && !batSword.activeSelf)
            {
                batSword.SetActive(true);
                Bat bat = batSword.GetComponent<Bat>();
                if (bat != null)
                {
                    bat.attackPower = damage;
                    bat.hitBackEnemy = hitBackEnemy;
                    bat.hitBackBullet = hitBackBullet;
                }
            }
            SoundManager.PlaySFX("Whoosh");
            yield return new WaitForSeconds(attackInterval);
        }
    }

    // batter skill: increase hit back power
    public override void OnStartCharge()
    {
        base.OnStartCharge();
        hitBackBullet *= 1.5f;
        hitBackEnemy *= 1.5f;
    }

    public override void OnEndCharge()
    {
        base.OnEndCharge();
        hitBackBullet /= 1.5f;
        hitBackEnemy /= 1.5f;
    }
}
