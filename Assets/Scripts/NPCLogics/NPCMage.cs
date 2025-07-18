using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMage : NPCMaster, IColliderHandler
{
    [Header("Mage Stats")]
    public GameObject fxLightningPrefab; // Assign FxLightning prefab in inspector
    public float aoeRange = 1.5f;
    public float aoeDamage = 10f;
    public ColliderToHandle colliderToHandle;

    [Header("Mage Charges")]
    public int chargeMaxChain = 3; // Assign Charge prefab in inspector

    private readonly List<EnemyMaster> targetsInRange = new List<EnemyMaster>();
    private IEnumerator attackCoroutine;
    private bool isAttacking = false;

    public override void Start()
    {
        base.Start();
        colliderToHandle.ChangeColliderRange(detectorRange);
        colliderToHandle.SetHandlerObject(this.gameObject);
    }

    public override void UpdatePerSecond()
    {
        base.UpdatePerSecond();
        EvaluateState();
    }

    public void HandleTriggerEnter2D(Collider2D other)
    {
        if (!triggerTags.Contains(other.tag) || other.tag != "Enemy") return;
        EnemyMaster enemy = other.GetComponent<EnemyMaster>();
        if (enemy != null && !targetsInRange.Contains(enemy))
        {
            targetsInRange.Add(enemy);
            EvaluateState();
        }
    }

    public void HandleTriggerExit2D(Collider2D other)
    {
        if (!triggerTags.Contains(other.tag) || other.tag != "Enemy") return;
        EnemyMaster enemy = other.GetComponent<EnemyMaster>();
        if (enemy != null && targetsInRange.Contains(enemy))
        {
            targetsInRange.Remove(enemy);
            EvaluateState();
        }
    }

    public void HandleCollisionEnter2D(Collision2D collision) { }
    public void HandleCollisionExit2D(Collision2D collision) { }

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

    private void EvaluateState()
    {
        if (targetsInRange.Count == 0)
        {
            ChangeState(State.Idle);
        }
        else
        {
            ChangeState(State.Attacking);
        }
    }

    private IEnumerator AttackLoop()
    {
        while (isAttacking)
        {
            if (targetsInRange.Count == 0)
            {
                ChangeState(State.Idle);
                yield break;
            }
            EnemyMaster target = targetsInRange[Random.Range(0, targetsInRange.Count)];
            SpawnLightningAOE(target);
            yield return new WaitForSeconds(attackInterval);
        }
    }

    private void SpawnLightningAOE(EnemyMaster target)
    {
        if (fxLightningPrefab == null || target == null) return;
        GameObject fx = Instantiate(fxLightningPrefab, target.transform.position, Quaternion.identity);
        FxLightning fxScript = fx.GetComponentInChildren<FxLightning>();
        if (fxScript != null)
        {
            fxScript.SpawnAt(target.transform.position, aoeRange, aoeDamage);
        }
        Destroy(fx, 2f);
    }

    // Charge skill: shoot a lightning chain
    public override void OnStartCharge()
    {
        base.OnStartCharge();
        CombatManager.instance?.ShootLightningChain(transform, _damage: damage, _maxChain: chargeMaxChain);
    }

    public override void OnEndCharge()
    {
        base.OnEndCharge();
        CombatManager.instance?.ShootLightningChain(transform, _damage: damage, _maxChain: chargeMaxChain);
    }
}
