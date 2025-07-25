using UnityEngine;

/// <summary>
/// DialougeBonfire: Handles bonfire-specific event actions for NPC dialogue events.
/// </summary>
public class DialougeBonfire : EventNPCDialogue
{
    // Recover 50% health
    public void RecoverHalfHealth()
    {
        var player = PlayerController.instance;
        if (player != null)
        {
            float recoverAmount = HealthBar.HealthMax * 0.5f;
            GameEvents.instance.HealPawn(recoverAmount, player.gameObject.GetComponent<PawnMaster>(), player.gameObject, player.transform);
        }
    }

    // Recover 20% health and gain 50% experience
    public void RecoverSmallHealthAndGainHalfExp()
    {
        var player = PlayerController.instance;
        if (player != null)
        {
            float recoverAmount = HealthBar.HealthMax * 0.2f;
            GameEvents.instance.HealPawn(recoverAmount, player.gameObject.GetComponent<PawnMaster>(), player.gameObject, player.transform);
            // Gain 50% experience using ExpBar static values
            float gainExp = ExpBar.ExpMax * 0.5f;
            ExpBar.GainExp(gainExp);
            GameEvents.instance.ShowStringUI($"+{(int)gainExp} EXP", player, GameEvents.DamageType.Normal, player.transform.position);
        }
    }

    // Recover 20% health and gain 20 coin
    public void RecoverSmallHealthAndGainCoin()
    {
        var player = PlayerController.instance;
        if (player != null)
        {
            float recoverAmount = HealthBar.HealthMax * 0.2f;
            GameEvents.instance.HealPawn(recoverAmount, player.gameObject.GetComponent<PawnMaster>(), player.gameObject, player.transform);
            GameEvents.instance.UpdateCoins(20);
            GameEvents.instance.ShowStringUI($"+20 Coin", player, GameEvents.DamageType.Normal, player.transform.position);
        }
    }

    // Get a random value card
    public void GetRandomValueCard()
    {
        var cardPrefab = CardDatabase.GetRandomCard(card => card.card_type == CardMaster.CardType.Value, false);
        if (cardPrefab != null)
        {
            CardManager.instance.QueueAddCardObjects(new System.Collections.Generic.List<GameObject> { GameObject.Instantiate(cardPrefab) });
        }
    }
}
