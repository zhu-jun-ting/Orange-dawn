using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardActionCircularBulletOnHitWall : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float _triggerProbability = 1f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public event System.Action<CardMaster, Transform> OnTrigger;

    [Header("Circular Bullet Settings")]
    public GameObject circularBulletPrefab; // Assign in inspector
    public float Damage = 10f; // Damage for each bullet 
    public int Amount = 1; // How many bullets to spawn
    public int ManaCost = 2; // Mana cost
    public float bulletRadius = 1f; // Radius of the circular bullets
    [Tooltip("Maximum random offset added to bullet radius")] 
    public float randomRadiusOffset = 0.2f;
    public float bulletDuration = 3f; // Duration for which the bullets exist (if needed)

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
        if (UnityEngine.Random.value > probability) return;
        
        OnTrigger?.Invoke(this, bullet.transform);
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        if (circularBulletPrefab == null || target == null) return;
        if (!ManaBar.CanCostMana(-ManaCost)) return;

        lastActionTime = Time.time;
        Vector2 spawnPos = target.position;
        float angleStep = 360f / Amount;
        for (int i = 0; i < Amount; i++)
        {
            GameObject bulletObj = ObjectPool.Instance.GetObject(circularBulletPrefab);
            bulletObj.transform.position = spawnPos;
            var bullet = bulletObj.GetComponent<CircularBullet>();
            if (bullet != null)
            {
                bullet.center = spawnPos;
                float radiusRand = UnityEngine.Random.Range(-randomRadiusOffset, randomRadiusOffset);
                float finalRadius = bulletRadius + radiusRand;
                bullet.radius = finalRadius > 0 ? finalRadius : 0.01f; // Ensure radius is positive
                bullet.initialAngle = i * angleStep;
                bullet.lifetime = bulletDuration; // Set lifetime if needed
                // bullet.angularSpeed = bullet.angularSpeed; // Use prefab value or set here (remove redundant assignment)
                bullet.att = Damage; // Set bullet damage directly
            }
        }
        GameEvents.instance.UpdateMana(-ManaCost);
    }

    public override string GetDescription()
    {
        return $"On bullet hit wall, spawn {Amount} circular bullet(s) (Damage: {Damage}) in a circle. Mana: {ManaCost}, Chance: {probability * 100f}%";
    }

    public override bool UpdateNumberValue(NumberType numberType, float value, CardMaster source = null, bool isPermanent = false, bool isMult = false)
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
            probability += value;
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
        probability = _initProbability;
        ManaCost = _initManaCost;
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitWall -= HandleOnHitWall;
        OnTrigger -= TriggerAction;
    }
}
