using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardBasePistol : CardMaster
{

    protected override void Awake() {
        base.Awake();
    }
    private void OnEnable()
    {
        CardMaster.OnUpdateBaseDesctipion += UpdateDescriptionWithPistolStats;
        UpdateDescriptionWithPistolStats();

        // Debug.Log($"CardBasePistol OnEnaable: {instance.name}, current_gun: {current_gun}");
    }

    void OnDisable()
    {
        CardMaster.OnUpdateBaseDesctipion -= UpdateDescriptionWithPistolStats;
    }

    public override void OnCardEnable()
    {
        if (current_gun == null)
        {
            current_gun = FindActivePistolOnPlayer();
        }
        UpdateDescriptionWithPistolStats();
        base.OnCardEnable(); // Call the base method to store the current gun reference
    }

    public override void OnCardDisable()
    {
        // Optionally clear or reset description here if needed
        base.OnCardDisable(); // Call the base method to clear the current gun reference
    }

    private void UpdateDescriptionWithPistolStats()
    {
        if (current_gun == null) current_gun = FindActivePistolOnPlayer();
        if (current_gun != null)
        {
            string desc = $"Damage: {current_gun.damage}\nSpeed: {current_gun.speed}\nRecon: {current_gun.recon}\nInterval: {current_gun.interval}";
            // Debug.Log($"Damage: {current_gun.damage}\nSpeed: {current_gun.speed}\nRecon: {current_gun.recon}\nInterval: {current_gun.interval}");
            // Directly set the field, do NOT call SetCardDescription to avoid recursion
            card_description = desc;
        }
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
            UpdateDescriptionWithPistolStats();
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