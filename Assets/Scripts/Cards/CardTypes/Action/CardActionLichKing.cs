using System.Collections;
using UnityEngine;

public class CardActionLichKing : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public event System.Action<CardMaster, Transform> OnTrigger;
    public float actionCooldown { get; set; } = 1f;

    // --- Common for all action cards ---
    private float lastActionTime = -10f;

    public override void OnCardEnable()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnLevelCleared += HandleOnLevelCleared;
        OnTrigger -= TriggerAction;
        OnTrigger += TriggerAction;
        base.OnCardEnable();
    }

    public override void Reset()
    {
        base.Reset();
        if (GameEvents.instance != null)
            GameEvents.instance.OnLevelCleared -= HandleOnLevelCleared;
        OnTrigger -= TriggerAction;
    }

    private void HandleOnLevelCleared()
    {
        if (Time.time - lastActionTime < actionCooldown) return;
        OnTrigger?.Invoke(this, transform);
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        lastActionTime = Time.time;
        // Give a random Temporary card to the player
        GameObject tempCardPrefab = CardDatabase.GetRandomCard(cm => GameSettings.IsConditionAllowed(cm.card_type, CardMaster.CardCondition.IsTemporary));
        if (tempCardPrefab != null)
        {
            // Add to hand area
            if (CardManager.instance != null && CardManager.instance.handArea != null)
            {
                GameObject newCard = Object.Instantiate(tempCardPrefab);
                if (newCard.TryGetComponent<CardMaster>(out var cardMaster))
                {
                    cardMaster.card_conditions.Add(CardMaster.CardCondition.IsTemporary);
                    CardManager.instance.QueueAddCardObjects(new System.Collections.Generic.List<GameObject> { newCard });
                }
            }
        }
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon("At the end of each level, gain a random Temporary card.");
    }
}
