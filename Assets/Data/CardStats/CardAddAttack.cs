using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardStats", menuName = "Cards/CardAddAttackStats", order = 1)]

public class CardAddAttack : CardMaster {

    public float attackToAdd = 10f;

    public override void OnCardEnable(Gun gun)
    {
        if (gun == null)
        {
            Debug.LogError("Gun is null in CardAddAttack.OnCardEnable");
            return;
        }

        // Add attack to the gun
        gun.damage += attackToAdd;
        // Debug.Log($"Added attack to {gun.name}. New attack value: {gun.damage}");

        base.OnCardEnable(gun); // Call the base method to store the current gun reference
    }

    public override void OnCardDisable(Gun gun)
    {
        if (gun == null)
        {
            Debug.LogError("Gun is null in CardAddAttack.OnCardEnable");
            return;
        }

        // Add attack to the gun
        gun.damage -= attackToAdd;
        // Debug.Log($"Added attack to {gun.name}. New attack value: {gun.damage}");

        base.OnCardDisable(gun); // Call the base method to clear the current gun reference
    }
} 
