using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardBasePistol", menuName = "Cards/CardBasePistol", order = 2)]

public class CardBasePistol : CardMaster
{
    private void OnEnable()
    {
        CardMaster.OnUpdateCardValues += UpdateDescriptionWithPistolStats;
        UpdateDescriptionWithPistolStats();
    }

    void OnDisable()
    {
        CardMaster.OnUpdateCardValues -= UpdateDescriptionWithPistolStats;
    }

    public override void OnCardEnable(Gun gun)
    {
        if (gun == null)
        {
            Debug.LogError("Gun is null in CardBasePistol.OnCardEnable");
            return;
        }
        UpdateDescriptionWithPistolStats();
    }

    public override void OnCardDisable(Gun gun)
    {
        // Optionally clear or reset description here if needed
    }

    private void UpdateDescriptionWithPistolStats()
    {
        if (current_gun == null) current_gun = FindActivePistolOnPlayer();
        if (current_gun != null)
        {
            string desc = $"Damage: {current_gun.damage}\nSpeed: {current_gun.speed}\nRecon: {current_gun.recon}\nInterval: {current_gun.interval}";
            Debug.Log($"Damage: {current_gun.damage}\nSpeed: {current_gun.speed}\nRecon: {current_gun.recon}\nInterval: {current_gun.interval}");
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
}