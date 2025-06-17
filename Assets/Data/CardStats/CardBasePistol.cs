using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardBasePistol : CardMaster
{

    protected override void Awake() {
        base.Awake();
    }
    private void OnEnable()
    {
        // UpdateDescriptionWithPistolStats();
        // Debug.Log($"CardBasePistol OnEnaable: {instance.name}, current_gun: {current_gun}");
    }

    void OnDisable()
    {

    }

    public override void OnCardEnable()
    {
        if (current_gun == null)
        {
            current_gun = FindActivePistolOnPlayer();
        }
        base.OnCardEnable(); // Call the base method to store the current gun reference
    }

    public override void OnCardDisable()
    {
        // Optionally clear or reset description here if needed
        base.OnCardDisable(); // Call the base method to clear the current gun reference
    }

    public override string GetDescription()
    {
        if (current_gun == null) current_gun = FindActivePistolOnPlayer();
        if (current_gun != null)
        {
            return String.Format(card_description,
                current_gun.damage, current_gun.speed, current_gun.recon, current_gun.interval, current_gun.critChance, current_gun.critDamage, current_gun.bulletNum, current_gun.bulletAngle, current_gun.penetrate);
        }
        return "";
    }

    private Gun FindActivePistolOnPlayer()
    {
        var player = PlayerController.instance;
        if (player != null && player.guns != null && player.guns.Length > 0)
        {
            var gunNumField = typeof(PlayerController).GetField("gunNum", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            int gunNum = gunNumField != null ? (int)gunNumField.GetValue(player) : 0;
            if (gunNum >= 0 && gunNum < player.guns.Length && player.guns[gunNum] != null)
            {
                var gun = player.guns[gunNum].GetComponent<Gun>();
                if (gun != null && gun.name.ToLower().Contains("pistol"))
                    return gun;
            }
        }
        return null;
    }

    public override void UpdateNumberValue(CardMaster.NumberType numberType, float value, CardMaster source)
    {
        if (IsBuffedFromSource(source, addToList:true, includeSelf:true))
        {
            return;
        }

        if (!(source != null && IsChildren(source)))
        {
            return;
        }

        base.UpdateNumberValue(numberType, value, source);


        if (current_gun == null) return;

        if (numberType == CardMaster.NumberType.Damage)
        {
            current_gun.damage += value;
            Debug.Log($"UpdateNumberValue from: {source.card_name} - Damage increase {value} New Damage: {current_gun.damage}");
        }
        else
        {
            Debug.LogError($"UpdateNumberValue not implemented for {instance.name}. NumberType: {numberType}, Value: {value}");
        }
    }

    public override void Reset()
    {
        if(current_gun != null) current_gun.Reset(); 
        base.Reset(); // Call the base reset method to reset other properties
    }
}