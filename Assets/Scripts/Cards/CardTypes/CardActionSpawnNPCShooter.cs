using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardActionSpawnNPCShooter : CardMaster, ICardAction
{
    public float shoot_interval = 0.5f;
    public float attack = 10f;
    public float max_HP = 100f;
    public int spawn_count = 1;
    public float lifecycle = 10f;
    public int max_instances = 10;

    [Header("Spawm Settings")]
    public float spawn_radius = 5f;
    public GameObject npcShooterPrefab; // Assign in inspector



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
        for (int i = 0; i < spawn_count; i++)
        {

            // Randomly position the NPC within a circle around the location
            Vector2 randCircle = Random.insideUnitCircle * spawn_radius;
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
    }

    public override void OnCardEnable()
    {
        // Try to find the gun reference from linked cards
        current_gun = GetLinkedGun();
        CardMaster[] linked = new CardMaster[] {up_link_cardmaster, left_link_cardmaster, right_link_cardmaster, down_link_cardmaster};

        base.OnCardEnable();
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
        shoot_interval = initialShootInterval;
        attack = initialAttack;
        max_HP = initialMaxHP;
        spawn_count = initialSpawnCount;
        lifecycle = initialLifecycle;
        max_instances = initialMaxInstances;
        spawn_radius = initialSpawnRadius;
        npcShooterPrefab = initialNpcShooterPrefab;
        base.Reset(); // Call the base reset method to reset other properties
    }
    
    // return the formatted description of the card
    public override string GetDescription()
    {
        return string.Format(card_description, max_HP, attack, shoot_interval, spawn_count, max_instances);
    }

    public override void UpdateNumberValue(CardMaster.NumberType numberType, float value, CardMaster source = null)
    {
        if (IsBuffedFromSource(source, addToList: true, includeSelf: true)) return;
        base.UpdateNumberValue(numberType, value, source);
        if (numberType == CardMaster.NumberType.Damage)
        {
            attack += value;
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
