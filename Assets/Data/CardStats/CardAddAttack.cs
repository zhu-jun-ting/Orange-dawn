using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardAddAttack : CardMaster
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
        Gun foundGun = null;
        CardMaster[] linked = new CardMaster[] {
            up_link_cardmaster, left_link_cardmaster, right_link_cardmaster, down_link_cardmaster
        };
        foreach (var link in linked)
        {
            if (link != null && link.current_gun != null)
            {
                foundGun = link.current_gun;
                break;
            }
        }
        current_gun = foundGun; // Set to found gun or null if none

        // Debug.Log($"up link_cardmaster: {up_link_cardmaster}, left_link_cardmaster: {left_link_cardmaster}, right_link_cardmaster: {right_link_cardmaster}, down_link_cardmaster: {down_link_cardmaster}");

        // Call UpdateNumberValue on all linked cards
        foreach (var link in linked)
        {
            if (link != null)
            {
                link.UpdateNumberValue(CardMaster.NumberType.Damage, attackToAdd, instance);
            }
        }

        card_description = $"Adds {attackToAdd} attack to the gun.";
        base.OnCardEnable();
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
        card_description = $"Adds {attackToAdd} attack to the gun.";
        base.Reset(); // Call the base reset method to reset other properties
        
    }

    public override void UpdateNumberValue(CardMaster.NumberType numberType, float value, CardMaster source = null)
    {

        if (IsBuffedFromSource(source, addToList:true, includeSelf:true))
        {
            return;
        }

        // if source is a children of me, then I should take the buff from source
        if (!(source != null && IsChildren(source)))
        {
            return;
        }

        base.UpdateNumberValue(numberType, value, source);

        if (numberType == CardMaster.NumberType.Damage)
        {
            attackToAdd += value;
            card_description = $"Adds {attackToAdd} attack to the gun.";
        }
        else
        {
            Debug.LogError($"UpdateNumberValue not implemented for {this.name}. NumberType: {numberType}, Value: {value}");
        }
    }
}
