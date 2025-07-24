using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// this is a reference of the first PingPong player have
// this updats damage and bulletNum 
// damage use gun.damage
// bulletNum use gun.bulletNum

public class CardGunPingPong : CardMaster
{

    // public string gun_name = "Pistol";
    public GameObject gunPrefab;
    public Sprite bulletSprite;

    private PlayerController player;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        base.Start();
        player = PlayerController.instance;
        if (player == null || gunPrefab == null) return;

        // Instantiate and parent under player, set inactive
        var go = Instantiate(gunPrefab, player.transform);
        go.SetActive(false);
        current_gun = go.GetComponent<Gun>();
        current_gun.owner = player.gameObject;

        if (!PlayerController.instance.guns.Contains(current_gun.gameObject)) PlayerController.instance.guns.Add(current_gun.gameObject);
        if (bulletSprite != null && current_gun != null)
        {
            current_gun.SetBulletSprite(bulletSprite);
        }
    }

    public override void OnCardEnable()
    {
        if (current_gun != null)
            current_gun.gameObject.SetActive(true);
        CardMaster.OnApplyValuesToGuns -= HandleOnApplyValuesToGuns;
        CardMaster.OnApplyValuesToGuns += HandleOnApplyValuesToGuns;
        base.OnCardEnable();
    }

    public override void OnCardDisable()
    {
        if (current_gun != null)
            current_gun.gameObject.SetActive(false);
        base.OnCardDisable();
    }

    public override bool OnCardDestroyed()
    {
        if (current_gun != null)
        {
            Destroy(current_gun.gameObject);
            current_gun = null;
            return true;
        }
        return base.OnCardDestroyed();
    }

    public override string GetDescription()
    {
        if (current_gun != null)
        {
            return GameSettings.AddIcon(String.Format(card_description, (int)damage, (int)amount));
        }
        return "";
    }

    public void HandleOnApplyValuesToGuns()
    {
        // apply the value changes to player
        current_gun.damage += (int)damage;
        current_gun.bulletNum += (int)amount; // Assuming amount is used for bullet number
        if (current_gun.bulletNum >= 2) current_gun.bulletNum -= 1; 
        if (current_gun.bulletNum >= 10) current_gun.bulletNum = 10; // Ensure bulletNum is always less than 10
    }

    public override void Reset()
    {
        if (current_gun != null) current_gun.Reset();
        if (current_gun != null) current_gun.gameObject.SetActive(false);
        CardMaster.OnApplyValuesToGuns -= HandleOnApplyValuesToGuns;
        base.Reset(); // Call the base reset method to reset other properties
    }
}