using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Sticker card that attaches to an Action Card and, with 10% probability, charges an Ally NPC nearby when triggered.
/// </summary>
public class CardInstantStickerBattery : CardInstantStickerMaster
{
    [Header("Sticker Battery Settings")]
    [Range(0f, 1f)] public float chargeProbability = 0.1f;
    // No range filtering; select any Ally NPC in battle

    [Header("Buff Entry Text")]
    public string batteryBuffName = "Battery Sticker";
    [TextArea(3, 10)]
    public string batteryBuffDescription = "When this Action Card triggers, Probability: {0} to charge an Ally nearby.";

    public override void ActionOnTrigger(CardMaster card, Transform target)
    {
        if (UnityEngine.Random.value > chargeProbability) return;
        if (CombatManager.instance == null || target == null) return;
        var npcs = CombatManager.instance.currentNPCs;
        if (npcs == null || npcs.Count == 0) return;
        List<NPCMaster> allies = new List<NPCMaster>();
        foreach (var npc in npcs)
        {
            if (npc != null)
            {
                allies.Add(npc);
            }
        }
        if (allies.Count == 0) return;
        NPCMaster randomAlly = allies[UnityEngine.Random.Range(0, allies.Count)];
        if (randomAlly != null)
        {
            randomAlly.Charge(_ignoreCoolDown : true);
        }
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(batteryBuffDescription, (int) (chargeProbability * 100)));
    }

    public override string GetBuffEntryName()
    {
        return GameSettings.AddIcon(batteryBuffName);
    }

    public override string GetBuffEntryText()
    {
        return GameSettings.AddIcon(string.Format(batteryBuffDescription, (int) (chargeProbability * 100)));
    }
}
