using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardValueTestShouldDesotryAfterLevelCleared : CardMaster
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
        // Debug.Log($"CardAddAttack OnEnaable: {instance.name}, current_gun: {current_gun}");

        // Try to find the gun reference from linked cards
        current_gun = GetLinkedGun();

        // Call UpdateNumberValue on all linked cards
        CardMaster[] linked = new CardMaster[] {up_link_cardmaster, left_link_cardmaster, right_link_cardmaster, down_link_cardmaster};
        foreach (var link in linked)
        {
            if (link != null)
            {
                if (link.card_type == CardType.Value)
                {
                    // If the link is a value card, we can add attack to it
                    link.UpdateNumberValue(CardMaster.NumberType.Damage, attackToAdd, instance);
                }
                else
                {
                    // If the link is a gun card, we should apply the buff at the very end
                    CardMaster.OnApplyValuesToGuns += () => link.UpdateNumberValue(CardMaster.NumberType.Damage, attackToAdd, instance);
                }

            }
        }

        // card_description = string.Format(card_description, attackToAdd);
        base.OnCardEnable();
    }

    public override void OnCardLevelCleared()
    {
        OnCardDestroyed();
    }


    public override void OnCardDisable()
    {

        // Add attack to the gun
        // gun.damage -= attackToAdd;

        base.OnCardDisable(); // Call the base method to clear the current gun reference
    }

    public override void Reset()
    {
        attackToAdd = attackToAddDefault;
        // card_description = string.Format(card_description, attackToAdd);
        base.Reset(); // Call the base reset method to reset other properties
        // Debug.Log("Reset ---" + string.Format(card_description, attackToAdd));

    }
    
    // return the formatted description of the card
    public override string GetDescription()
    {
        return string.Format(card_description, attackToAdd);
    }

    public override void UpdateNumberValue(CardMaster.NumberType numberType, float value, CardMaster source = null)
    {

        if (IsBuffedFromSource(source, addToList: true, includeSelf: true)) return;

        // if source is a children of me, then I should take the buff from source
        // TODO: decide whether to allow buffs from children or not
        // if (!(source != null && IsChildren(source))) return;



        base.UpdateNumberValue(numberType, value, source);

        if (numberType == CardMaster.NumberType.Damage)
        {
            attackToAdd += value;
            // card_description = string.Format(card_description, attackToAdd);
            // Debug.Log(string.Format(card_description, attackToAdd));
        }

    }
}
