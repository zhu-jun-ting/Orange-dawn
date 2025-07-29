using System.Collections;
using UnityEngine;

public class CardActionSkeletons : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public event System.Action<CardMaster, Transform> OnTrigger;
    public float actionCooldown { get; set; } = 1f;

    [Header("Skeleton Settings")]
    public float radius = 3f;
    public GameObject skeletonPrefab;


    // --- Common for all action cards ---
    private float lastActionTime = -10f;

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

    private void HandleOnLevelStart()
    {
        if (Time.time - lastActionTime < actionCooldown) return;
        if (UnityEngine.Random.value > probability) return;

        if (!ManaBar.CanCostMana(-(int)mana)) return;
        lastActionTime = Time.time;
        OnTrigger?.Invoke(this, transform);
        GameEvents.instance.UpdateMana(-(int)mana);
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        GameEvents.instance.TriggerActionCard(card, target);
        // Use only parent variables: damage, health, (int)mana, (int)amount, etc.
        // Example: Summon skeletons using amount as count, (int)damage, health, (int)mana
        int count = Mathf.Max(1, (int)amount); // Use amount as skeleton count
        GameObject prefab = skeletonPrefab;
        if (prefab == null) return;
        Vector3 center = target != null ? target.position : Vector3.zero;
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2f / count;
            Vector3 spawnPos = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            GameObject skeletonObj = ObjectPool.Instance.GetObject(prefab);
            skeletonObj.transform.position = spawnPos;
            NPCBatter skeleton = skeletonObj.GetComponent<NPCBatter>();
            if (skeleton != null)
            {
                skeleton.damage = damage;
                skeleton.maxHP = health;
            }
        }
        
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), (int)damage, health, (int)amount));
    }
}
