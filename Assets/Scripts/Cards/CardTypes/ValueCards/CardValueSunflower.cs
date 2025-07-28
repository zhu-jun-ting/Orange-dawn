using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardValueSunflower : CardMaster
{
    // id: 201
    // name: Sunflower  
    // add 1 ATK, increase by 1 when turn ends

    [Header("Card Add Attack Settings")]
    public float incrementAfterLevelCleared = 1f;

    public override void OnCardLevelCleared()
    {
        // Increment the attack to add after each level cleared
        damage += incrementAfterLevelCleared;
        default_damage += incrementAfterLevelCleared;
        base.OnCardLevelCleared();
    }
    
    // return the formatted description of the card
    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), damage, incrementAfterLevelCleared));
    }
}
