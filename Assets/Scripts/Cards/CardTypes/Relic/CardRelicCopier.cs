using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardRelicCopier : CardMaster, ICardAction
{
    // id: 508
    // name: Copier
    // desc: After entering {0} rooms (current: {1}), create a copy of the linked card and destroy this card.

    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public event System.Action<CardMaster, Transform> OnTrigger;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }

    [Header("Copier Settings")]
    public int roomsRequired = 3;
    
    private int roomCounter = 0;

    public override void OnCardEnable()
    {
        base.OnCardEnable();
        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnPlayerChoseNextRoom -= HandleRoomChosen;
            GameEvents.instance.OnPlayerChoseNextRoom += HandleRoomChosen;
        }
    }

    public override void OnCardDisable()
    {
        base.OnCardDisable();
        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnPlayerChoseNextRoom -= HandleRoomChosen;
        }
    }

    private void HandleRoomChosen(GameEvents.Dir dir, FloorManager.RoomType roomType)
    {
        roomCounter++;
        if (roomCounter >= roomsRequired)
        {
            TriggerAction(this, transform);
        }
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        GameEvents.instance.TriggerActionCard(card, target);

        // Find linked card
        var linked = up_link_cardmaster ?? left_link_cardmaster ?? right_link_cardmaster ?? down_link_cardmaster;
        if (linked != null)
        {
            // Create copy
            if (int.TryParse(linked.card_id, out int linkedId))
            {
                GameObject prefab = CardManager.GetCardById(linkedId);
                if (prefab != null)
                {
                    // Add to hand
                    if (CardManager.instance != null)
                    {
                        // Pass the prefab directly, CardManager will instantiate it
                        CardManager.instance.QueueAddCardObjects(new List<GameObject> { prefab });
                    }
                    
                    // Destroy self
                    OnCardDestroyed();
                }
            }
        }
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), roomsRequired, roomCounter));
    }
}
