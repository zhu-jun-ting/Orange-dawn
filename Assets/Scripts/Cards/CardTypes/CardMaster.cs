using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using DG.Tweening;
using System.Numerics;

public class CardMaster : MonoBehaviour
{
    [Header("Card Settings")]
    // common for all cards
    public Sprite card_icon;
    public string card_id; // unique identifier for the card
    public string card_name;
    public int card_cost = 0;
    public int card_sell_price = 0;
    [TextArea(3, 10)] public string card_description;
    private float destroyEffectDuration => GameSettings.instance ? GameSettings.instance.destroyEffectDuration : 0.5f;

    [Header("Card UIStars")]
    [Tooltip("Positions for UI stars, relative to the card's grid location (X is downwards and Y is rightwards). E.g. (0, 0) is the card's position, (1, 0) is one cell to the down.")]
    public List<UnityEngine.Vector2Int> uiStarPositions = new List<UnityEngine.Vector2Int>(); // Positions for UI stars, if any
    public List<NumberType> numberTypesCanBeModified = new List<NumberType>(); // Types of numbers that can be buffed by this card


    // current gun reference, used for gun cards
    [HideInInspector] public Gun current_gun;

    // buff entry for Instant cards
    protected BuffEntry buffEntry;

    // events to update card values and texts
    //   OnUpdateCardValues: perform a BFS from root card to update all linked cards' values
    //   OnLateUpdateCardValues: applying card's self modifiers and board modifiers
    //   OnUpdateBaseDesctipion: update the base description of the card
    //   OnUpdateCardTexts: update the card texts in the UI
    public static event System.Action OnUpdateCardValues;
    public static event System.Action OnLateUpdateCardValues;
    public static event System.Action OnApplyValuesToGuns;
    public static event System.Action OnUpdateBaseDesctipion;
    public static event System.Action OnUpdateCardTexts;

    // events that specifies for this card
    public event System.Action OnThisCardEnable;
    public event System.Action OnThisCardSold;
    public event System.Action OnThisCardPurchased;
    public event System.Action OnThisCardDestroyed;
    public event System.Action OnThisCardLevelCleared;




    // Enum for different link types
    public enum LinkType
    {
        Common,
        Value,
        Condition,
        Action,
    }


    public enum NumberType
    {
        Damage,
        Health,
        Probability,
        Amount,
        Mana,
        Coin
    }

    public enum CardType
    {
        Base,
        Gun,
        Value,
        Action,
        Instant
    }

    public enum CardBond
    {
        Mech,
        Skull,
        Human
    }

