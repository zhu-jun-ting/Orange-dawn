using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// id: custom
// name: Womb
// desc: When an enemy dies, Probability: 10% to shoot a pinball towards a random target. Cost: Mana: 2
public class CardActionWomb : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public event System.Action<CardMaster, Transform> OnTrigger;

    [Header("Pinball Settings")]
    public GameObject pinballPrefab; // Assign pinball prefab in inspector
    public List<string> targetTags = new List<string> { "Enemy" }; // Tags that can be targeted

    [Tooltip("Maximum random angle offset (degrees) for bullet inaccuracy")]
    public float randomAngleOffset = 10f;

    private float lastActionTime = -10f;

    public override void OnCardEnable()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnPawnDie += HandleOnPawnDie;
        OnTrigger -= TriggerAction; // Unsubscribe to avoid duplicates
        OnTrigger += TriggerAction; // Subscribe to the trigger event
        base.OnCardEnable();
    }

    public override void OnCardDisable()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnPawnDie -= HandleOnPawnDie;
        OnTrigger -= TriggerAction;
        base.OnCardDisable();
    }

    private void HandleOnPawnDie(PawnMaster deadPawn, float damageDealt, GameObject instigator, GameEvents.DamageType damageType, Gun source)
    {
        // Only trigger on enemy deaths
        if (deadPawn as EnemyMaster == null) return;
        
        if (Time.time - lastActionTime < actionCooldown) return;
        if (UnityEngine.Random.value > probability / 100f) return;
        if (!ManaBar.CanCostMana(-(int)mana)) return;

        OnTrigger?.Invoke(this, deadPawn.transform);

        GameEvents.instance.UpdateMana(-(int)mana);
    }

    public void TriggerAction(CardMaster card, Transform deadEnemyTransform)
    {
        GameEvents.instance.TriggerActionCard(card, deadEnemyTransform);
        if (pinballPrefab == null || deadEnemyTransform == null) return;

        lastActionTime = Time.time;
        Vector2 spawnPos = deadEnemyTransform.position;
        
        // Find a random target from the target tags
        GameObject randomTarget = FindRandomTarget();
        Quaternion spawnRotation = Quaternion.identity;
        
        if (randomTarget != null)
        {
            Vector2 direction = ((Vector2)randomTarget.transform.position - spawnPos).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            spawnRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        SpawnBullets(pinballPrefab,
            _count: 1,
            _position: spawnPos,
            _rotation: spawnRotation,
            _randomAngleOffset: randomAngleOffset,
            _triggerTags: targetTags,
            _bulletDamage: damage);
    }

    private GameObject FindRandomTarget()
    {
        List<GameObject> validTargets = new List<GameObject>();
        
        // Search for all objects with target tags
        foreach (var tag in targetTags)
        {
            GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(tag);
            foreach (var obj in objectsWithTag)
            {
                if (obj != null && obj.activeSelf)
                    validTargets.Add(obj);
            }
        }

        // Return random target or null if none found
        if (validTargets.Count > 0)
            return validTargets[UnityEngine.Random.Range(0, validTargets.Count)];
        
        return null;
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), probability, (int)damage, (int)mana));
    }

    public override void Reset()
    {
        base.Reset();
        if (GameEvents.instance != null)
            GameEvents.instance.OnPawnDie -= HandleOnPawnDie;
        OnTrigger -= TriggerAction;
    }
}
