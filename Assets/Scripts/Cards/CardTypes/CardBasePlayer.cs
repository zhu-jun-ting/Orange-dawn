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
            return string.Format(card_description,
                player.max_health,
                player.moveSpeed,
                player.dodge);
        }
        return "";
    }

    public override bool UpdateNumberValue(CardMaster.NumberType numberType, float value, CardMaster source)
    {
        if (IsBuffedFromSource(source, addToList: true, includeSelf: true)) return false;

        base.UpdateNumberValue(numberType, value, source);

        if (player == null) player = PlayerController.instance;
        if (player == null) return false;

        switch (numberType)
        {
            case NumberType.Health:
                player.max_health += value;
                player.UpdateMaxHealth();
                return true;
            case NumberType.Probability:
                player.dodge += value;
                return true;
            case NumberType.Speed:
                player.moveSpeed += value;
                return true;
            default:
                return false;
        }
    }

    public override void Reset()
    {
        base.Reset();
        player.Reset();
    }
}
