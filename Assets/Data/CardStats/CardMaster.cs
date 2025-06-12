using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMaster : ScriptableObject {
    // common for all cards
    public Sprite card_icon;
    public string card_id; // unique identifier for the card
    public string card_name;
    public string card_description;
    public Gun current_gun; 

    // events to update card values and texts
    public static event System.Action OnUpdateCardValues;
    public static event System.Action OnUpdateCardTexts;



    // Enum for different link types
    public enum LinkType
    {
        Common,
        Red,
        Green,
        Blue,
    }



    // Grouped link settings in inspector
    [Header("Up Link Settings")]
    public bool up_link_enabled = false;
    public CardMaster.LinkType up_link_type = CardMaster.LinkType.Common;
    [HideInInspector] public CardMaster up_link_cardmaster = null;



    [Header("Left Link Settings")]
    public bool left_link_enabled = false;
    public CardMaster.LinkType left_link_type = CardMaster.LinkType.Common;
    [HideInInspector] public CardMaster left_link_cardmaster = null;



    [Header("Right Link Settings")]
    public bool right_link_enabled = false;
    public CardMaster.LinkType right_link_type = CardMaster.LinkType.Common;
    [HideInInspector] public CardMaster right_link_cardmaster = null;



    [Header("Down Link Settings")]
    public bool down_link_enabled = false;
    public CardMaster.LinkType down_link_type = CardMaster.LinkType.Common;
    [HideInInspector] public CardMaster down_link_cardmaster = null;




    [Header("Card Properties")]
    public bool is_free_card = false; // If true, card can be placed anywhere regardless of link restrictions




    [Header("Link GameObjects")]
    [Tooltip("GameObject to show the up link connection in the UI")]
    public GameObject up_link_gameobject;
    public GameObject left_link_gameobject;
    public GameObject right_link_gameobject;
    public GameObject down_link_gameobject;



    public virtual void OnCardEnable(Gun gun)
    {
        current_gun = gun; // Store the current gun reference
        OnUpdateCardValues?.Invoke();
        OnUpdateCardTexts?.Invoke();
    }
    public virtual void OnCardDisable(Gun gun)
    {
        current_gun = null; // Clear the current gun reference
        OnUpdateCardValues?.Invoke();
        OnUpdateCardTexts?.Invoke(); 
    }

    public void SetCardName(string name)
    {
        card_name = name;
        OnUpdateCardTexts?.Invoke();
    }

    public void SetCardDescription(string description)
    {
        card_description = description;
        OnUpdateCardTexts?.Invoke();
    }

}