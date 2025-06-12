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

    public static event System.Action OnUpdateCardValues;
    public static event System.Action OnUpdateCardTexts;

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