    public enum CardRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
    }

    public enum CardDir
    {
        Up,
        Left,
        Right,
        Down,
    }

    public enum CardCondition
    {
        IsUndraggable, // If true, card cannot be dragged off the board
        IsPowerful, // If true, card value is multiplied by 2
        IsFrail, // If true, card value is halved
        IsFragile, // If true, card may be destroyed when turn ends (probability in GameSettings)
        IsTemporary, // If true, card is auto-destroyed after turn ends
        IsVolatile, // If true, card may change to another card after turn ends
        IsGrowing, // If true, card's values grow randomly each turn (possibly negative)
        IsDecaying, // If true, card's values decay randomly each turn (possibly negative)
        IsEternal, // If true, card cannot be destroyed or sold
    }



    // Grouped link settings in inspector
    [Header("Link Settings")]
    public bool useRandomLinks = false; // If true, links are randomly assigned when card is created
    [Range(1, 4)] public int linkCount = 1; // how many random links to assign
    public bool up_link_enabled = false;
    [HideInInspector] public CardMaster.LinkType up_link_type = CardMaster.LinkType.Common;
    [HideInInspector] public CardMaster up_link_cardmaster = null;
    public bool left_link_enabled = false;
    [HideInInspector] public CardMaster.LinkType left_link_type = CardMaster.LinkType.Common;
    [HideInInspector] public CardMaster left_link_cardmaster = null;
    public bool right_link_enabled = false;
    [HideInInspector] public CardMaster.LinkType right_link_type = CardMaster.LinkType.Common;
    [HideInInspector] public CardMaster right_link_cardmaster = null;
    public bool down_link_enabled = false;
    [HideInInspector] public CardMaster.LinkType down_link_type = CardMaster.LinkType.Common;
    [HideInInspector] public CardMaster down_link_cardmaster = null;




    [Header("Card Properties")]
    public bool is_free_card = false; // If true, card can be placed anywhere regardless of link restrictions
    public bool is_root = false; // if true, this card is the root of the card tree that traverse from this card
    public CardType card_type = CardType.Base; // if true, this card is the root of the card tree that traverse from this card
    public CardRarity card_rarity = CardRarity.Common; // Rarity of the card, used for UIStars
    public List<CardBond> card_bonds = new List<CardBond>(); // List of card bonds this card has, used for UIStars
    public List<CardCondition> card_conditions = new List<CardCondition>(); // List of conditions this card has

    [Header("Card Values")]
    public float damage = 0f; // Damage value of the card, used for guns
    public float health = 0f; // Health value of the card, used for health cards
    public float probability = 0f; // Probability value of the card, used for dodge or crit chance
    public float amount = 0f; // Amount value of the card, used for amount cards
    public float mana = 0f; // Mana value of the card, used for mana cards
    public float coin = 0f; // Coin value of the card, used for coin cards

    // Default values for permanent stat changes
    [HideInInspector] public float default_damage;
    [HideInInspector] public float default_health;
    [HideInInspector] public float default_probability;
    [HideInInspector] public float default_amount;
    [HideInInspector] public float default_mana;
    [HideInInspector] public float default_coin;

    // Temp values for only one level, clear after level is cleared
    [HideInInspector] public float temp_damage = 0f;
    [HideInInspector] public float temp_health = 0f;
    [HideInInspector] public float temp_probability = 0f;
    [HideInInspector] public float temp_amount = 0f;
    [HideInInspector] public float temp_mana = 0f;
    [HideInInspector] public float temp_coin = 0f;
    [HideInInspector] public List<NumberType> myNumTypes = new List<NumberType>();

    [Header("Innter Variables")]
    protected int deathrattle_times = 1; // How many times this card can trigger deathrattle effects

    /// <summary>
    /// Generic number update for this card. Supports add or multiply. Override in subclasses for custom logic.
    /// </summary>
    /// <param name="numberType">Which number to update</param>
    /// <param name="value">Value to add or multiply</param>
    /// <param name="source">Source card (for buff tracking)</param>
    /// <param name="isMult">If true, multiply; else add</param>
    /// <returns>True if updated</returns>
    public virtual bool UpdateNumberValue(NumberType numberType, float value, CardMaster source = null, bool isPermanent = false, bool isMult = false)
    {
        if (!numberTypesCanBeModified.Contains(numberType)) return false;

        if (isMult)
        {
            switch (numberType)
            {
                case NumberType.Damage:
                    damage *= value;
                    if (isPermanent) default_damage *= value;
                    ShowPopupOnUpdateValue(source, value, "Damage", "x", isPermanent ? "Permanent" : "");
                    return true;
                case NumberType.Health:
                    health *= value;
                    if (isPermanent) default_health *= value;
                    ShowPopupOnUpdateValue(source, value, "Health", "x", isPermanent ? "Permanent" : "");
                    return true;
                case NumberType.Probability:
                    probability *= value;
                    if (isPermanent) default_probability *= value;
                    ShowPopupOnUpdateValue(source, value, "Probability", "x", isPermanent ? "Permanent" : "");
                    return true;
                case NumberType.Amount:
                    amount *= value;
                    if (isPermanent) default_amount *= value;
                    ShowPopupOnUpdateValue(source, value, "Amount", "x", isPermanent ? "Permanent" : "");
                    return true;
                case NumberType.Mana:
                    mana *= value;
                    if (isPermanent) default_mana *= value;
                    ShowPopupOnUpdateValue(source, value, "Mana", "x", isPermanent ? "Permanent" : "");
                    return true;
                case NumberType.Coin:
                    coin *= value;
                    if (isPermanent) default_coin *= value;
                    ShowPopupOnUpdateValue(source, value, "Coin", "x", isPermanent ? "Permanent" : "");
                    return true;
                default: return false;
            }
        }
        else
        {
            switch (numberType)
            {
                case NumberType.Damage:
                    damage += value;
                    if (isPermanent) default_damage += value;
                    ShowPopupOnUpdateValue(source, value, "Damage", "+", isPermanent ? "Permanent" : "");
                    return true;
                case NumberType.Health:
                    health += value;
                    if (isPermanent) default_health += value;
                    ShowPopupOnUpdateValue(source, value, "Health", "+", isPermanent ? "Permanent" : "");
                    return true;
                case NumberType.Probability:
                    probability += value;
                    if (isPermanent) default_probability += value;
                    ShowPopupOnUpdateValue(source, value, "Probability", "+", isPermanent ? "Permanent" : "");
                    return true;
                case NumberType.Amount:
                    amount += value;
                    if (isPermanent) default_amount += value;
                    ShowPopupOnUpdateValue(source, value, "Amount", "+", isPermanent ? "Permanent" : "");
                    return true;
                case NumberType.Mana:
                    mana += value;
                    if (isPermanent) default_mana += value;
                    ShowPopupOnUpdateValue(source, value, "Mana", "+", isPermanent ? "Permanent" : "");
                    return true;
                case NumberType.Coin:
                    coin += value;
                    if (isPermanent) default_coin += value;
                    ShowPopupOnUpdateValue(source, value, "Coin", "+", isPermanent ? "Permanent" : "");
                    return true;
                default: return false;
            }
        }
    }

    public void ShowPopupOnUpdateValue(CardMaster source, float value, string type, string sign, string isPermanent)
    {
        // Only show popup if this card or source card is lastDraggedCard
        var lastDragged = BoardArea.instance != null ? BoardArea.instance.lastDraggedCard : null;
        if (lastDragged == this || (source != null && lastDragged == source) || source == this)
        {
            var cardCommon = GetComponent<CardCommon>();
            if (cardCommon != null) cardCommon.ShowPopup($"{type}: {sign}{value} {isPermanent}");
        }
    }

    public void ShowPopup(string message)
    {
        var cardCommon = GetComponent<CardCommon>();
        if (cardCommon != null) cardCommon.ShowPopup(message);
    }

    /// <summary>
    /// Update this card's own number value (permanent if isPermanent). Supports add or multiply.
    /// </summary>
    public virtual bool UpdateSelfNumberValue(NumberType numberType, float value, bool isPermanent = false, bool isMult = false)
    {
        // For base, just call UpdateNumberValue
        return UpdateNumberValue(numberType, value, this, isPermanent, isMult);
    }

    [HideInInspector] public Vector2Int gridLocation = new Vector2Int(-1, -1); // Location on the board grid, used for linking and positioning
    public CardMaster instance;

    protected virtual void Awake()
    {
        instance = this;
        // Store initial values as defaults
        default_damage = damage;
        default_health = health;
        default_probability = probability;
        default_amount = amount;
        default_mana = mana;
        default_coin = coin;

        if (damage != 0) myNumTypes.Add(NumberType.Damage);
        if (health != 0) myNumTypes.Add(NumberType.Health);
        if (probability != 0) myNumTypes.Add(NumberType.Probability);
        if (amount != 0) myNumTypes.Add(NumberType.Amount);
        if (mana != 0) myNumTypes.Add(NumberType.Mana);
        if (coin != 0) myNumTypes.Add(NumberType.Coin);

        OnLateUpdateCardValues += UpdateCardConditions;

        if (useRandomLinks)
        {
            // List of all directions
            var directions = new List<string> { "up", "left", "right", "down" };
            // Shuffle directions
            for (int i = 0; i < directions.Count; i++)
            {
                int j = UnityEngine.Random.Range(i, directions.Count);
                var temp = directions[i];
                directions[i] = directions[j];
                directions[j] = temp;
            }
            // Enable only linkCount random directions, disable others
            up_link_enabled = false;
            left_link_enabled = false;
            right_link_enabled = false;
            down_link_enabled = false;
            for (int i = 0; i < linkCount && i < directions.Count; i++)
            {
                switch (directions[i])
                {
                    case "up": up_link_enabled = true; break;
                    case "left": left_link_enabled = true; break;
                    case "right": right_link_enabled = true; break;
                    case "down": down_link_enabled = true; break;
                }
            }
        }
    }

    void Start()
    {
        ResetUIStars();
    }

    public virtual void OnCardEnable()
    {
        // If this card is a value card, update linked cards' numbers using UpdateNumberValue
        if (card_type == CardType.Value)
        {
            CardMaster[] linked = new CardMaster[] { up_link_cardmaster, left_link_cardmaster, right_link_cardmaster, down_link_cardmaster };
            // For each number type, if the value is not zero, apply to linked cards
            var valuePairs = new (NumberType, float)[] {
                (NumberType.Damage, damage),
                (NumberType.Health, health),
                (NumberType.Probability, probability),
                (NumberType.Amount, amount),
                (NumberType.Mana, mana),
                (NumberType.Coin, coin)
            };
            foreach (var (nType, nValue) in valuePairs)
            {
                if (Mathf.Abs(nValue) > 0.0001f)
                {
                    foreach (var link in linked)
                    {
                        if (link != null)
                        {
                            if (link.card_type == CardType.Gun)
                            {
                                // If the link is a gun card, we should apply the buff at the very end
                                CardMaster.OnApplyValuesToGuns += () => link.UpdateNumberValue(nType, nValue, this);
                            }
                            else
                            {
                                // If the link is a value card, we can add attack to it
                                link.UpdateNumberValue(nType, nValue, this);
                            }
                        }
                    }
                }
            }
        }
        // Invoke the event for this card
        OnThisCardEnable?.Invoke();
    }

    public virtual void OnCardDisable()
    {

    }

    public virtual void Reset()
    {
        // Reset all values to their defaults
        damage = default_damage;
        health = default_health;
        probability = default_probability;
        amount = default_amount;
        mana = default_mana;
        coin = default_coin;
        ClearUpdateSources();
    }

    // (Removed duplicate UpdateNumberValue and UpdateSelfNumberValue)

    public BuffEntry AddBuffEntry(string buffName, string buffDescription, int order = 0)
    {
        var cardCommon = GetComponent<CardCommon>();
        if (cardCommon != null) return cardCommon.AddBuffDescription(buffName, buffDescription, order);
        return null;
    }

    // you should override this method to return the card's name and description
    // if you want to use the default implementation, just return card_name
    public virtual string GetName()
    {
        return card_name;
    }

    public virtual string GetDescription()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        var numberTypes = myNumTypes;
        var numberValues = new List<float>
        {
            damage,
            health,
            probability,
            amount,
            mana,
            coin
        };
        for (int i = 0; i < numberTypes.Count; i++)
        {
            float val = 0f;
            switch (numberTypes[i])
            {
                case NumberType.Damage: val = damage; break;
                case NumberType.Health: val = health; break;
                case NumberType.Probability: val = probability; break;
                case NumberType.Amount: val = amount; break;
                case NumberType.Mana: val = mana; break;
                case NumberType.Coin: val = coin; break;
            }
            sb.AppendFormat("{0}: {1}", numberTypes[i], val);
            if (i < numberTypes.Count - 1) sb.Append("\n");
        }
        return GameSettings.AddIcon(sb.ToString());
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

    public bool TryPurchaseCard()
    {
        if (CoinCounter.instance == null)
        {
            Debug.LogError("CoinCounter.instance not found in scene.");
            return false;
        }
        if (!CoinCounter.CanCostCoin(-card_cost))
        {
            Debug.LogError($"Not enough coins to purchase {card_name} (cost: {card_cost})");
            return false;
        }
        // Deduct coins
        GameEvents.instance.UpdateCoins(-card_cost);
        // Add to hand
        if (HandArea.instance != null)
        {
            CardManager.instance.QueueAddCardObjects(new List<GameObject> { this.gameObject });
        }
        else
        {
            Debug.LogError("HandArea.instance is null");
        }
        return true;
    }

    public virtual void OnCardPurchased()
    {
        // do something when the card is purchased
        OnThisCardPurchased?.Invoke();
    }

    public virtual void OnCardSold()
    {
        // do something when the card is sold
        OnThisCardSold?.Invoke();

        OnCardDestroyed();
        GameEvents.instance.UpdateCoins(card_sell_price);
    }

    public virtual void OnCardLevelCleared()
    {
        // do something when the card is level cleared
        OnThisCardLevelCleared?.Invoke();
        UpdateConditionsWhenLevelCleared();
    }

    public void UpdateConditionsWhenLevelCleared()
    {
        // Fragile: chance to destroy
        if (card_conditions != null && card_conditions.Contains(CardCondition.IsFragile))
        {
            float chance = 0.25f;
            if (GameSettings.instance != null)
            {
                chance = Mathf.Clamp01(GameSettings.instance.fragileDestroyChance);
            }
            if (UnityEngine.Random.value < chance)
            {
                OnCardDestroyed();
                return;
            }
        }
        // Temporary: always destroy
        if (card_conditions != null && card_conditions.Contains(CardCondition.IsTemporary))
        {
            OnCardDestroyed();
            return;
        }
        // Volatile: change to another card of same rarity
        if (card_conditions != null && card_conditions.Contains(CardCondition.IsVolatile))
        {
            var db = Resources.Load<CardDatabase>("CardDatabase");
            if (db != null)
            {
                var candidates = CardDatabase.FindCards(card => card.card_rarity == this.card_rarity && card.card_id != this.card_id);
                if (candidates != null && candidates.Count > 0)
                {
                    var prefab = candidates[UnityEngine.Random.Range(0, candidates.Count)];
                    var newCard = Instantiate(prefab, this.transform.parent);
                    newCard.transform.SetSiblingIndex(this.transform.GetSiblingIndex());
                    Destroy(this.gameObject);
                    return;
                }
            }
        }

        // Growing: update only one random value
        if (GameSettings.instance == null)
        {
            Debug.LogError("GameSettings.instance is null, cannot apply growth.");
            return;
        }
        if (card_conditions != null && card_conditions.Contains(CardCondition.IsGrowing))
        {
            if (numberTypesCanBeModified.Count > 0)
            {
                var chosenType = numberTypesCanBeModified[UnityEngine.Random.Range(0, numberTypesCanBeModified.Count)];
                UpdateSelfNumberValue(chosenType, GameSettings.Growth(chosenType), isPermanent: true, isMult: false);
            }
        }
        // Decaying: update only one random value
        if (card_conditions != null && card_conditions.Contains(CardCondition.IsDecaying))
        {
            if (numberTypesCanBeModified.Count > 0)
            {
                var chosenType = numberTypesCanBeModified[UnityEngine.Random.Range(0, numberTypesCanBeModified.Count)];
                UpdateSelfNumberValue(chosenType, GameSettings.Decay(chosenType), isPermanent: true, isMult: false);
            }
        }
    }

    public virtual bool Grow(int times = 1)
    {
        if (times < 1) return false;
        if (numberTypesCanBeModified.Count > 0)
        {
            for (int i = 0; i < times; i++)
            {
                var chosenType = numberTypesCanBeModified[UnityEngine.Random.Range(0, numberTypesCanBeModified.Count)];
                UpdateSelfNumberValue(chosenType, GameSettings.Growth(chosenType), isPermanent: true, isMult: false);
            }
            ShowPopup("Grow: " + times + " times");
            return true;
        }
        else
        {
            return false; // No types to grow
        }
    }

    public virtual bool Decay(int times = 1)
    {
        if (times < 1) return false;
        if (numberTypesCanBeModified.Count > 0)
        {
            for (int i = 0; i < times; i++)
            {
                var chosenType = numberTypesCanBeModified[UnityEngine.Random.Range(0, numberTypesCanBeModified.Count)];
                UpdateSelfNumberValue(chosenType, GameSettings.Decay(chosenType), isPermanent: true, isMult: false);    
            }
            ShowPopup("Decay: " + times + " times");
            return true;
        }
        else
        {
            return false; // No types to decay
        }
    }

    public virtual UIStar.StarType GetStarType(CardMaster cardMaster = null)
    {
        if (cardMaster == null)
            return UIStar.StarType.White;
        if (cardMaster.numberTypesCanBeModified != null && cardMaster.numberTypesCanBeModified.Count > 0)
        {
            // Highlight if any of the types are present
            foreach (var nType in myNumTypes)
            {
                if (cardMaster.numberTypesCanBeModified.Contains(nType))
                    return UIStar.StarType.Yellow;
            }
        }
        return UIStar.StarType.White;
    }

    public void UpdateUIStars(UnityEngine.Vector2Int thisCardPosition = default)
    {
        if (uiStarPositions == null || uiStarPositions.Count == 0) return;

        // Create new stars at specified positions
        foreach (var pos in uiStarPositions)
        {
            var cardCommon = GetComponent<CardCommon>();
            if (cardCommon != null)
            {
                int row = (thisCardPosition.x + pos.x);
                int col = (thisCardPosition.y + pos.y);
                cardCommon.SetUIStar(pos, GetStarType(BoardArea.instance.GetCell(row, col)));
            }
        }
    }

    public void ResetUIStars()
    {
        if (uiStarPositions == null || uiStarPositions.Count == 0) return;

        // Create new stars at specified positions
        foreach (var pos in uiStarPositions)
        {
            var cardCommon = GetComponent<CardCommon>();
            if (cardCommon != null)
            {
                cardCommon.SetUIStar(pos, UIStar.StarType.White); // set white for reset
            }
        }
    }

    public void DissolveAllImagesAndTMPs(GameObject root, float duration)
    {
        var images = root.GetComponentsInChildren<UnityEngine.UI.Image>(true);
        foreach (var img in images)
        {
            if (img.material != null && img.material.HasProperty("_DissolveAmount"))
            {
                DOTween.To(
                    () => img.material.GetFloat("_DissolveAmount"),
                    x => img.material.SetFloat("_DissolveAmount", x),
                    1f, duration
                ).SetEase(Ease.InQuad);
            }
        }
        // Also dissolve all TMP_Text components if using a compatible dissolve material
        var tmps = root.GetComponentsInChildren<TMPro.TMP_Text>(true);
        foreach (var tmp in tmps)
        {
            if (tmp.fontMaterial != null && tmp.fontMaterial.HasProperty("_DissolveAmount"))
            {
                DOTween.To(
                    () => tmp.fontMaterial.GetFloat("_DissolveAmount"),
                    x => tmp.fontMaterial.SetFloat("_DissolveAmount", x),
                    1f, duration
                ).SetEase(Ease.InQuad);
            }
        }
    }

    public virtual bool OnCardDestroyed()
    {
        if (card_conditions != null && card_conditions.Contains(CardCondition.IsEternal))
        {
            GameEvents.instance.ShowMessage($"Eternal card {card_name} cannot be destroyed.", GameEvents.MessageType.FullWarning);
            return false;
        }

        if (CombatManager.isInBattle)
        {
            StartCoroutine(WaitForBattleEndAndBoardOpenThenDestroy());
        }
        else
        {
            DoDestroyCard();
        }
        return true;
    }

    private IEnumerator WaitForBattleEndAndBoardOpenThenDestroy()
    {
        bool battleEnded = false;
        bool boardOpened = false;
        System.Action onLevelCleared = () => { battleEnded = true; };
        System.Action<bool> onToggleBoard = (isActive) => { if (isActive) boardOpened = true; };
        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnLevelCleared += onLevelCleared;
            GameEvents.instance.OnToggleBoard += onToggleBoard;
        }
        // Wait until both battleEnded and boardOpened are true
        while (!(battleEnded && boardOpened))
        {
            yield return new WaitForSeconds(0.5f);
        }
        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnLevelCleared -= onLevelCleared;
            GameEvents.instance.OnToggleBoard -= onToggleBoard;
        }
        yield return new WaitForSeconds(1f);
        DoDestroyCard();
    }

    private void DoDestroyCard()
    {
        // Actually call the event to announce card destruction
        if (GameEvents.instance != null)
        {
            GameEvents.instance.CardDiscarded(this);
        }
        // --- DOTween Effect: Dissolve before moving card away ---
        var canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        float shakeDuration = 0.5f;
        float waitDuration = 0.5f;
        float scaleFadeDuration = destroyEffectDuration;

        Sequence destroySeq = DOTween.Sequence();
        // 1. Rotate shake
        destroySeq.Append(transform.DOShakeRotation(shakeDuration, strength: new UnityEngine.Vector3(0, 0, 20), vibrato: 50, randomness: 90, fadeOut: true));
        // 2. Wait
        destroySeq.AppendInterval(waitDuration);
        // 3. Scale up and fade out
        destroySeq.Append(transform.DOScale(1.6f, scaleFadeDuration).SetEase(Ease.InQuad));
        destroySeq.Join(canvasGroup.DOFade(0f, scaleFadeDuration).SetEase(Ease.InQuad));
        // 4. Move far away after sequence
        destroySeq.OnComplete(() =>
        {
            this.transform.position = new UnityEngine.Vector3(10000, 10000, 10000);
            // Move this card to the hand area before destroying
            if (HandArea.instance != null)
            {
                this.transform.SetParent(HandArea.instance.DiscardedCardsParent, false);
                HandArea.instance.AddDiscardedCard(this);
            }
        });

        // do something when the card is destroyed
        OnThisCardDestroyed?.Invoke();

        if (gridLocation != null && gridLocation.x >= 0 && gridLocation.y >= 0)
        {
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
        }
        else if (gridLocation.x < 0 || gridLocation.y < 0)
        {
            HandArea.instance?.DiscardCard(this);
        }
        // Reset card and update
        Reset();
        CardDragHandler.TriggerUpdateCards();
    }

    public virtual string GetBuffEntryName()
    {
        // should override this method to return the buff entry name
        return string.Empty;
    }

    public virtual string GetBuffEntryText()
    {
        // should override this method to return the buff entry text
        return string.Empty;
    }

    public virtual void UpdateBuffEntry()
    {
        if (buffEntry != null)
        {
            buffEntry.SetBuffName(GetBuffEntryName());
            buffEntry.SetBuffDescription(GetBuffEntryText());
        }
    }

    // --- Events Callers ---
    // Static methods to safely invoke events from outside this class
    public static void InvokeUpdateCardValues()
    {
        OnUpdateCardValues?.Invoke();
    }
    public static void InvokeLateUpdateCardValues()
    {
        OnLateUpdateCardValues?.Invoke();
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

    public void UpdateCardConditions()
    {
        // Powerful: double values
        if (card_conditions != null && card_conditions.Contains(CardCondition.IsPowerful))
        {
            damage *= 2f;
            health *= 2f;
            probability *= 2f;
            amount *= 2f;
            mana *= 2f;
            coin *= 2f;
        }
        // Frail: halve values
        if (card_conditions != null && card_conditions.Contains(CardCondition.IsFrail))
        {
            damage *= 0.5f;
            health *= 0.5f;
            probability *= 0.5f;
            amount *= 0.5f;
            mana *= 0.5f;
            coin *= 0.5f;
        }
    }

    // --- Card Condition Management ---
    // Adds a condition to the card if it doesn't already exist
    // Returns true if the condition was added, false if it already existed
    public bool AddCondition(CardCondition condition)
    {
        // Check if the condition can be added based on game settings
        if (!CanAddCondition(condition)) return false;

        if (card_conditions == null)
            card_conditions = new List<CardCondition>();
        if (!card_conditions.Contains(condition))
        {
            card_conditions.Add(condition);
            OnUpdateCardTexts?.Invoke();
            return true;
        }
        return false;
    }

    // Removes a condition from the card if it exists
    // Returns true if the condition was removed, false if it didn't exist
    public bool RemoveCondition(CardCondition condition)
    {
        if (card_conditions == null)
            return false;
        if (card_conditions.Contains(condition))
        {
            card_conditions.Remove(condition);
            OnUpdateCardTexts?.Invoke();
            return true;
        }
        return false;
    }

    protected bool CanAddCondition(CardCondition condition)
    {
        // Check if the condition can be added based on game settings or other logic
        if (GameSettings.instance != null)
        {
            // Example: check if the condition is allowed in current game mode
            return GameSettings.IsConditionAllowed(card_type, condition);
        }
        if (GameEvents.instance != null)
            GameEvents.instance.ShowMessage("Cannot add condition: GameSettings not found.", GameEvents.MessageType.FullWarning);
        return false; // Default to false if no specific logic
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

    public static void SetLinkColor(GameObject linkGO, Color color)
    {
        if (linkGO == null) return;
        bool isTransparent = color.a <= 0.01f;
        if (isTransparent)
        {
            linkGO.SetActive(false);
        }
        else
        {
            if (!linkGO.activeSelf)
                linkGO.SetActive(true);
            var renderer = linkGO.GetComponent<SpriteRenderer>();
            if (renderer != null)
                renderer.color = color;
            // If using Image for UI, also support:
            var img = linkGO.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
                img.color = color;
        }
    }

    // Make SetLinkAlpha public for use by CardDragHandler
    public void SetLinkAlpha(GameObject go, float alpha)
    {
        if (go == null) return;
        var img = go.GetComponent<UnityEngine.UI.Image>();
        var c = img.color;
        c.a = alpha;
        SetLinkColor(go, c);
    }

    private void SetLinkColor(GameObject go, Color color, float alpha)
    {
        if (go == null) return;
        color.a = alpha;
        SetLinkColor(go, color);

    }

    // Set a specific link (by direction) to black 50% transparent
    public void SetLinkHalfTransparentBlack(string dir)
    {
        var go = GetLinkGameObject(dir);
        if (go == null) return;

        SetLinkColor(go, (GameSettings.instance != null && GameSettings.instance.colorLinkInactive != default(Color))
                ? GameSettings.instance.colorLinkInactive
                : new Color(0f, 0f, 0f, 0.5f));
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
        return foundGun;
    }

    public Gun GetLinkedGun(CardDir dir)
    {
        switch (dir)
        {
            case CardDir.Up: return up_link_cardmaster?.current_gun;
            case CardDir.Left: return left_link_cardmaster?.current_gun;
            case CardDir.Right: return right_link_cardmaster?.current_gun;
            case CardDir.Down: return down_link_cardmaster?.current_gun;
            default: return null;
        }
    }

    // Custom LinkType comparison: Common matches any, others only match themselves
    public static bool LinkTypesEqual(LinkType a, LinkType b)
    {
        if (a == LinkType.Common || b == LinkType.Common)
            return true;
        return a == b;
    }

    /// <summary>
    /// Get all star cards at the star positions defined in uiStarPositions.
    /// Also returns empty slots and locked slots. Empty and Locked slots are returned as the actual grid positions, not relative to this card's position.
    /// </summary>
    /// <param name="starCards">The list of cardMasters of the star positions</param>
    /// <param name="emptySlots">The list of empty grid positions</param>
    /// <param name="lockedSlots">The list of locked grid positions</param>
    public void GetStarCards(out List<CardMaster> starCards, out List<UnityEngine.Vector2Int> emptySlots, out List<UnityEngine.Vector2Int> lockedSlots)
    {
        // Initialize the list of star cards
        starCards = new List<CardMaster>();
        emptySlots = new List<UnityEngine.Vector2Int>();
        lockedSlots = new List<UnityEngine.Vector2Int>();
        if (uiStarPositions == null || uiStarPositions.Count == 0) return;

        // Get all cards at the star positions
        foreach (var pos in uiStarPositions)
        {
            int targetRow = gridLocation.x + pos.x;
            int targetCol = gridLocation.y + pos.y;
            var card = BoardArea.instance.GetCell(targetRow, targetCol);
            if (card != null)
            {
                starCards.Add(card);
            }
            else
            {
                bool isCellOpen = BoardArea.instance.IsCellOpen(targetRow, targetCol);
                if (isCellOpen)
                {
                    // If the cell is open, add it to empty slots
                    emptySlots.Add(new UnityEngine.Vector2Int(targetRow, targetCol));
                }
                else
                {
                    // If the cell is locked, add it to locked slots
                    lockedSlots.Add(new UnityEngine.Vector2Int(targetRow, targetCol));
                }
            }
        }
        return;
    }

    /// <summary>
    /// Spawns a number of objects around a specified position with a given rotation.
    /// </summary>
    /// <param name="_prefab">object to spawn</param>
    /// <param name="_count">how many to spawn</param>
    /// <param name="_position">where to spawn the objects</param>
    /// <param name="_rotation">rotation of the spawned objects</param>
    /// <param name="_radius">radius within which to spawn the objects</param>
    /// <param name="_modifyObject">action to modify the spawned object</param>
    /// <returns></returns>
    public List<GameObject> SpawnObjects(GameObject _prefab, int _count = 1, UnityEngine.Vector2? _position = null, UnityEngine.Quaternion _rotation = default, float _radius = 1f, System.Action<GameObject> _modifyObject = null)
    {
        if (_prefab == null || _count <= 0) return new List<GameObject>();
        UnityEngine.Vector2 spawnPosition = _position ?? (PlayerController.instance != null ? PlayerController.instance.GetPosition() : UnityEngine.Vector2.zero);
        List<GameObject> spawnedObjects = new List<GameObject>();

        for (int i = 0; i < _count; i++)
        {
            spawnPosition = CombatManager.instance.TryGetSpawnLocation(spawnPosition, _radius) ?? spawnPosition;
            var obj = ObjectPool.Instance.GetObject(_prefab, spawnPosition, _rotation);
            _modifyObject?.Invoke(obj); // Apply any modifications if needed

            // if (CombatManager.instance != null) CombatManager.instance.AddObject(obj.transform);
            // if (GameEvents.instance != null)
            // {
            //     GameEvents.instance.SpawnObject(obj.transform);
            // }
            spawnedObjects.Add(obj);
        }

        return spawnedObjects;
    }

    /// <summary>
    /// Spawns a number of pawns around a specified position with a given rotation.
    /// </summary>
    /// <param name="_prefab">object to spawn</param>
    /// <param name="_count">how many to spawn</param>
    /// <param name="_position">where to spawn the objects</param>
    /// <param name="_rotation">rotation of the spawned objects</param>
    /// <param name="_radius">radius within which to spawn the objects</param>
    /// <param name="_modifyObject">action to modify the spawned object</param>
    /// <returns></returns>
    public List<GameObject> SpawnPawns(GameObject _prefab, int _count = 1, UnityEngine.Vector2? _position = null, UnityEngine.Quaternion _rotation = default, float _radius = 1f, System.Action<GameObject> _modifyObject = null)
    {
        if (_prefab == null || _count <= 0) return new List<GameObject>();
        UnityEngine.Vector2 spawnPosition = _position ?? (PlayerController.instance != null ? PlayerController.instance.GetPosition() : UnityEngine.Vector2.zero);
        List<GameObject> spawnedObjects = new List<GameObject>();

        for (int i = 0; i < _count; i++)
        {
            spawnPosition = CombatManager.instance.TryGetSpawnLocation(spawnPosition, _radius) ?? spawnPosition;
            var obj = ObjectPool.Instance.GetObject(_prefab, spawnPosition, _rotation);
            _modifyObject?.Invoke(obj); // Apply any modifications if needed

            spawnedObjects.Add(obj);
        }

        return spawnedObjects;
    }

    /// <summary>
    /// Spawns a number of objects around a specified position with a given rotation.
    /// </summary>
    /// <param name="_prefab">object to spawn (must with a GunBullet script attached)</param>
    /// <param name="_count">how many to spawn</param>
    /// <param name="_position">where to spawn the objects</param>
    /// <param name="_rotation">rotation of the spawned objects</param>
    /// <param name="_radius">radius within which to spawn the objects</param>
    /// <param name="_triggerTags">tags to trigger on</param>
    /// <param name="_randomAngleOffset">random angle offset for inaccuracy</param>
    /// <param name="_modifyBullet">action to modify the bullet</param>
    /// <returns></returns>
    public List<GameObject> SpawnBullets(GameObject _prefab, int _count = 1, UnityEngine.Vector2 _position = default, UnityEngine.Quaternion _rotation = default, float _radius = 0.3f,
        List<string> _triggerTags = null, float _bulletDamage = 5f, float _randomAngleOffset = 10f, System.Action<GunBullet> _modifyBullet = null)
    {
        if (_prefab == null || _count <= 0) return new List<GameObject>();
        if (_triggerTags == null) _triggerTags = new List<string> { "Enemy" }; // Default to hitting enemies
        UnityEngine.Vector2 spawnPosition = _position;
        List<GameObject> spawnedBullets = new List<GameObject>();

        for (int i = 0; i < _count; i++)
        {
            spawnPosition = CombatManager.instance.TryGetSpawnLocation(spawnPosition, _radius) ?? spawnPosition;
            var bulletObj = ObjectPool.Instance.GetObject(_prefab, spawnPosition, _rotation);
            GunBullet bullet = bulletObj.GetComponent<GunBullet>();

            if (bullet != null)
            {
                bullet.SetOwner(gameObject); // Set owner to this card
                bullet.SetSpeed(UnityEngine.Vector2.zero); // Start with zero speed
                bullet.trigger_tags = _triggerTags; // Ensure it can hit enemies
                // Set direction towards nearest enemy
                UnityEngine.Vector2 dir = CombatManager.instance.GetVectorToNearestEnemy(spawnPosition);
                if (dir == UnityEngine.Vector2.zero)
                    dir = UnityEngine.Random.insideUnitCircle.normalized; // fallback
                // Add random angle offset for inaccuracy
                float angleOffset = UnityEngine.Random.Range(-_randomAngleOffset, _randomAngleOffset);
                dir = UnityEngine.Quaternion.Euler(0, 0, angleOffset) * dir;
                bullet.SetSpeed(dir, bullet.speed);
                bullet.att = _bulletDamage; // Set the bullet damage

                // Apply any additional modifications to the bullet
                _modifyBullet?.Invoke(bullet);
            }
            else
            {
                Debug.LogError($"Spawned object {bulletObj.name} does not have a GunBullet component. Please ensure the prefab is set up correctly.");
            }

            spawnedBullets.Add(bulletObj);
        }

        return spawnedBullets;
    }

    // Helper to create a temporary transform at a position
    public Transform CreateTempTransformAt(UnityEngine.Vector2 pos)
    {
        GameObject temp = new GameObject("TempWallSpawnPoint");
        temp.transform.position = pos;
        // Destroy after 2 seconds to avoid leaks
        Destroy(temp, 2f);
        return temp.transform;
    }

    // Helper to check if a number is prime
    public static bool IsPrime(int n)
    {
        // Check for numbers less than 2 (0 and 1 are not prime)
        if (n <= 1) return false;
        // Check for 2 and 3 explicitly
        if (n <= 3) return true;
        // Eliminate multiples of 2 and 3
        if (n % 2 == 0 || n % 3 == 0) return false;

        // Only check odd divisors up to √n
        for (int i = 5; i * i <= n; i += 6)
        {
            if (n % i == 0 || n % (i + 2) == 0) return false;
        }
        return true;
    }
    
    public List<CardMaster> GetLinkedCards()
    {
        var linked = new List<CardMaster>();
        if (up_link_cardmaster != null) linked.Add(up_link_cardmaster);
        if (down_link_cardmaster != null) linked.Add(down_link_cardmaster);
        if (left_link_cardmaster != null) linked.Add(left_link_cardmaster);
        if (right_link_cardmaster != null) linked.Add(right_link_cardmaster);
        return linked;
    }
}