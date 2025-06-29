using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardValueAddHealth : CardMaster
{

    [Header("Card Add Attack Settings")]
    [Tooltip("Amount of attack to add to the gun when this card is enabled.")]
    public float attackToAdd = 10f;
    private float attackToAddDefault = 10f;

    protected override void Awake() {
        base.Awake();
        attackToAddDefault = attackToAdd;
    }

    public override void OnCardEnable()
    {
        current_gun = null;

        // Call UpdateNumberValue on all linked cards
        CardMaster[] linked = new CardMaster[] {up_link_cardmaster, left_link_cardmaster, right_link_cardmaster, down_link_cardmaster};
        foreach (var link in linked)
        {
            if (link != null)
            {
                if (link.card_type == CardType.Gun)
                {
                    // If the link is a gun card, we should apply the buff at the very end
                    CardMaster.OnApplyValuesToGuns += () => link.UpdateNumberValue(CardMaster.NumberType.Health, attackToAdd, instance);
                }
                else
                {
                    // If the link is a value card, we can add attack to it
                    link.UpdateNumberValue(CardMaster.NumberType.Health, attackToAdd, instance);
                }

            }
        }

        base.OnCardEnable();
    }


    public override void OnCardDisable()
    {
        base.OnCardDisable(); // Call the base method to clear the current gun reference
    }

    public override void Reset()
    {
        attackToAdd = attackToAddDefault;
        base.Reset(); // Call the base reset method to reset other properties
    }
    
    // return the formatted description of the card
    public override string GetDescription()
    {
        return string.Format(card_description, attackToAdd);
    }

    public override void UpdateNumberValue(CardMaster.NumberType numberType, float value, CardMaster source = null)
    {

        if (IsBuffedFromSource(source, addToList: true, includeSelf: true)) return;
        base.UpdateNumberValue(numberType, value, source);

        if (numberType == CardMaster.NumberType.Health)
        {
            attackToAdd += value;
        }

    }

    public override void UpdateSelfNumberValue(CardMaster.NumberType numberType, float value, bool isPermanent = false)
    {
        if (numberType == CardMaster.NumberType.Health && isPermanent)
        {
            attackToAdd += value;
            attackToAddDefault += value;
        } 
        else if (numberType == CardMaster.NumberType.Health && !isPermanent)
        {
            attackToAdd += value;
        }

        base.UpdateSelfNumberValue(numberType, value);
    }
}
