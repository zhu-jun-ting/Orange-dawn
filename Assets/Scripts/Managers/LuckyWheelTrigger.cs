
using UnityEngine;
using System.Collections.Generic;

public class LuckyWheelTrigger : MonoBehaviour
{
    
    public enum RewardType
    {
        None,
        FifteenCoins,
        RandomCard,
        RandomEffectOnCard,
        HealHalfHealth,
        DestoryRandomCard,
        RandomValueCard, 
        GiveHalfExperience,
    }
    
    [Header("Reward/Effect Settings")]
    public RewardType rewardType;

    
    public bool IsPositionInside(Vector2 pos)
    {
        // Use collider or bounds to check if position is inside
        var collider = GetComponent<Collider2D>();
        if (collider != null)
            return collider.OverlapPoint(pos);
        // Optionally, use RectTransform or custom logic
        return false;
    }

    private Color originalColor;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            if (spriteRenderer != null)
                spriteRenderer.color = Color.Lerp(originalColor, Color.green, 0.3f);
        } 
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            if (spriteRenderer != null)
                spriteRenderer.color = originalColor;
        }
    }

    public void OnBulletLanded()
    {
        // Give reward or trigger effect
        switch (rewardType)
        {
            case RewardType.FifteenCoins:
                if (GameEvents.instance != null)
                {
                    GameEvents.instance.UpdateCoins(15);
                    GameEvents.instance.ShowMessage("+15 Coins!", GameEvents.MessageType.Banner);
                }
                break;
            case RewardType.RandomCard:
                if (CardManager.instance != null)
                {
                    var randomCard = CardDatabase.GetRandomCard(_ => true, true); // fallback: any card
                    if (randomCard != null)
                    {
                        CardManager.instance.QueueAddCardObjects(new List<GameObject> { randomCard });
                        GameEvents.instance?.ShowMessage("You got a random card!", GameEvents.MessageType.Banner);
                    }
                }
                break;
            case RewardType.RandomEffectOnCard:
                {
                    var allCards = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                    var handCards = new List<GameObject>();
                    foreach (var obj in allCards)
                    {
                        if (obj != null && obj.activeInHierarchy && obj.transform.parent != null && obj.transform.parent.name.ToLower().Contains("hand"))
                        {
                            handCards.Add(obj);
                        }
                    }
                    if (handCards.Count > 0)
                    {
                        var card = handCards[Random.Range(0, handCards.Count)];
                        card.SendMessage("ApplyRandomEffect", SendMessageOptions.DontRequireReceiver);
                        GameEvents.instance?.ShowMessage("A random effect was applied to a card!", GameEvents.MessageType.Banner);
                    }
                }
                break;
            case RewardType.HealHalfHealth:
                var player = PlayerController.instance?.gameObject;
                if (player != null)
                {
                    float heal = HealthBar.HealthMax * 0.5f;
                    GameEvents.instance?.HealPawn(heal, PlayerController.instance, player, null);
                }
                break;
            case RewardType.DestoryRandomCard:
                if (HandArea.instance != null)
                {
                    var handCards = HandArea.instance.GetCardsOnHand();
                    handCards.RemoveAll(card => card.card_conditions.Contains(CardMaster.CardCondition.IsEternal));
                    if (handCards.Count > 0)
                    {
                        var card = handCards[Random.Range(0, handCards.Count)];
                        if (card != null)
                        {
                            card.OnCardDestroyed();
                            PlayerController.ShowPopup(GameSettings.AddIcon(string.Format("Destroyed {0}", card.card_name)));
                        }
                    }
                }
                break;
            case RewardType.RandomValueCard:
                if (CardManager.instance != null)
                {
                    // Fallback: use GetRandomCard with a predicate for value cards if available, else any card
                    var valueCard = CardDatabase.GetRandomCard(card => {
                        var isValue = false;
                        if (card != null)
                        {
                            var field = card.GetType().GetField("isValueCard");
                            if (field != null && field.FieldType == typeof(bool))
                                isValue = (bool)field.GetValue(card);
                        }
                        return isValue;
                    }, true);
                    if (valueCard == null)
                        valueCard = CardDatabase.GetRandomCard(_ => true, true);
                    if (valueCard != null)
                    {
                        CardManager.instance.QueueAddCardObjects(new List<GameObject> { valueCard });
                    }
                }
                break;
            case RewardType.GiveHalfExperience:
                ExpBar.GainExp(ExpBar.ExpMax * 0.5f);
                break;
            default:
                break;
        }
        if (Time.time - GameEvents.instance.lastLevelStartOrClearTime > 1f) GameEvents.instance?.LevelCleared();
    }
}