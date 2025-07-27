using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// id: 312
// name: Artillery
// desc: When you break any object in scene, Probability: 50% to shoot Amount: 1 explosive bullets towards enemies
// Damage: 14"
// Mana: 1

public class CardActionArtillery : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public event System.Action<CardMaster, Transform> OnTrigger;


    [Header("Explode Bullet Settings")]
    public GameObject explodeBulletPrefab; // Assign in inspector
    public List<string> trigger_tags = new List<string> { "Enemy" }; // Tags that can trigger the action


    [Tooltip("Maximum random angle offset (degrees) for bullet inaccuracy")]
    public float randomAngleOffset = 10f;


    private float lastActionTime = -10f;

    public override void OnCardEnable()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnDestroyObject += HandleOnDestroyObject;
        OnTrigger -= TriggerAction; // Unsubscribe to avoid duplicates
        OnTrigger += TriggerAction; // Subscribe to the trigger event
        base.OnCardEnable();
    }

    public override void OnCardDisable()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnDestroyObject -= HandleOnDestroyObject;
        OnTrigger -= TriggerAction;
        base.OnCardDisable();
    }

    private void HandleOnDestroyObject(Transform objectTransform, GunBullet bullet)
    {
        if (bullet == null || explodeBulletPrefab == null) return;
        if (Time.time - lastActionTime < actionCooldown) return;
        if (UnityEngine.Random.value > probability / 100) return;
        if (!ManaBar.CanCostMana(-(int)mana)) return;

        OnTrigger?.Invoke(this, objectTransform);

        GameEvents.instance.UpdateMana(-(int)mana);
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        GameEvents.instance.TriggerActionCard(card, target);
        if (explodeBulletPrefab == null || target == null) return;

        lastActionTime = Time.time;
        Vector2 spawnPos = target.position;
        
        SpawnBullets(explodeBulletPrefab,
            _count: (int)amount,
            _position: spawnPos,
            _rotation: Quaternion.identity,
            _randomAngleOffset: randomAngleOffset,
            _triggerTags: trigger_tags,
            _bulletDamage: 0f,
            _modifyBullet: bullet =>
        {
            var explodeBullet = bullet.GetComponent<ExplodeBullet>();
            if (explodeBullet != null) explodeBullet.explosionDamage = damage; // Set explosion damage
        });
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(card_description, probability, (int)amount, damage, (int)mana));
    }

    public override void Reset()
    {
        base.Reset();
        if (GameEvents.instance != null)
            GameEvents.instance.OnDestroyObject -= HandleOnDestroyObject;
        OnTrigger -= TriggerAction;
    }
}
