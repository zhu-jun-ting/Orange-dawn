using System;
using UnityEngine;
using System.Collections.Generic;

// id: custom
// name: Static Electro
// desc: When you destroy an item, Probability: 10% to shoot chain lightning and stun targets. Cost: Mana: 2
public class CardActionStaticElectro : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public event Action<CardMaster, Transform> OnTrigger;

    [Header("Static Electro Settings")]
    public float lightningRetriggerChance = 0.3f; // Chance to retrigger chain
    public int lightningMaxChain = 2; // Max chain count
    public string[] targetTags = new string[] { "Enemy" }; // Tags to target

    private bool isSubscribed = false;

    public override void OnCardEnable()
    {
        if (!isSubscribed && GameEvents.instance != null)
        {
            GameEvents.instance.OnDestroyObject += HandleOnDestroyObject;
            isSubscribed = true;
        }
        OnTrigger -= TriggerAction;
        OnTrigger += TriggerAction;
        base.OnCardEnable();
    }

    public override void OnCardDisable()
    {
        if (isSubscribed && GameEvents.instance != null)
        {
            GameEvents.instance.OnDestroyObject -= HandleOnDestroyObject;
            isSubscribed = false;
        }
        OnTrigger -= TriggerAction;
        base.OnCardDisable();
    }

    public override void Reset()
    {
        base.Reset();
        if (isSubscribed && GameEvents.instance != null)
        {
            GameEvents.instance.OnDestroyObject -= HandleOnDestroyObject;
            isSubscribed = false;
        }
        OnTrigger -= TriggerAction;
    }

    private void HandleOnDestroyObject(Transform destroyedObject, GunBullet bullet)
    {
        if (destroyedObject == null) return;
        if (UnityEngine.Random.value > probability / 100f) return;
        if (!ManaBar.CanCostMana(-(int)mana)) return;
        OnTrigger?.Invoke(this, destroyedObject);
        GameEvents.instance.UpdateMana(-(int)mana);
    }

    public void TriggerAction(CardMaster card, Transform destroyLocation)
    {
        if (destroyLocation == null || CombatManager.instance == null) return;
        
        // Create a list with Shock DOT type to apply stun
        List<EnemyMaster.DotType> dotTypes = new List<EnemyMaster.DotType> { EnemyMaster.DotType.Shock };
        
        // Convert inherited probability (0-100 range) to 0-1 range for dotProbability
        float dotProbability = probability / 100f;
        
        // Shoot lightning chain with stun effect
        CombatManager.instance.ShootLightningChain(
            destroyLocation,
            damage,
            lightningRetriggerChance,
            lightningMaxChain,
            targetTags,
            dotTypes,
            dotProbability
        );
    }


    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), probability, (int)damage, (int)mana));
    }
}
