using System.Collections;
using UnityEngine;

public class CardValueBonePile : CardMaster
{
    // id: 216
    // name: Bone Pile
    // Each time this card is discarded, it returns to your hand but loses 2 Damage permanently. Destroyed forever if Damage <= 0.

    // id: 217
    // name: Bone Pile
    // desc: Each time this card being discarded will give you back but Health: -5 Permanently (Will destory forever if Health: <=0) Health: 20

    [Header("BonePile Settings")]
    public float damageDiffOnDiscard = 2f;
    public float healthDiffOnDiscard = 2f;

    public override bool OnCardDestroyed()
    {
        // Called when the card is discarded
        damage += damageDiffOnDiscard;
        default_damage += damageDiffOnDiscard;
        health += healthDiffOnDiscard;
        default_health += healthDiffOnDiscard;
        if (damage > 0)
        {
            // Return to hand
            if (CardManager.instance != null && CardManager.instance.handArea != null)
            {
                CardManager.instance.handArea.MoveCardToHand(this, null);
            }
            return false; // Not destroyed, just returned to hand
        }
        else
        {
            // Destroy forever
            return base.OnCardDestroyed();
        }
    }

    public override string GetDescription()
    {
        if (damageDiffOnDiscard != 0f) return GameSettings.AddIcon(string.Format(card_description, damageDiffOnDiscard, damage));
        else if (healthDiffOnDiscard != 0f) return GameSettings.AddIcon(string.Format(card_description, healthDiffOnDiscard, health));
        else return GameSettings.AddIcon(string.Format(card_description, damageDiffOnDiscard, damage));
    }
}
