using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardActionSpawnNPCShooter : CardMaster, ICardAction
{


    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float _triggerProbability = 1f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public float triggerProbability { get => _triggerProbability; set => _triggerProbability = value; }
    public event Action<CardMaster, Transform> OnTrigger;

    [Header("NPC Shooter Settings")]
    public float shoot_interval = 0.5f;
    public float attack = 10f;
    public float max_HP = 100f;
    public int spawn_count = 1;
    public float lifecycle = 10f;
    public int max_instances = 10;
    public int manaCost = 5; // Mana cost for this action

    [Header("Spawm Settings")]
    public float spawn_radius = 5f;
    public GameObject npcShooterPrefab; // Assign in inspector

    [Header("Trigger Settings")]
    public CardMaster.CardDir triggerDirection;



    // Store initial values for reset
    private float initialShootInterval;
    private float initialAttack;
    private float initialMaxHP;
    private int initialSpawnCount;
    private float initialLifecycle;
    private int initialMaxInstances;
    private float initialSpawnRadius;
    private GameObject initialNpcShooterPrefab;




    public void TriggerAction(CardMaster source, Transform location)
    {
        if (npcShooterPrefab == null || location == null) return;

        // Calculate mana cost
        if (!ManaBar.CanCostMana(-manaCost)) return;

        for (int i = 0; i < spawn_count; i++)
        {
            // Randomly position the NPC within a circle around the location
            Vector2 randCircle = UnityEngine.Random.insideUnitCircle * spawn_radius;
            Vector3 spawnPos = location.position + new Vector3(randCircle.x, 0, randCircle.y);

            // Ensure the NPC prefab is pooled
            ObjectPool.Instance.SetMaxSize(npcShooterPrefab, max_instances);
            GameObject npc = ObjectPool.Instance.GetObject(npcShooterPrefab);
            npc.transform.position = spawnPos;
            npc.transform.rotation = Quaternion.identity;
            // Assign stats

            NPCShooter shooter = npc.GetComponent<NPCShooter>();
            if (shooter != null)
            {
                shooter.maxHP = max_HP;
                shooter.attack = attack;
                shooter.shoot_interval = shoot_interval;
                // Optionally set HP to max
                shooter.maxHP = max_HP;
            }

            // Auto-destroy after lifecycle
            if (lifecycle > 0)
            {
                GameObject.Destroy(npc, lifecycle);
            }

            // spawnedNPCs.Add(npc);
        }

        // Deduct mana cost
        GameEvents.instance.UpdateMana(-manaCost);
    }

    public override void OnCardEnable()
    {
        // Try to find the gun reference from linked cards
        current_gun = GetLinkedGun(triggerDirection);
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitPawn += HandleOnHitPawn;

        OnTrigger -= TriggerAction; // Unsubscribe to avoid duplicates
        OnTrigger += TriggerAction; // Subscribe to the trigger event

        base.OnCardEnable();
    }

    public override void OnCardDisable()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitPawn -= HandleOnHitPawn;
        OnTrigger -= TriggerAction;
        base.OnCardDisable();
    }
    
    private float lastTriggerTime = -Mathf.Infinity;
    private void HandleOnHitPawn(float damage, PawnMaster receiver, GameObject instigator, GameEvents.DamageType damageType, Transform location, float hit_back, Gun source)
    {
        if (receiver != null && receiver.CompareTag("Enemy") && source == current_gun)
        {
            if (Time.time - lastTriggerTime >= _actionCooldown)
            {
                if (UnityEngine.Random.value <= _triggerProbability)
                {
                    lastTriggerTime = Time.time;
                    // Trigger the action at the receiver's position
                    OnTrigger?.Invoke(this, receiver.transform);
                }
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        initialShootInterval = shoot_interval;
        initialAttack = attack;
        initialMaxHP = max_HP;
        initialSpawnCount = spawn_count;
        initialLifecycle = lifecycle;
        initialMaxInstances = max_instances;
        initialSpawnRadius = spawn_radius;
        initialNpcShooterPrefab = npcShooterPrefab;
    }

    public override void Reset()
    {
        base.Reset(); // Call the base reset method to reset other properties

        if (GameEvents.instance != null)
            GameEvents.instance.OnHitPawn -= HandleOnHitPawn;
        OnTrigger -= TriggerAction;
        
        shoot_interval = initialShootInterval;
        attack = initialAttack;
        max_HP = initialMaxHP;
        spawn_count = initialSpawnCount;
        lifecycle = initialLifecycle;
        max_instances = initialMaxInstances;
        spawn_radius = initialSpawnRadius;
        npcShooterPrefab = initialNpcShooterPrefab;

    }

    // return the formatted description of the card
    public override string GetDescription()
    {
        return string.Format(card_description, max_HP, attack, shoot_interval, spawn_count, max_instances, manaCost);
    }

    public override bool UpdateNumberValue(CardMaster.NumberType numberType, float value, CardMaster source = null)
    {

        if (IsBuffedFromSource(source, addToList: true, includeSelf: true)) return false;

        base.UpdateNumberValue(numberType, value, source);

        if (numberType == CardMaster.NumberType.Mana)
        {
            manaCost += (int)value;
            return true;
        }
        else
        {
            return false;
        }
    }

}




// Helper for auto-destroying spawned NPCs after a set time
public class AutoDestroy : MonoBehaviour
{
    public float lifetime = 10f;
    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
