using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardActionExplodeBulletOnHitWall : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float _triggerProbability = 1f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public float triggerProbability { get => _triggerProbability; set => _triggerProbability = value; }
    public event System.Action<CardMaster, Transform> OnTrigger;

    [Header("Explode Bullet Settings")]
    public GameObject explodeBulletPrefab; // Assign in inspector
    public List<string> trigger_tags = new List<string> { "Enemy" }; // Tags that can trigger the action
    public float Damage = 10f; // Damage for each bullet (modifies ImpulseAOE)
    public int Amount = 1; // How many bullets to spawn
    public int ManaCost = 2; // Mana cost
    [Tooltip("Maximum random angle offset (degrees) for bullet inaccuracy")]
    public float randomAngleOffset = 10f;

    // Store initial values for reset
    private float _initDamage;
    private int _initAmount;
    private float _initProbability;
    private int _initManaCost;

    private float lastActionTime = -10f;

    protected override void Awake()
    {
        base.Awake();
        _initDamage = Damage;
        _initAmount = Amount;
        _initProbability = _triggerProbability;
        _initManaCost = ManaCost;
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
        if (Time.time - lastActionTime < actionCooldown) return;
        if (!ManaBar.CanCostMana(-ManaCost)) return;
        if (UnityEngine.Random.value > triggerProbability) return;
        lastActionTime = Time.time;
        OnTrigger?.Invoke(this, bullet.transform);
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        if (explodeBulletPrefab == null || target == null) return;
        Vector2 spawnPos = target.position;
        for (int i = 0; i < Amount; i++)
        {
            GameObject bulletObj = ObjectPool.Instance.GetObject(explodeBulletPrefab);
            bulletObj.transform.position = spawnPos;
            var bullet = bulletObj.GetComponent<ExplodeBullet>();
            if (bullet != null)
            {
                bullet.SetOwner(gameObject); // Set owner to this card
                bullet.SetSpeed(Vector2.zero); // Start with zero speed
                bullet.trigger_tags = trigger_tags; // Ensure it can hit enemies
                // Set direction towards nearest enemy
                Vector2 dir = CombatManager.instance.GetVectorToNearestEnemy(spawnPos);
                if (dir == Vector2.zero)
                    dir = UnityEngine.Random.insideUnitCircle.normalized; // fallback
                // Add random angle offset for inaccuracy
                float angleOffset = UnityEngine.Random.Range(-randomAngleOffset, randomAngleOffset);
                dir = Quaternion.Euler(0, 0, angleOffset) * dir;
                bullet.SetSpeed(dir, bullet.speed);
                // Set damage for ImpulseAOE if present
                bullet.explosionDamage = Damage;
            }
        }
        GameEvents.instance.UpdateMana(-ManaCost);
    }

    public override string GetDescription()
    {
        return $"On bullet hit wall, spawn {Amount} explode bullet(s) (Damage: {Damage}) towards nearest enemy. Mana: {ManaCost}, Chance: {triggerProbability * 100f}%, Inaccuracy: ±{randomAngleOffset}°";
    }

    public override bool UpdateNumberValue(CardMaster.NumberType numberType, float value, CardMaster source = null)
    {
        if (IsBuffedFromSource(source, addToList: true, includeSelf: true)) return false;
        base.UpdateNumberValue(numberType, value, source);
        if (numberType == CardMaster.NumberType.Damage)
        {
            Damage += value;
            return true;
        }
        else if (numberType == CardMaster.NumberType.Amount)
        {
            Amount += (int)value;
            return true;
        }
        else if (numberType == CardMaster.NumberType.Probability)
        {
            triggerProbability += value;
            return true;
        }
        else if (numberType == CardMaster.NumberType.Mana)
        {
            ManaCost += (int)value;
            return true;
        }
        return false;
    }

    public override void Reset()
    {
        base.Reset();
        Damage = _initDamage;
        Amount = _initAmount;
        triggerProbability = _initProbability;
        ManaCost = _initManaCost;
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitWall -= HandleOnHitWall;
        OnTrigger -= TriggerAction;
    }
}
