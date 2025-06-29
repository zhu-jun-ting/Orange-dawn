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

    public override void UpdateNumberValue(CardMaster.NumberType numberType, float value, CardMaster source)
    {
        if (IsBuffedFromSource(source, addToList: true, includeSelf: true))
        {
            return;
        }

        base.UpdateNumberValue(numberType, value, source);

        if (player == null) player = PlayerController.instance;
        if (player == null) return;

        switch (numberType)
        {
            case NumberType.Health:
                player.max_health += value;
                player.UpdateMaxHealth();
                break;
            case NumberType.Probablity:
                player.dodge += value;
                break;
            case NumberType.Speed:
                player.moveSpeed += value;
                break;
            default:
                Debug.LogError($"UpdateNumberValue not implemented for {instance.name}. NumberType: {numberType}, Value: {value}");
                break;
        }
    }

    public override void Reset()
    {
        base.Reset();
        player.Reset();
    }
}
