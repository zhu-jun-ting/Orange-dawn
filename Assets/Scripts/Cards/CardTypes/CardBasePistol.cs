using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardBasePistol : CardMaster
{
    
    // public string gun_name = "Pistol";
    public GameObject gunPrefab; 

    private PlayerController player;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        player = PlayerController.instance;
        if (player == null || gunPrefab == null) return;
        
        // Instantiate and parent under player, set inactive
        var go = Instantiate(gunPrefab, player.transform);
        go.SetActive(false);
        current_gun = go.GetComponent<Gun>();
        
    }
    private void OnEnable()
    {
        if (current_gun != null)
            current_gun.gameObject.SetActive(true);
    }

    void OnDisable()
    {
        if (current_gun != null)
            current_gun.gameObject.SetActive(false);
    }

    public override void OnCardEnable()
    {
        if (current_gun == null)
        {
            current_gun = FindActivePistolOnPlayer();
        }
        if (current_gun != null)
            current_gun.gameObject.SetActive(true);
        base.OnCardEnable();
    }

    public override void OnCardDisable()
    {
        if (current_gun != null)
            current_gun.gameObject.SetActive(false);
        base.OnCardDisable();
    }

    public override void OnCardDestroyed()
    {
        base.OnCardDestroyed();
        if (current_gun != null)
        {
            Destroy(current_gun.gameObject);
            current_gun = null;
        }
        
    }

    public override string GetDescription()
    {
        if (current_gun == null) current_gun = FindActivePistolOnPlayer();
        if (current_gun != null)
        {
            return GameSettings.AddIcon(String.Format(card_description,
                current_gun.damage, current_gun.speed, current_gun.recon, current_gun.interval, current_gun.critChance, current_gun.critDamage, current_gun.bulletNum, current_gun.bulletAngle, current_gun.penetrate));
        }
        return "";
    }

    private Gun FindActivePistolOnPlayer()
    {
        // Always return the current_gun reference
        return current_gun;
    }

    public override bool UpdateNumberValue(CardMaster.NumberType numberType, float value, CardMaster source)
    {
        if (IsBuffedFromSource(source, addToList:true, includeSelf:true))
        {
            return false;
        }

        base.UpdateNumberValue(numberType, value, source);


        if (current_gun == null) return false;

        if (numberType == CardMaster.NumberType.Damage)
        {
            current_gun.damage += value;

            // Only show popup if this card or source card is lastDraggedCard
            var lastDragged = BoardArea.instance != null ? BoardArea.instance.lastDraggedCard : null;
            if (lastDragged == this || (source != null && lastDragged == source))
            {
                var cardCommon = GetComponent<CardCommon>();
                if (cardCommon != null) cardCommon.ShowPopup($"+ {value} Damage");
            }
            return true;
        }
        else
        {
            // Only show warning if this card or source card is lastDraggedCard
            var lastDragged = BoardArea.instance != null ? BoardArea.instance.lastDraggedCard : null;
            if (lastDragged == this || (source != null && lastDragged == source))
            {
                GameEvents.instance.ShowMessage(
                    $"UpdateNumberValue not implemented for {instance.name}. NumberType: {numberType}, Value: {value}",
                    GameEvents.MessageType.FullWarning,
                    Vector2.zero
                );
            }
            return false;
        }
    }

    public override void Reset()
    {
        if (current_gun != null) current_gun.Reset(); 
        if (current_gun != null) current_gun.gameObject.SetActive(false);
        base.Reset(); // Call the base reset method to reset other properties
    }
}