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
    public int max_instances = 100;

    [Header("Spawm Settings")]
    public float spawn_radius = 5f;
    public GameObject npcShooterPrefab; // Assign in inspector

    // Keep track of spawned NPCs
    private static List<GameObject> spawnedNPCs = new List<GameObject>();

    public void TriggerAction(CardMaster source, Transform location)
    {
        if (npcShooterPrefab == null || location == null) return;
        for (int i = 0; i < spawn_count; i++)
        {
            // Enforce max_instances
            if (spawnedNPCs.Count >= max_instances)
            {
                if (spawnedNPCs[0] != null)
                    GameObject.Destroy(spawnedNPCs[0]);
                spawnedNPCs.RemoveAt(0);
            }
            // Random position within radius
            Vector2 randCircle = Random.insideUnitCircle * spawn_radius;
            Vector3 spawnPos = location.position + new Vector3(randCircle.x, 0, randCircle.y);
            GameObject npc = GameObject.Instantiate(npcShooterPrefab, spawnPos, Quaternion.identity);
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
            spawnedNPCs.Add(npc);
        }
    }

    public override void OnCardEnable()
    {
        // Try to find the gun reference from linked cards
        current_gun = GetLinkedGun();
        CardMaster[] linked = new CardMaster[] {up_link_cardmaster, left_link_cardmaster, right_link_cardmaster, down_link_cardmaster};

        base.OnCardEnable();
    }

    public override void Reset()
    {
        base.Reset(); // Call the base reset method to reset other properties
    }
    
    // return the formatted description of the card
    public override string GetDescription()
    {
        return string.Format(card_description);
    }

    public override void UpdateNumberValue(CardMaster.NumberType numberType, float value, CardMaster source = null)
    {
        if (IsBuffedFromSource(source, addToList: true, includeSelf: true)) return;
        base.UpdateNumberValue(numberType, value, source);
        if (numberType == CardMaster.NumberType.Damage)
        {
            // ...existing code...
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
