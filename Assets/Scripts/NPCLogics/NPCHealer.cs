using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCHealer : NPCMaster, IColliderHandler
{
    [Header("Healer Stats")]
    public float healAOERadius = 1.5f;
    public ColliderToHandle colliderToHandle;
    public GameObject healAOEPrefab; // Assign HealAOE prefab in inspector

    private readonly List<PawnMaster> targetsInRange = new List<PawnMaster>();
    private IEnumerator healCoroutine;
    private bool isHealing = false;

    public override void Awake()
    {
        base.Awake();
        curHP = maxHP;
    }

    public override void Start()
    {
        base.Start();
        colliderToHandle.ChangeColliderRange(detectorRange);
        colliderToHandle.SetHandlerObject(this.gameObject);
        targetsInRange.Add(this); // Add self to targetsInRange to allow healing itself if needed
    }

    public override void UpdatePerSecond()
    {
        base.UpdatePerSecond();
        EvaluateState();
    }

    public void HandleTriggerEnter2D(Collider2D other)
    {
        if (!triggerTags.Contains(other.tag)) return;
        PawnMaster pawn = other.GetComponent<PawnMaster>();
        if (pawn != null && !targetsInRange.Contains(pawn))
        {
            targetsInRange.Add(pawn);
            EvaluateState();
        }
    }

    public void HandleTriggerExit2D(Collider2D other)
    {
        if (!triggerTags.Contains(other.tag)) return;
        PawnMaster pawn = other.GetComponent<PawnMaster>();
        if (pawn != null && targetsInRange.Contains(pawn))
        {
            targetsInRange.Remove(pawn);
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
            isHealing = false;
            if (healCoroutine != null) StopCoroutine(healCoroutine);
        }
        else if (s == State.Attacking)
        {
            isHealing = true;
            if (healCoroutine != null) StopCoroutine(healCoroutine);
            healCoroutine = HealLoop();
            StartCoroutine(healCoroutine);
        }
    }

    private void EvaluateState()
    {
        bool hasTargetToHeal = targetsInRange.Exists(p => !p.isFullHealth);
        if (targetsInRange.Count == 0 || !hasTargetToHeal)
        {
            ChangeState(State.Idle);
        }
        else
        {
            ChangeState(State.Attacking);
        }
    }

    private IEnumerator HealLoop()
    {
        while (isHealing)
        {
            List<PawnMaster> healable = targetsInRange.FindAll(p => !p.isFullHealth);
            if (healable.Count == 0)
            {
                ChangeState(State.Idle);
                yield break;
            }
            PawnMaster target = healable[Random.Range(0, healable.Count)];
            SpawnHealAOE(target);
            yield return new WaitForSeconds(attackInterval);
        }
    }

    private void SpawnHealAOE(PawnMaster target)
    {
        if (healAOEPrefab != null && target != null)
        {
            GameObject aoe = Instantiate(healAOEPrefab, target.transform.position, Quaternion.identity);
            HealAOE healAOE = aoe.GetComponent<HealAOE>();
            if (healAOE != null)
            {
                healAOE.maxDamage = damage;
                healAOE.maxRadius = healAOERadius;
                healAOE.targetTags = triggerTags;
            }
        }
    }
}
