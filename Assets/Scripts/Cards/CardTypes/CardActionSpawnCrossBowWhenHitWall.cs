using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CardActionSpawnCrossBowWhenHitWall : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float _triggerProbability = 1f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public float triggerProbability { get => _triggerProbability; set => _triggerProbability = value; }
    public event Action<CardMaster, Transform> OnTrigger;

    [Header("CrossBow Settings")]
    public GameObject crossBowPrefab; // Assign in inspector
    public float crossBowDamage = 10f; // Damage dealt by each crossbow
    public int spawnCount = 1; // How many crossbows to spawn
    public float spawnRadius = 2f;
    // Removed: public float bounceDuration, jumpPower, numJumps

    [Header("Mana Cost")]
    public int manaCost = 1;

    [Header("Trigger Settings")]
    public float triggerCooldown = 0.5f;

    // Store initial values for reset
    private int initialSpawnCount;
    private float initialCrossBowDamage;
    private int initialManaCost;
    private float initialTriggerProbability;

    protected override void Awake()
    {
        initialSpawnCount = spawnCount;
        initialCrossBowDamage = crossBowDamage;
        initialManaCost = manaCost;
        initialTriggerProbability = triggerProbability;
    }

    private float lastSpawnTime = -10f;
    public void TriggerAction(CardMaster source = null, Transform location = null)
    {
        if (crossBowPrefab == null || location == null) return;
        if (!ManaBar.CanCostMana(-manaCost)) return;

        lastSpawnTime = Time.time;
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
            GameObject crossbowObj = Instantiate(crossBowPrefab, targetPos, Quaternion.identity);
            var crossbow = crossbowObj.GetComponent<ItemCrossBow>();
            if (crossbow != null)
            {
                crossbow.shootDamage = crossBowDamage;
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
        if (Time.time - lastSpawnTime < triggerCooldown) return;
        if (UnityEngine.Random.value > triggerProbability) return;
        lastSpawnTime = Time.time;
        OnTrigger?.Invoke(this, CreateTempTransformAt(hitPosition));
    }

    private Transform CreateTempTransformAt(Vector2 pos)
    {
        GameObject temp = new GameObject("TempCrossBowSpawnPoint");
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
        crossBowDamage = initialCrossBowDamage;
        manaCost = initialManaCost;
        triggerProbability = initialTriggerProbability;
        lastSpawnTime = -10f;
    }

    public override string GetDescription()
    {
        // return $"On bullet hit wall, spawn {spawnCount} crossbow(s) (Damage: {crossBowDamage}). Mana: {manaCost}, Chance: {triggerProbability * 100f}%";
        return GameSettings.AddIcon("Damage: 10");
    }

    public override bool UpdateNumberValue(NumberType numberType, float value, CardMaster source = null, bool isPermanent = false, bool isMult = false)
    {
        if (IsBuffedFromSource(source, addToList: true, includeSelf: true)) return false;
        base.UpdateNumberValue(numberType, value, source);
        if (numberType == CardMaster.NumberType.Damage)
        {
            crossBowDamage += value;
            return true;
        }
        else if (numberType == CardMaster.NumberType.Amount)
        {
            spawnCount += (int)value;
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
