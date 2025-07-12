using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardValueAttackDice : CardMaster
{
    // id: 202
    // name: Attack Dice
    // add 1~10 ATK, change at turn ends

    [Header("Attack Dice Settings")]
    public int diceRange = 10;

    private System.Random rng = new System.Random();

    public override void OnCardLevelCleared()
    {
        // Each time, roll a new attack value between default_damage and default_damage + diceRange (inclusive)
        int min = Mathf.RoundToInt(default_damage);
        int max = min + diceRange;
        damage = rng.Next(min, max + 1);
        base.OnCardLevelCleared();
    }

    protected override void Awake()
    {
        base.Awake();
        // Initialize with a random value on spawn
        int min = Mathf.RoundToInt(default_damage);
        int max = min + diceRange;
        damage = rng.Next(min, max + 1);
    }

    public override string GetDescription()
    {
        int min = Mathf.RoundToInt(default_damage);
        int max = min + diceRange;
        return GameSettings.AddIcon(string.Format(card_description, damage, min, max));
    }
}
