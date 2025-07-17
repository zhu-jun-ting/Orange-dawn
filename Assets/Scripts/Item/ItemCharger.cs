using UnityEngine;
using System.Collections.Generic;

public class ItemCharger : ItemMaster
{
    [Header("Charger Settings")]
    [Tooltip("Charge duration in seconds")] public int chargeDuration = 5;
    [Tooltip("Ignore charge cooldown")] public bool ignoreCooldown = true;

    public override void OnItemDestroyed(Collision2D collision)
    {
        base.OnItemDestroyed(collision);
        NPCMaster nearestNPC = FindNearestNPC();
        if (nearestNPC != null)
        {
            nearestNPC.Charge(chargeDuration, ignoreCooldown);
        }
    }

    private NPCMaster FindNearestNPC()
    {
        if (CombatManager.instance == null || CombatManager.instance.currentNPCs == null || CombatManager.instance.currentNPCs.Count == 0)
            return null;
        NPCMaster nearest = null;
        float minDist = float.MaxValue;
        Vector2 myPos = transform.position;
        foreach (var npc in CombatManager.instance.currentNPCs)
        {
            if (npc == null) continue;
            float dist = Vector2.Distance(myPos, npc.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = npc;
            }
        }
        return nearest;
    }
}