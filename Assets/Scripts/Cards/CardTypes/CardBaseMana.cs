using System;
using UnityEngine;

// this card gives player 10 Health, and 10% Dodge
// health: for +10 health, use NumberType.Health
// dodge: for +10% dodge, use NumberType.Probability (use probability / 100f)

public class CardBaseMana : CardMaster
{
    private PlayerController player;

    private void Start()
    {
        player = PlayerController.instance;
    }

    public override void OnCardEnable()
    {
        base.OnCardEnable();
        CardMaster.OnApplyValuesToGuns -= HandleOnApplyValuesToGuns;
        CardMaster.OnApplyValuesToGuns += HandleOnApplyValuesToGuns;
    }

    public override string GetDescription()
    {
        if (player == null) player = PlayerController.instance;
        if (player != null)
        {
            return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), (int)mana ));
        }
        return "";
    }


    public void HandleOnApplyValuesToGuns()
    {
        // apply the value changes to player
        ManaBar.manaMax += (int)mana;
    }

    public override void Reset()
    {
        ManaBar.Reset();
        CardMaster.OnApplyValuesToGuns -= HandleOnApplyValuesToGuns;
    }
}
