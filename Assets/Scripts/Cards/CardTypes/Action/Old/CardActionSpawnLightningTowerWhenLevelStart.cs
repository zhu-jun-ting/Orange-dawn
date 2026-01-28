using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CardActionSpawnLightningTowerWhenLevelStart : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float _triggerProbability = 1f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public float triggerProbability { get => _triggerProbability; set => _triggerProbability = value; }
    public event Action<CardMaster, Transform> OnTrigger;

    [Header("Lightning Tower Settings")]
    public GameObject lightningTowerPrefab; // Assign in inspector
    public int spawnCount = 1;
    public float lightningDamage = 10f;
    public int manaCost = 1;

    // Store initial values for reset
    private int initialSpawnCount;
    private float initialLightningDamage;
    private float initialTriggerProbability;

    protected override void Awake()
    {
        initialSpawnCount = spawnCount;
        initialLightningDamage = lightningDamage;
        initialTriggerProbability = triggerProbability;
    }

    public void TriggerAction(CardMaster source = null, Transform location = null)
    {
        GameEvents.instance.TriggerActionCard(source, location);
        if (lightningTowerPrefab == null || location == null) return;
        if (!ManaBar.CanCostMana(-manaCost)) return;

        lastTriggerTime = Time.time;
        for (int i = 0; i < spawnCount; i++)
        {
            Vector2? validPos = CombatManager.instance != null
                ? CombatManager.instance.TryGetSpawnLocation(8)
                : null;
            Vector2 targetPos = validPos.HasValue
                ? validPos.Value
                : (Vector2)location.position + UnityEngine.Random.insideUnitCircle * 2f;

            GameObject towerObj = Instantiate(lightningTowerPrefab, targetPos, Quaternion.identity);
            var tower = towerObj.GetComponent<ItemLightningTower>();
            if (tower != null)
            {
                tower.aoeDamage = lightningDamage;
            }
        }
        GameEvents.instance.UpdateMana(-manaCost);
    }

    public override void OnCardEnable()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnLevelStart += HandleOnLevelStart;
        OnTrigger -= TriggerAction; // Unsubscribe to avoid duplicates
        OnTrigger += TriggerAction; // Subscribe to the trigger event
        base.OnCardEnable();
    }

    public override void OnCardDisable()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnLevelStart -= HandleOnLevelStart;
        OnTrigger -= TriggerAction;
        base.OnCardDisable();
    }

    private void HandleOnLevelStart()
    {
        // Prevent too frequent spawning
        if (Time.time - lastTriggerTime < actionCooldown) return;
        if (UnityEngine.Random.value > triggerProbability) return;

        // Spawn at a valid random location using CombatManager's TryGetSpawnLocation
        if (CombatManager.instance == null) return;
        Vector2? pos = CombatManager.instance.TryGetSpawnLocation(8);
        if (!pos.HasValue) return;
        // Create a temporary transform at the chosen location
        GameObject temp = new GameObject("TempLightningTowerSpawnPoint");
        temp.transform.position = pos.Value;
        OnTrigger?.Invoke(this, temp.transform);
        GameObject.Destroy(temp, 2f);
    }

    private float lastTriggerTime = -10f;

    public override void Reset()
    {
        base.Reset();
        if (GameEvents.instance != null)
            GameEvents.instance.OnLevelStart -= HandleOnLevelStart;
        OnTrigger -= TriggerAction;
        spawnCount = initialSpawnCount;
        lightningDamage = initialLightningDamage;
        triggerProbability = initialTriggerProbability;
        lastTriggerTime = -10f;
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), spawnCount, triggerProbability, lightningDamage, (int)manaCost));
    }

    public override bool UpdateNumberValue(NumberType numberType, float value, CardMaster source = null, bool isPermanent = false, bool isMult = false)
    {
        if (IsBuffedFromSource(source, addToList: true, includeSelf: true)) return false;
        base.UpdateNumberValue(numberType, value, source);
        if (numberType == CardMaster.NumberType.Amount)
        {
            spawnCount += (int)value;
            return true;
        }
        else if (numberType == CardMaster.NumberType.Damage)
        {
            lightningDamage += value;
            return true;
        }
        return false;
    }
}
