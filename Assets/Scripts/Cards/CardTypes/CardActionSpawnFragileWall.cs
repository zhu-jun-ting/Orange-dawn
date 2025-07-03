using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CardActionSpawnFragileWall : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float _triggerProbability = 1f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public float triggerProbability { get => _triggerProbability; set => _triggerProbability = value; }
    public event Action<CardMaster, Transform> OnTrigger;

    [Header("Fragile Wall Settings")]
    public GameObject fragileWallPrefab; // Assign in inspector
    public GameObject impulseAOEPrefab; // Assign in inspector
    public float spawnRadius = 2f;
    public int spawnCount = 3;
    // public float spawnChance = 1f; // (Removed, use triggerProbability)
    public float spawnDamage = 10f;
    public float impulseRadius = 1f;
    public float impulseDuration = 0.3f;
    public float impulseMaxDamage = 10f;
    public Ease bounceEase = Ease.OutBounce;
    public float bounceDuration = 0.5f;
    public float jumpPower = 1.5f; // world units, adjust as needed
    public int numJumps = 2;

    [Header("Mana Cost")]
    public int manaCost = 1;

    [Header("Trigger Settings")]
    public float triggerCooldown = 0.5f;

    // Store initial values for reset
    private int initialSpawnCount;
    private float initialSpawnDamage;
    private float initialTriggerProbability;


    protected override void Awake()
    {
        initialSpawnCount = spawnCount;
        initialSpawnDamage = spawnDamage;
        initialTriggerProbability = triggerProbability;
    }


    private float lastWallSpawnTime = -10f;
    public void TriggerAction(CardMaster source = null, Transform location = null)
    {
        // location: where to spawn the walls (center)
        if (fragileWallPrefab == null || location == null) return;
        // Prevent too frequent wall spawning
        if (Time.time - lastWallSpawnTime < triggerCooldown) return;
        lastWallSpawnTime = Time.time;
        // Mana cost check
        if (!ManaBar.CanCostMana(-manaCost)) return;
        if (UnityEngine.Random.value > triggerProbability) return;
        for (int i = 0; i < spawnCount; i++)
        {
            // Try to get a valid random spawn location inside the circle
            Vector2? validPos = CombatManager.instance != null
                ? CombatManager.instance.TryGetSpawnLocation(location.position, spawnRadius, 8)
                : null;
            Vector2 targetPos;
            if (validPos.HasValue)
                targetPos = validPos.Value;
            else
                targetPos = (Vector2)location.position + UnityEngine.Random.insideUnitCircle * spawnRadius;

            GameObject wallObj = Instantiate(fragileWallPrefab, location.position, Quaternion.identity);
            float duration = bounceDuration > 0 ? bounceDuration : 0.5f;
            wallObj.transform.DOJump(targetPos, jumpPower, numJumps, duration, false)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    // Call FragileWall.OnTweenComplete if present
                    var fragileWall = wallObj.GetComponent<FragileWall>();
                    if (fragileWall != null)
                    {
                        fragileWall.OnTweenComplete();
                    }
                    // Spawn ImpulseAOE at the final position
                    if (impulseAOEPrefab != null)
                    {
                        GameObject aoe = Instantiate(impulseAOEPrefab, wallObj.transform.position, Quaternion.identity);
                        var impulse = aoe.GetComponent<ImpulseAOE>();
                        if (impulse != null)
                        {
                            impulse.maxRadius = impulseRadius;
                            impulse.duration = impulseDuration;
                            impulse.maxDamage = spawnDamage;
                        }
                    }
                });
        }
        // Deduct mana cost
        GameEvents.instance.UpdateMana(-manaCost);
    }

    public override void OnCardEnable()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitWall += HandleOnHitWall;
        base.OnCardEnable();
    }

    public override void OnCardDisable()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitWall -= HandleOnHitWall;
        base.OnCardDisable();
    }

    private void HandleOnHitWall(GunBullet bullet, Vector2 hitPosition, GameObject wall)
    {
        // Use TriggerAction to spawn walls at hitPosition
        TriggerAction(this, CreateTempTransformAt(hitPosition));
    }

    // Helper to create a temporary transform at a position
    private Transform CreateTempTransformAt(Vector2 pos)
    {
        GameObject temp = new GameObject("TempWallSpawnPoint");
        temp.transform.position = pos;
        // Destroy after 2 seconds to avoid leaks
        Destroy(temp, 2f);
        return temp.transform;
    }

    public override void Reset()
    {
        base.Reset();
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitWall -= HandleOnHitWall;
        spawnCount = initialSpawnCount;
        spawnDamage = initialSpawnDamage;
        triggerProbability = initialTriggerProbability;
    }

    public override string GetDescription()
    {
        return string.Format(card_description, spawnCount, triggerProbability, spawnDamage, manaCost);
    }

    public override bool UpdateNumberValue(CardMaster.NumberType numberType, float value, CardMaster source = null)
    {
        if (IsBuffedFromSource(source, addToList: true, includeSelf: true)) return false;
        base.UpdateNumberValue(numberType, value, source);
        if (numberType == CardMaster.NumberType.Amount)
        {
            spawnCount += (int)value;
            return true;
        }
        // If you have a Probability type, use it. Otherwise, treat as a float (customize as needed)
        else if (numberType == CardMaster.NumberType.Probability)
        {
            triggerProbability += value;
            triggerProbability = Mathf.Clamp01(triggerProbability);
            return true;
        }
        else if (numberType == CardMaster.NumberType.Damage)
        {
            spawnDamage += value;
            return true;
        }
        return false;
    }
}
