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
    [TextArea(3, 10)] public string card_description;
    public Gun current_gun;

    // events to update card values and texts
    //   OnUpdateCardValues: perform a BFS from root card to update all linked cards' values
    //   OnUpdateBaseDesctipion: update the base description of the card
    //   OnUpdateCardTexts: update the card texts in the UI
    public static event System.Action OnUpdateCardValues;
    public static event System.Action OnApplyValuesToGuns;
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
        Mana,
    }

    public enum CardType
    {
        Value,
        Gun,
        Spawner,
    }

    public enum CardDir
    {
        Up,
        Left,
        Right,
        Down,
    }



    // Grouped link settings in inspector
    [Header("Up Link Settings")]
    public bool up_link_enabled = false;
    public CardMaster.LinkType up_link_type = CardMaster.LinkType.Common;
    public CardMaster up_link_cardmaster = null;



    [Header("Left Link Settings")]
    public bool left_link_enabled = false;
    public CardMaster.LinkType left_link_type = CardMaster.LinkType.Common;
    public CardMaster left_link_cardmaster = null;



    [Header("Right Link Settings")]
    public bool right_link_enabled = false;
    public CardMaster.LinkType right_link_type = CardMaster.LinkType.Common;
    public CardMaster right_link_cardmaster = null;



    [Header("Down Link Settings")]
    public bool down_link_enabled = false;
    public CardMaster.LinkType down_link_type = CardMaster.LinkType.Common;
    public CardMaster down_link_cardmaster = null;




    [Header("Card Properties")]
    public bool is_free_card = false; // If true, card can be placed anywhere regardless of link restrictions
    public bool is_root = false; // if true, this card is the root of the card tree that traverse from this card
    public CardType card_type = CardType.Value; // if true, this card is the root of the card tree that traverse from this card




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

    public virtual void UpdateNumberValue(CardMaster.NumberType numberType, float value, CardMaster source = null)
    {

    }

    // you should override this method to return the card's name and description
    // if you want to use the default implementation, just return card_name
    public virtual string GetName()
    {
        return card_name;
    }

    public virtual string GetDescription()
    {
        return card_description;
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


    public virtual void OnCardLevelCleared() {
        
    }

    public virtual void OnCardDestroyed() {
        // Remove references from linked cards before destroying this card
        // Up
        if (up_link_cardmaster != null && up_link_cardmaster.down_link_cardmaster == this)
        {
            up_link_cardmaster.down_link_cardmaster = null;
            // Set up link visual to black 0.5 transparent
            up_link_cardmaster.SetLinkHalfTransparentBlack("down");
            this.SetLinkHalfTransparentBlack("up");
            up_link_cardmaster = null;
        }
        // Down
        if (down_link_cardmaster != null && down_link_cardmaster.up_link_cardmaster == this)
        {
            down_link_cardmaster.up_link_cardmaster = null;
            down_link_cardmaster.SetLinkHalfTransparentBlack("up");
            this.SetLinkHalfTransparentBlack("down");
            down_link_cardmaster = null;
        }
        // Left
        if (left_link_cardmaster != null && left_link_cardmaster.right_link_cardmaster == this)
        {
            left_link_cardmaster.right_link_cardmaster = null;
            left_link_cardmaster.SetLinkHalfTransparentBlack("right");
            this.SetLinkHalfTransparentBlack("left");
            left_link_cardmaster = null;
        }
        // Right
        if (right_link_cardmaster != null && right_link_cardmaster.left_link_cardmaster == this)
        {
            right_link_cardmaster.left_link_cardmaster = null;
            right_link_cardmaster.SetLinkHalfTransparentBlack("left");
            this.SetLinkHalfTransparentBlack("right");
            right_link_cardmaster = null;
        }
        // --- Remove all board references like dragging off board ---
        if (BoardArea.instance != null && BoardArea.instance.gridState != null)
        {
            var grid = BoardArea.instance.gridState;
            int rows = BoardArea.instance.rows;
            int cols = BoardArea.instance.columns;
            // Find this card's position on the board
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (grid[row, col] == this)
                    {
                        // Up
                        if (row > 0)
                        {
                            var upCard = grid[row - 1, col];
                            if (upCard != null && upCard.down_link_cardmaster == this)
                            {
                                upCard.down_link_cardmaster = null;
                                this.up_link_cardmaster = null;
                            }
                        }
                        // Down
                        if (row < rows - 1)
                        {
                            var downCard = grid[row + 1, col];
                            if (downCard != null && downCard.up_link_cardmaster == this)
                            {
                                downCard.up_link_cardmaster = null;
                                this.down_link_cardmaster = null;
                            }
                        }
                        // Left
                        if (col > 0)
                        {
                            var leftCard = grid[row, col - 1];
                            if (leftCard != null && leftCard.right_link_cardmaster == this)
                            {
                                leftCard.right_link_cardmaster = null;
                                this.left_link_cardmaster = null;
                            }
                        }
                        // Right
                        if (col < cols - 1)
                        {
                            var rightCard = grid[row, col + 1];
                            if (rightCard != null && rightCard.left_link_cardmaster == this)
                            {
                                rightCard.left_link_cardmaster = null;
                                this.right_link_cardmaster = null;
                            }
                        }
                        BoardArea.instance.ClearCell(row, col);
                        break;
                    }
                }
            }
        }

        // Move this card to the hand area before destroying
        if (HandArea.instance != null)
        {
            this.transform.SetParent(HandArea.instance.transform, false);
            HandArea.instance.AddCard(this);
        }

        // make the destroyed card invisible
        this.transform.position = new Vector3(10000, 10000, 10000);

        CardDragHandler.TriggerUpdateCards();

    }

    // --- Events Callers ---
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
    public static void InvokeApplyValuesToGuns()
    {
        OnApplyValuesToGuns?.Invoke();
    }


    // --- Helper Functions ---
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


    // --- Link Visuals ---
    // Use CardDragHandler's link GameObjects only
    private GameObject GetLinkGameObject(string dir)
    {
        var dragHandler = GetComponent<CardDragHandler>();
        if (dragHandler != null)
        {
            switch (dir)
            {
                case "up": return dragHandler.up_link_gameobject;
                case "left": return dragHandler.left_link_gameobject;
                case "right": return dragHandler.right_link_gameobject;
                case "down": return dragHandler.down_link_gameobject;
            }
        }
        return null;
    }

    public void SetAllLinksHalfTransparent()
    {
        SetLinkAlpha(GetLinkGameObject("up"), 0.5f);
        SetLinkAlpha(GetLinkGameObject("left"), 0.5f);
        SetLinkAlpha(GetLinkGameObject("right"), 0.5f);
        SetLinkAlpha(GetLinkGameObject("down"), 0.5f);
    }

    public void SetAllLinksInvisible()
    {
        SetLinkAlpha(GetLinkGameObject("up"), 0f);
        SetLinkAlpha(GetLinkGameObject("left"), 0f);
        SetLinkAlpha(GetLinkGameObject("right"), 0f);
        SetLinkAlpha(GetLinkGameObject("down"), 0f);
    }

    public void SetLinkInvisible(string dir)
    {
        var go = GetLinkGameObject(dir);
        if (go != null)
        {
            SetLinkAlpha(go, 0f);
        }
    }

    public void SetActiveLinksGreenAndVisible(bool up, bool left, bool right, bool down)
    {
        if (up) SetLinkColor(GetLinkGameObject("up"), Color.green, 1f); else if (!up_link_enabled) SetLinkAlpha(GetLinkGameObject("up"), 0f);
        if (left) SetLinkColor(GetLinkGameObject("left"), Color.green, 1f); else if (!left_link_enabled) SetLinkAlpha(GetLinkGameObject("left"), 0f);
        if (right) SetLinkColor(GetLinkGameObject("right"), Color.green, 1f); else if (!right_link_enabled) SetLinkAlpha(GetLinkGameObject("right"), 0f);
        if (down) SetLinkColor(GetLinkGameObject("down"), Color.green, 1f); else if (!down_link_enabled) SetLinkAlpha(GetLinkGameObject("down"), 0f);
    }

    public void SetPlacedLinksGreenAndVisible(bool up, bool left, bool right, bool down)
    {
        if (up) SetLinkColor(GetLinkGameObject("up"), Color.green, 1f); else SetLinkAlpha(GetLinkGameObject("up"), 0f);
        if (left) SetLinkColor(GetLinkGameObject("left"), Color.green, 1f); else SetLinkAlpha(GetLinkGameObject("left"), 0f);
        if (right) SetLinkColor(GetLinkGameObject("right"), Color.green, 1f); else SetLinkAlpha(GetLinkGameObject("right"), 0f);
        if (down) SetLinkColor(GetLinkGameObject("down"), Color.green, 1f); else SetLinkAlpha(GetLinkGameObject("down"), 0f);
    }

    public void SetPlacedLinksColorAndAlpha(bool up, bool left, bool right, bool down, Color color, float alpha)
    {
        if (up) SetLinkColor(GetLinkGameObject("up"), color, alpha);
        if (left) SetLinkColor(GetLinkGameObject("left"), color, alpha);
        if (right) SetLinkColor(GetLinkGameObject("right"), color, alpha);
        if (down) SetLinkColor(GetLinkGameObject("down"), color, alpha);
    }

    public void SetPlacedLinksColorAndAlpha(String dir, Color color, float alpha)
    {
        var go = GetLinkGameObject(dir);
        if (go != null) SetLinkColor(go, color, alpha);
    }

    // Make SetLinkAlpha public for use by CardDragHandler
    public void SetLinkAlpha(GameObject go, float alpha)
    {
        if (go == null) return;
        var img = go.GetComponent<UnityEngine.UI.Image>();
        if (img != null)
        {
            var c = img.color;
            c.a = alpha;
            img.color = c;
        }
        // Debug.Log("set link alpha: " + go.name + " to " + alpha);
    }
    private void SetLinkColor(GameObject go, Color color, float alpha)
    {
        if (go == null) return;
        var img = go.GetComponent<UnityEngine.UI.Image>();
        if (img != null)
        {
            color.a = alpha;
            img.color = color;
        }
    }

    // Set a specific link (by direction) to black 50% transparent
    public void SetLinkHalfTransparentBlack(string dir)
    {
        var go = GetLinkGameObject(dir);
        if (go == null) return;
        var img = go.GetComponent<UnityEngine.UI.Image>();
        if (img != null)
        {
            img.color = new Color(0f, 0f, 0f, 0.5f);
        }
    }

    public static void ClearOnApplyValuesToGuns()
    {
        if (OnApplyValuesToGuns == null) return;
        foreach (Delegate d in OnApplyValuesToGuns.GetInvocationList())
        {
            OnApplyValuesToGuns -= (System.Action)d;
        }
    }

    public Gun GetLinkedGun()
    {
        // Try to find the gun reference from linked cards
        Gun foundGun = null;
        CardMaster[] linked = new CardMaster[] {
            up_link_cardmaster, left_link_cardmaster, right_link_cardmaster, down_link_cardmaster
        };
        foreach (var link in linked)
        {
            if (link != null && link.current_gun != null)
            {
                foundGun = link.current_gun;
                break;
            }
        }
        current_gun = foundGun; // Set to found gun or null if none
        return foundGun;
    }
}