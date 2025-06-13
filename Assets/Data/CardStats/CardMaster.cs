using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CardMaster : MonoBehaviour
{
    // common for all cards
    public Sprite card_icon;
    public string card_id; // unique identifier for the card
    public string card_name;
    public string card_description;
    public Gun current_gun;

    // events to update card values and texts
    //   OnUpdateCardValues: perform a BFS from root card to update all linked cards' values
    //   OnUpdateBaseDesctipion: update the base description of the card
    //   OnUpdateCardTexts: update the card texts in the UI
    public static event System.Action OnUpdateCardValues;
    public static event System.Action OnUpdateBaseDesctipion;
    public static event System.Action OnUpdateCardTexts;




    // Enum for different link types
    public enum LinkType
    {
        Common,
        Red,
        Green,
        Blue,
    }


    public enum NumberType
    {
        Damage,
        Health,
        Probablity,
        Amount,
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
    public bool is_root = false; // if true, this card is the root of the card tree that traverse from this card




    [Header("Link GameObjects")]
    [Tooltip("GameObject to show the up link connection in the UI")]
    public GameObject up_link_gameobject;
    public GameObject left_link_gameobject;
    public GameObject right_link_gameobject;
    public GameObject down_link_gameobject;


    protected CardMaster instance;

    protected virtual void Awake()
    {
        instance = this;
    }

    public virtual void OnCardEnable()
    {
        // OnUpdateCardValues?.Invoke();
        // OnUpdateBaseDesctipion?.Invoke();
        // OnUpdateCardTexts?.Invoke();
    }
    public virtual void OnCardDisable()
    {
        // OnUpdateCardValues?.Invoke();
        // OnUpdateBaseDesctipion?.Invoke();
        // OnUpdateCardTexts?.Invoke(); 
    }

    public virtual void Reset()
    {
        ClearUpdateSources();
    }

    private HashSet<CardMaster> updateSources = new HashSet<CardMaster>();

    // check if this card is buffed from a specific source
    public bool IsBuffedFromSource(CardMaster source, bool addToList = true, bool includeSelf = true)
    {

        if (includeSelf && source == this) return true;
        if (source == null) return true;
        // Check if the source is already in the update sources
        if (updateSources.Count == 0) return false;

        if (updateSources.Contains(source))
        {
            return true;
        }
        else
        {
            if (addToList)
            {
                updateSources.Add(source);
            }
            return false;
        }
    }

    public virtual void UpdateNumberValue(CardMaster.NumberType numberType, float value, CardMaster source = null)
    {

    }

    // Call this at the start of each propagation/update cycle to clear the set
    public void ClearUpdateSources()
    {
        updateSources.Clear();
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

    // Static methods to safely invoke events from outside this class
    public static void InvokeUpdateCardValues()
    {
        OnUpdateCardValues?.Invoke();
    }
    public static void InvokeUpdateBaseDesctipion()
    {
        OnUpdateBaseDesctipion?.Invoke();
    }
    public static void InvokeUpdateCardTexts()
    {
        OnUpdateCardTexts?.Invoke();
    }

    // Returns true if this card is a parent of the source card in the same tree (using reversed BFS order)
    public bool IsChildren(CardMaster source)
    {
        var board = BoardArea.instance;
        if (board == null || board.roots == null) return false;

        foreach (var root in board.roots)
        {
            var bfs = BoardArea.GetOrderedBFSFromRoot(root);
            int thisIdx = bfs.IndexOf(this);
            int sourceIdx = bfs.IndexOf(source);
            if (thisIdx != -1 && sourceIdx != -1)
            {
                // In reversed BFS, parent is after child (root is last)
                return sourceIdx < thisIdx;
            }
        }
        return false;
    }
    
    
}