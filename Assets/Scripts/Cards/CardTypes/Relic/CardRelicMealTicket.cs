using System.Collections;
using UnityEngine;

// id: 505
// name: Meal Ticket
// desc: When you enter a shop room, recover you Health: 10.

public class CardRelicMealTicket : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public event System.Action<CardMaster, Transform> OnTrigger;
    public float actionCooldown { get; set; } = 1f;

    public int healAmount = 10;

    private float lastActionTime = -10f;

    public override void OnCardEnable()
    {
        base.OnCardEnable();
        if (GameEvents.instance != null)
            GameEvents.instance.OnPlayerChoseNextRoom += HandlePlayerNextRoom;
        OnTrigger -= TriggerAction;
        OnTrigger += TriggerAction;
    }

    public override void OnCardDisable()
    {
        base.OnCardDisable();
        if (GameEvents.instance != null)
            GameEvents.instance.OnPlayerChoseNextRoom -= HandlePlayerNextRoom;
        OnTrigger -= TriggerAction;
    }

    public override void Reset()
    {
        base.Reset();
        if (GameEvents.instance != null)
            GameEvents.instance.OnPlayerChoseNextRoom -= HandlePlayerNextRoom;
        OnTrigger -= TriggerAction;
    }

    private void HandlePlayerNextRoom(GameEvents.Dir dir, FloorManager.RoomType roomType)
    {
        if (Time.time - lastActionTime < actionCooldown) return;
        
        // Check if the target room is a Shop
        if (roomType == FloorManager.RoomType.Shop)
        {
            lastActionTime = Time.time;
            if (PlayerController.instance != null)
            {
                OnTrigger?.Invoke(this, PlayerController.instance.transform);
            }
        }
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        GameEvents.instance.TriggerActionCard(card, target);
        if (target == null) return;
        if (GameEvents.instance == null) return;
        if (PlayerController.instance == null) return;

        GameEvents.instance.HealPawn(healAmount, PlayerController.instance, gameObject, target);
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), healAmount));
    }
}
