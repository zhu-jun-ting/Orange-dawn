using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CardActionSpawnSpikeOnHitWall : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float _triggerProbability = 1f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public float triggerProbability { get => _triggerProbability; set => _triggerProbability = value; }
    public event Action<CardMaster, Transform> OnTrigger;

    [Header("Spike Settings")]
    public GameObject spikePrefab; // Assign in inspector
    public float spikeDamage = 10f; // Damage dealt by each spike
    public int spawnCount = 3; // How many spikes to spawn
    public float spawnRadius = 2f;
    // Removed: public float bounceDuration, jumpPower, numJumps

    [Header("Mana Cost")]
    public int manaCost = 1;

    [Header("Trigger Settings")]
    public float triggerCooldown = 0.5f;

    // Store initial values for reset
    private int initialSpawnCount;
    private float initialSpikeDamage;
    private int initialManaCost;
    private float initialTriggerProbability;

    protected override void Awake()
    {
        initialSpawnCount = spawnCount;
        initialSpikeDamage = spikeDamage;
        initialManaCost = manaCost;
        initialTriggerProbability = triggerProbability;
    }

    private float lastSpikeSpawnTime = -10f;
    public void TriggerAction(CardMaster source = null, Transform location = null)
    {
        GameEvents.instance.TriggerActionCard(source, location);
        if (spikePrefab == null || location == null) return;
        if (!ManaBar.CanCostMana(-manaCost)) return;

        lastSpikeSpawnTime = Time.time;
        if (UnityEngine.Random.value > triggerProbability) return;
        for (int i = 0; i < spawnCount; i++)
        {
            Vector2? validPos = CombatManager.instance != null
                ? CombatManager.instance.TryGetSpawnLocation(location.position, spawnRadius, 8)
                : null;
            Vector2 targetPos;
            if (validPos.HasValue)
                targetPos = validPos.Value;
            else
                targetPos = (Vector2)location.position + UnityEngine.Random.insideUnitCircle * spawnRadius;

            // Instantly spawn at target position (no DOTween animation)
            GameObject spikeObj = Instantiate(spikePrefab, targetPos, Quaternion.identity);
            // Call OnTweenComplete if present
            var spike = spikeObj.GetComponent<ItemSpike>();
            if (spike != null)
            {
                spike.damage = spikeDamage;
            }
        }
        GameEvents.instance.UpdateMana(-manaCost);
    }

    public override void OnCardEnable()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitWall += HandleOnHitWall;
        OnTrigger -= TriggerAction; // Unsubscribe to avoid duplicates
        OnTrigger += TriggerAction; // Subscribe to the trigger event
        base.OnCardEnable();
    }

    public override void OnCardDisable()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitWall -= HandleOnHitWall;
        OnTrigger -= TriggerAction;
        base.OnCardDisable();
    }

    private void HandleOnHitWall(GunBullet bullet, Vector2 hitPosition, GameObject wall)
    {
        // Prevent too frequent spawning
        if (Time.time - lastSpikeSpawnTime < triggerCooldown) return;
        if (UnityEngine.Random.value > triggerProbability) return;
        
        OnTrigger?.Invoke(this, CreateTempTransformAt(hitPosition));
    }

    private Transform CreateTempTransformAt(Vector2 pos)
    {
        GameObject temp = new GameObject("TempSpikeSpawnPoint");
        temp.transform.position = pos;
        Destroy(temp, 2f);
        return temp.transform;
    }

    public override void Reset()
    {
        base.Reset();
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitWall -= HandleOnHitWall;
        OnTrigger -= TriggerAction;
        spawnCount = initialSpawnCount;
        spikeDamage = initialSpikeDamage;
        manaCost = initialManaCost;
        triggerProbability = initialTriggerProbability;
        lastSpikeSpawnTime = -10f;
    }

    public override string GetDescription()
    {
        return $"On bullet hit wall, spawn {spawnCount} spike(s) (Damage: {spikeDamage}) in a circle. Mana: {manaCost}, Chance: {triggerProbability * 100f}%";
    }

    public override bool UpdateNumberValue(NumberType numberType, float value, CardMaster source = null, bool isPermanent = false, bool isMult = false)

    {
        if (IsBuffedFromSource(source, addToList: true, includeSelf: true)) return false;
        base.UpdateNumberValue(numberType, value, source);
        if (numberType == CardMaster.NumberType.Damage)
        {
            spikeDamage += value;
            return true;
        }
        else if (numberType == CardMaster.NumberType.Amount)
        {
            spawnCount += (int)value;
            return true;
        }
        else if (numberType == CardMaster.NumberType.Probability)
        {
            triggerProbability += value;
            triggerProbability = Mathf.Clamp01(triggerProbability);
            return true;
        }
        else if (numberType == CardMaster.NumberType.Mana)
        {
            manaCost += (int)value;
            return true;
        }
        return false;
    }
}
