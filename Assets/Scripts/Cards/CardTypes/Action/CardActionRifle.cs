using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// id: 313
// name: Rifle
// desc: When you hit any object in scene, Probability: 30% to create Amount: 2 bullets towards enemies
// Damage: 6
// Mana: 0

public class CardActionRifle : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public event Action<CardMaster, Transform> OnTrigger;

    [Header("Rifle Bullet Settings")]
    public GameObject rifleBulletPrefab; // Assign in inspector
    public List<string> trigger_tags = new List<string> { "Enemy" }; // Tags that can trigger the action
    [Tooltip("Maximum random angle offset (degrees) for bullet inaccuracy")]
    public float randomAngleOffset = 8f;

    private float lastActionTime = -10f;

    public override void OnCardEnable()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitWall += HandleOnHitWall;
        OnTrigger -= TriggerAction;
        OnTrigger += TriggerAction;
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
        if (bullet == null || rifleBulletPrefab == null) return;
        if (Time.time - lastActionTime < actionCooldown) return;
        if (UnityEngine.Random.value > probability / 100) return; 
        if (!ManaBar.CanCostMana(-(int)mana)) return;

        OnTrigger?.Invoke(this, CreateTempTransformAt(hitPosition));

        GameEvents.instance.UpdateMana(-(int)mana);
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        GameEvents.instance.TriggerActionCard(card, target);
        if (rifleBulletPrefab == null || target == null) return;
        lastActionTime = Time.time;
        Vector2 spawnPos = target.position;

        SpawnBullets(
            rifleBulletPrefab,
            _count: (int)amount,
            _position: spawnPos,
            _rotation: Quaternion.identity,
            _randomAngleOffset: randomAngleOffset,
            _triggerTags: trigger_tags,
            _bulletDamage: damage
        );
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), probability, (int)amount, damage, (int)mana));
    }

    public override void Reset()
    {
        base.Reset();
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitWall -= HandleOnHitWall;
        OnTrigger -= TriggerAction;
    }
}
