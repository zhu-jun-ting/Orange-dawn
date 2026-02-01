using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardRelicGarden : CardMaster, ICardAction
{
    // id: 506
    // name: Garden
    // desc: When you kill an enemy, Probability: {0}% to grow a random value card (80% chance) or decay it (20% chance).

    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.1f;
    public event System.Action<CardMaster, Transform> OnTrigger;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }

    [Header("Garden Settings")]
    public float triggerProbability = 0.05f;
    public float growProbability = 0.8f; // 80% chance to grow, 20% to decay

    public override void OnCardEnable()
    {
        base.OnCardEnable();
        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnPawnDie -= HandlePawnDie;
            GameEvents.instance.OnPawnDie += HandlePawnDie;
        }
    }

    public override void OnCardDisable()
    {
        base.OnCardDisable();
        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnPawnDie -= HandlePawnDie;
        }
    }

    private void HandlePawnDie(PawnMaster pawn, float damage, GameObject instigator, GameEvents.DamageType type, Gun gun)
    {
        if (pawn.isEnemy)
        {
            if (UnityEngine.Random.value <= triggerProbability)
            {
                TriggerAction(this, pawn.transform);
            }
        }
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        GameEvents.instance.TriggerActionCard(card, target);

        if (BoardArea.instance != null)
        {
            List<CardMaster> allCards = BoardArea.instance.GetCardsOnBoard();
            List<CardMaster> valueCards = new List<CardMaster>();
            foreach (var c in allCards)
            {
                if (c != null && c.card_type == CardType.Value)
                {
                    valueCards.Add(c);
                }
            }

            if (valueCards.Count > 0)
            {
                CardMaster chosen = valueCards[UnityEngine.Random.Range(0, valueCards.Count)];
                if (UnityEngine.Random.value <= growProbability)
                {
                    chosen.Grow(1);
                }
                else
                {
                    chosen.Decay(1);
                }
            }
        }
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), (int)(triggerProbability * 100)));
    }
}
