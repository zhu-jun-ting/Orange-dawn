using System;
using UnityEngine;

/// <summary>
/// Sticker card that attaches to an Action Card and, with 20% probability, inflicts fire (BurnAOE) around when triggered.
/// </summary>
public class CardInstantStickerTorch : CardInstantStickerMaster
{
    [Header("Sticker Torch Settings")]
    [Range(0f, 1f)] public float triggerProbability = 0.2f;
    public float burnDamage = 3f;
    public float maxRadius = 3f;
    public float maxDamage = 12f;
    public GameObject burnAOEPrefab; // Assign BurnAOE prefab in inspector

    [Header("Buff Entry Text")]
    public string torchBuffName = "Torch Sticker";
    [TextArea(3, 10)]
    public string torchBuffDescription = "When this Action Card triggers, Probability: {0} to inflict Burn around.";

    public override void ActionOnTrigger(CardMaster card, Transform target)
    {
        if (UnityEngine.Random.value > triggerProbability) return;
        if (burnAOEPrefab == null || target == null) return;
        GameObject aoe = GameObject.Instantiate(burnAOEPrefab, target.position, Quaternion.identity);
        BurnAOE aoeScript = aoe.GetComponent<BurnAOE>();
        if (aoeScript != null)
        {
            aoeScript.burnDamage = burnDamage;
            aoeScript.maxRadius = maxRadius;
            aoeScript.maxDamage = maxDamage;
            aoeScript.isPlayingFx = true;
        }
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(torchBuffDescription, (int) (triggerProbability*100)));
    }

    public override string GetBuffEntryName()
    {
        return GameSettings.AddIcon(torchBuffName);
    }

    public override string GetBuffEntryText()
    {
        return GameSettings.AddIcon(string.Format(torchBuffDescription, (int) (triggerProbability*100)));
    }
}
