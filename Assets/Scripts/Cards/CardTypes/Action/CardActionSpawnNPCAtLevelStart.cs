using System.Collections;
using UnityEngine;

public class CardActionSpawnNPCAtLevelStart : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public event System.Action<CardMaster, Transform> OnTrigger;
    public float actionCooldown { get; set; } = 1f;

    [Header("NPC Settings")]
    public GameObject npcPrefab; // Assign any NPC Ally prefab in inspector
    public float spawnRadius = 3f;
    private float lastActionTime = -10f;
    public float damageModifier = 1f; // Damage modifier for spawned NPCs
    public float healthModifier = 1f; // Health modifier for spawned NPCs

    public override void OnCardEnable()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnLevelStart += HandleOnLevelStart;
        OnTrigger -= TriggerAction;
        OnTrigger += TriggerAction;
        base.OnCardEnable();
    }

    public override void Reset()
    {
        base.Reset();
        if (GameEvents.instance != null)
            GameEvents.instance.OnLevelStart -= HandleOnLevelStart;
        OnTrigger -= TriggerAction;
    }

    private void HandleOnLevelStart(int levelIndex)
    {
        if (Time.time - lastActionTime < actionCooldown) return;
        if (UnityEngine.Random.value > probability) return;
        OnTrigger?.Invoke(this, transform);
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        lastActionTime = Time.time;
        int count = Mathf.Max(1, (int)(amount)); // Use amount as NPC count
        GameObject prefab = npcPrefab;
        if (prefab == null) return;
        Vector3 center = target != null ? target.position : Vector3.zero;
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2f / count;
            Vector3 spawnPos = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * spawnRadius;
            GameObject npcObj = ObjectPool.Instance.GetObject(prefab);
            npcObj.transform.position = spawnPos;
            NPCMaster npc = npcObj.GetComponent<NPCMaster>();
            if (npc != null)
            {
                npc.maxHP = health * healthModifier;
                npc.damage = damage * damageModifier;
            }
        }
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(card_description, amount, healthModifier, health * healthModifier, damageModifier, damage * damageModifier));
    }
}
