using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardConditionOnHitTarget : CardMaster
{


    [Header("Card Settings")]
    public List<CardMaster.CardDir> triggerDirections;
    public int manaCost = 5;


    public override void OnCardEnable()
    {
        // Try to find the gun reference from linked cards
        current_gun = GetLinkedGun();
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitPawn += HandleOnHitPawn;
        base.OnCardEnable();
    }

    public override void OnCardDisable()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitPawn -= HandleOnHitPawn;
        base.OnCardDisable();
    }

    private void HandleOnHitPawn(float damage, PawnMaster receiver, GameObject instigator, GameEvents.DamageType damageType, Transform location, Gun source)
    {
        if (receiver != null && receiver.CompareTag("Enemy") && source == current_gun)
        {
            if (!ManaBar.CanCostMana(-manaCost))
                return;
            bool hasTriggered = false;
            foreach (var dir in triggerDirections)
            {
                CardMaster linked = null;
                bool enabled = false;
                switch (dir)
                {
                    case CardDir.Up:
                        linked = up_link_cardmaster; enabled = up_link_enabled; break;
                    case CardDir.Left:
                        linked = left_link_cardmaster; enabled = left_link_enabled; break;
                    case CardDir.Right:
                        linked = right_link_cardmaster; enabled = right_link_enabled; break;
                    case CardDir.Down:
                        linked = down_link_cardmaster; enabled = down_link_enabled; break;
                }
                if (enabled && linked != null && linked is ICardAction action)
                {
                    action.TriggerAction(this, receiver.transform);
                    hasTriggered = true; // Mark that at least one action was triggered
                }
            }
            if(hasTriggered) GameEvents.instance.UpdateMana(-manaCost);
        }
    }



    public override void Reset()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitPawn -= HandleOnHitPawn;
        base.Reset(); // Call the base reset method to reset other properties
    }
    
    // return the formatted description of the card
    public override string GetDescription()
    {
        return string.Format(card_description, manaCost);
    }



    public override void UpdateNumberValue(CardMaster.NumberType numberType, float value, CardMaster source = null)
    {

        if (IsBuffedFromSource(source, addToList: true, includeSelf: true)) return;

        // if source is a children of me, then I should take the buff from source
        // TODO: decide whether to allow buffs from children or not
        // if (!(source != null && IsChildren(source))) return;



        base.UpdateNumberValue(numberType, value, source);

        if (numberType == CardMaster.NumberType.Mana)
        {
            manaCost += (int)value;
        }
        else
        {
            
        }
    }
}
