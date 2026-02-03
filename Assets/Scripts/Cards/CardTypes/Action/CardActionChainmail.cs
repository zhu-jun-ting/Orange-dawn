using System;
using System.Collections.Generic;
using UnityEngine;

// id: 334
// name: Chainmail
// desc: Negate 1 damage taken in each room.
public class CardActionChainmail : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.0f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public event Action<CardMaster, Transform> OnTrigger;

    [Header("Chainmail Settings")]
    private int currentBlocks = 0;

    public override void OnCardEnable()
    {
        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnIncomingDamage += HandleIncomingDamage;
            GameEvents.instance.OnPlayerChoseNextRoom += HandleRoomChange;
        }
        
        // Initialize blocks
        currentBlocks = (int)amount;

        OnTrigger -= TriggerAction;
        OnTrigger += TriggerAction;
        base.OnCardEnable();
    }

    public override void OnCardDisable()
    {
        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnIncomingDamage -= HandleIncomingDamage;
            GameEvents.instance.OnPlayerChoseNextRoom -= HandleRoomChange;
        }
        OnTrigger -= TriggerAction;
        base.OnCardDisable();
    }

    public override void Reset()
    {
        base.Reset();
        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnIncomingDamage -= HandleIncomingDamage;
            GameEvents.instance.OnPlayerChoseNextRoom -= HandleRoomChange;
        }
        OnTrigger -= TriggerAction;
    }

    private void HandleIncomingDamage(ref float damage, PawnMaster receiver)
    {
        if (receiver == null || !receiver.isPlayer) return;
        
        if (damage > 0 && currentBlocks > 0)
        {
            damage = 0f;
            currentBlocks--;
            
            GameEvents.instance.ShowStringUI("Blocked", receiver, GameEvents.DamageType.Normal, receiver.transform.position);
            OnTrigger?.Invoke(this, receiver.transform);
        }
    }

    private void HandleRoomChange(GameEvents.Dir dir, FloorManager.RoomType roomType)
    {
        currentBlocks = (int)amount;
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        GameEvents.instance.TriggerActionCard(card, target);
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), (int)amount));
    }
}
