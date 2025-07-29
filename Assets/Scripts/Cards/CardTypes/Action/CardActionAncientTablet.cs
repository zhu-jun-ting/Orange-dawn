using System.Collections;
using UnityEngine;

public class CardActionAncientTablet : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public event System.Action<CardMaster, Transform> OnTrigger;
    public float actionCooldown { get; set; } = 1f;

    [Header("Ancient Tablet Settings")]
    public GameObject skeletonPrefab; // Assign in inspector
    public float spawnRadius = 2.5f;
    public float damageModifier = 1f;
    public float healthModifier = 1f;
    private float lastActionTime = -10f;

    public override void OnCardEnable()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnNPCCharge += HandleOnNPCCharge;
        OnTrigger -= TriggerAction;
        OnTrigger += TriggerAction;
        base.OnCardEnable();
    }

    public override void Reset()
    {
        base.Reset();
        if (GameEvents.instance != null)
            GameEvents.instance.OnNPCCharge -= HandleOnNPCCharge;
        OnTrigger -= TriggerAction;
    }

    private void HandleOnNPCCharge(NPCMaster npc)
    {
        if (Time.time - lastActionTime < actionCooldown) return;
        if (npc == null || skeletonPrefab == null) return;
        if (UnityEngine.Random.value > probability / 100) return;
        if (PlayerController.instance != null && HealthBar.CanCostHealth(-health))
        {
            lastActionTime = Time.time;
            GameEvents.instance.UpdateHealth(-(int)health);
            OnTrigger?.Invoke(this, npc.transform);
        }
    }

    public void TriggerAction(CardMaster card, Transform npcTransform)
    {
        Vector3 center = npcTransform != null ? npcTransform.position : Vector3.zero;
        for (int i = 0; i < amount; i++)
        {
            Vector2? spawnPos2D = CombatManager.instance.TryGetSpawnLocation(center, spawnRadius);
            Vector3 spawnPos = spawnPos2D.HasValue ? new Vector3(spawnPos2D.Value.x, spawnPos2D.Value.y, center.z) : center;
            GameObject skeletonObj = ObjectPool.Instance.GetObject(skeletonPrefab);
            skeletonObj.transform.position = spawnPos;
            NPCMaster skeleton = skeletonObj.GetComponent<NPCMaster>();
            if (skeleton != null)
            {
                skeleton.maxHP = health * healthModifier;
                skeleton.damage = damage * damageModifier;
            }
        }
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), probability, (int)amount, healthModifier, (int)health * healthModifier, damageModifier, (int)damage * damageModifier, (int)health));
    }
}
