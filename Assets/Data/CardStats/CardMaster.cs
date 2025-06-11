using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMaster : ScriptableObject {
    // common for all cards
    public Sprite card_icon;
    public string card_id; // unique identifier for the card
    public string card_name;
    public string card_description;


    public virtual void OnCardEnable(Gun gun) {}
    public virtual void OnCardDisable(Gun gun) {}
}
