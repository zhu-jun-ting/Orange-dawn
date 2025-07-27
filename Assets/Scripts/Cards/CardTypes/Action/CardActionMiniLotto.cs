using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// id: 307
// name: Mini Lotto
// desc: When you break an object in the scene, you have a 5% chance to gain a random card. This card has the same chance to be destroyed when you are destroyed.
// Probability: 5%

public class CardActionMiniLotto : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public event Action<CardMaster, Transform> OnTrigger;

    [Header("Mini Lotto Settings")]
    private float lastActionTime = -10f;
    private bool isSubscribed = false;

    public override void OnCardEnable()
    {
        if (!isSubscribed && GameEvents.instance != null)
        {
            GameEvents.instance.OnDestroyObject += HandleOnDestroyObject;
            isSubscribed = true;
        }
        OnTrigger -= TriggerAction;
        OnTrigger += TriggerAction;
        base.OnCardEnable();
    }

    public override void OnCardDisable()
    {
        if (isSubscribed && GameEvents.instance != null)
        {
            GameEvents.instance.OnDestroyObject -= HandleOnDestroyObject;
            isSubscribed = false;
        }
        OnTrigger -= TriggerAction;
        base.OnCardDisable();
    }

    public override void Reset()
    {
        base.Reset();
        if (isSubscribed && GameEvents.instance != null)
        {
            GameEvents.instance.OnDestroyObject -= HandleOnDestroyObject;
            isSubscribed = false;
        }
        OnTrigger -= TriggerAction;
    }

    private void HandleOnDestroyObject(Transform obj, GunBullet bullet)
    {
        if (CardManager.instance == null || CardDatabase.instance == null) return;
        if (Time.time - lastActionTime < actionCooldown) return;
        if (UnityEngine.Random.value > probability / 100f) return;
        if (!ManaBar.CanCostMana(-(int)mana)) return;
        lastActionTime = Time.time;
        OnTrigger?.Invoke(this, obj);
        GameEvents.instance.UpdateMana(-(int)mana);
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        GameEvents.instance.TriggerActionCard(card, target);
        if (UnityEngine.Random.value > 0.5f)
        {
            // All action logic here: destroy the card
            if (CardManager.instance != null)
            {
                OnCardDestroyed();
                GameEvents.instance.CardDiscarded(this);
            }
            else
            {
                Debug.LogWarning("CardManager instance is null, cannot destroy card.");
            }
        }
        else
        {
            // All action logic here: add a random card to hand
            var randomCard = CardDatabase.GetRandomCard((cm) => true); // Any card
            if (randomCard != null)
            {
                var cardList = new List<GameObject> { randomCard };
                CardManager.instance.QueueAddCardObjects(cardList, 0.5f);
            }
        }

    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(card_description, probability));
    }
}
