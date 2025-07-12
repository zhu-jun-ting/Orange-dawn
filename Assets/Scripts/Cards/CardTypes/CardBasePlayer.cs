using System;
using UnityEngine;

public class CardBasePlayer : CardMaster
{
    private PlayerController player;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        player = PlayerController.instance;
    }

    public override void OnCardEnable()
    {
        if (player == null) player = PlayerController.instance;
        base.OnCardEnable();
    }

    public override void OnCardDisable()
    {
        base.OnCardDisable();
    }

    public override void OnCardDestroyed()
    {
        base.OnCardDestroyed();
    }

    public override string GetDescription()
    {
        if (player == null) player = PlayerController.instance;
        if (player != null)
        {
            return GameSettings.AddIcon(string.Format(card_description,
                player.max_health,
                ManaBar.manaMax,
                player.dodge*100 // Convert to percentage
            ));
        }
        return "";
    }

    public override bool UpdateNumberValue(NumberType numberType, float value, CardMaster source = null, bool isPermanent = false, bool isMult = false)
    {
        if (IsBuffedFromSource(source, addToList: true, includeSelf: true)) return false;

        if (player == null) player = PlayerController.instance;
        if (player == null) return false;

        if (isMult)
        {
            switch (numberType)
            {
                case NumberType.Health:
                    player.max_health *= value;
                    if (isPermanent) player.max_health = Mathf.Max(player.max_health, player.initial_max_health);
                    player.UpdateMaxHealth();
                    return true;
                case NumberType.Probability:
                    player.dodge *= value;
                    if (isPermanent) player.initial_dodge = player.dodge;
                    return true;
                case NumberType.Mana:
                    ManaBar.manaMax = (int)(ManaBar.manaMax * value);
                    if (isPermanent) ManaBar.initialManaMax = ManaBar.manaMax;
                    return true;
                default:
                    return false;
            }
        }
        else
        {
            switch (numberType)
            {
                case NumberType.Health:
                    player.max_health += value;
                    if (isPermanent) player.max_health = Mathf.Max(player.max_health, player.initial_max_health);
                    player.UpdateMaxHealth();
                    return true;
                case NumberType.Probability:
                    player.dodge += value/100;  // Convert percentage to decimal
                    if (isPermanent) player.initial_dodge = player.dodge;
                    return true;
                case NumberType.Mana:
                    ManaBar.manaMax = (int)(ManaBar.manaMax + value);
                    if (isPermanent) ManaBar.initialManaMax = ManaBar.manaMax;
                    return true;
                default:
                    return false;
            }
        }
        
    }

    public override void Reset()
    {
        player.Reset();
    }
}
