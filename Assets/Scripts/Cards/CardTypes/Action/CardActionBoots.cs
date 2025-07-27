using System.Collections;
using UnityEngine;

public class CardActionBoots : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public event System.Action<CardMaster, Transform> OnTrigger;
    public float actionCooldown { get; set; } = 1f;

    [Header("Boots Settings")]
    public float requiredDistance = 100f; // Distance to trigger
    public float allyDuration = 10f; // How long the ally stays
    public float spawnRadius = 3f;
    public float damageModifier = 1f;
    public float healthModifier = 1f;
    private float movedDistance = 0f;
    private float lastActionTime = -10f;
    private Transform playerTransform;

    public override void OnCardEnable()
    {
        movedDistance = 0f;
        if (GameEvents.instance != null)
            GameEvents.instance.OnPlayerMove += HandleOnPlayerMove;
        OnTrigger -= TriggerAction;
        OnTrigger += TriggerAction;
        base.OnCardEnable();
    }

    public override void Reset()
    {
        base.Reset();
        if (GameEvents.instance != null)
            GameEvents.instance.OnPlayerMove -= HandleOnPlayerMove;
        OnTrigger -= TriggerAction;
        movedDistance = 0f;
    }

    private void HandleOnPlayerMove(float distance)
    {
        movedDistance += distance;
        if (movedDistance >= requiredDistance && Time.time - lastActionTime >= actionCooldown && CoinCounter.instance != null)
        {
            if (CoinCounter.CanCostCoin(-(int)(coin)))
            {
                movedDistance = 0f;
                lastActionTime = Time.time;
                playerTransform = PlayerController.instance != null ? PlayerController.instance.transform : null;
                OnTrigger?.Invoke(this, playerTransform);
                GameEvents.instance.UpdateCoins(-(int)(coin));
            }
        }
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        GameEvents.instance.TriggerActionCard(card, target);
        if (GameSettings.instance == null || GameSettings.instance.NPCs == null || GameSettings.instance.NPCs.Count == 0) return;
        int count = Mathf.Max(1, (int)(amount));
        Vector3 center = target != null ? target.position : Vector3.zero;
        for (int i = 0; i < count; i++)
        {
            // Pick a random NPC Ally prefab
            GameObject prefab = GameSettings.instance.NPCs[Random.Range(0, GameSettings.instance.NPCs.Count)];
            if (prefab == null) continue;
            Vector2? spawnPos2D = CombatManager.instance.TryGetSpawnLocation(center, spawnRadius);
            Vector3 spawnPos = spawnPos2D.HasValue ? new Vector3(spawnPos2D.Value.x, spawnPos2D.Value.y, center.z) : center;
            GameObject npcObj = ObjectPool.Instance.GetObject(prefab);
            npcObj.transform.position = spawnPos;
            NPCMaster npc = npcObj.GetComponent<NPCMaster>();
            if (npc != null)
            {
                npc.maxHP = health * healthModifier;
                npc.damage = damage * damageModifier;
            }
            if (allyDuration > 0)
            {
                AutoDestroy autoDestroy = npcObj.GetComponent<AutoDestroy>();
                if (autoDestroy == null)
                    autoDestroy = npcObj.AddComponent<AutoDestroy>();
                autoDestroy.lifetime = allyDuration;
            }
        }
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(card_description, requiredDistance, (int)amount, healthModifier, health * healthModifier, damageModifier, damage * damageModifier, (int)coin));
    }
}
