using System;
using UnityEngine;

/// <summary>
/// Sticker card that attaches to an Action Card and, with 30% probability, shoots a vertical beam with Damage: 6 when triggered.
/// </summary>
public class CardInstantStickerStrip : CardInstantStickerMaster
{
    [Header("Sticker Strip Settings")]
    [Range(0f, 1f)] public float triggerProbability = 0.3f;
    public float beamDamage = 6f;
    public GameObject lightBeamPrefab; // Assign LightBeamStraight prefab in inspector

    [Header("Buff Entry Text")]
    public string stripBuffName = "Strip Sticker";
    [TextArea(3, 10)]
    public string stripBuffDescription = "When this Action Card triggers, Probability: {0} to shoot a vertical beam with Damage: {1}.";

    public override void ActionOnTrigger(CardMaster card, Transform target)
    {
        if (UnityEngine.Random.value > triggerProbability) return;
        if (lightBeamPrefab == null || target == null) return;
        GameObject beam = GameObject.Instantiate(lightBeamPrefab, target.position, Quaternion.identity);
        LightBeamStraight beamScript = beam.GetComponent<LightBeamStraight>();
        if (beamScript != null)
        {
            beamScript.horizontal = false;
            beamScript.vertical = true;
            beamScript.damage = beamDamage;
        }
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(stripBuffDescription, (int) (triggerProbability*100), beamDamage));
    }

    public override string GetBuffEntryName()
    {
        return GameSettings.AddIcon(stripBuffName);
    }

    public override string GetBuffEntryText()
    {
        return GameSettings.AddIcon(string.Format(stripBuffDescription, (int) (triggerProbability*100), beamDamage));
    }
}
