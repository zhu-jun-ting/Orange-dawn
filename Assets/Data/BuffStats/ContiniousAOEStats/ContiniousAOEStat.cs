using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ContiniousAOEStat_", menuName = "BuffStat/ContiniousAOEStat", order = 2)]
public class ContiniousAOEStat : StatMaster {
    [Header("the compoenent for a valid ContiniousAOEStat buff")]
    public float additional_aoe_damage_per_tick;
    public float additional_aoe_range;

    public override void StartBuff(IBuffable buffable) {
        // called ONLY ONCE when the buff is added to the pawn
        // @param buffable : the pawn that should apply this buff to
        buffable.UpdatePlayerContinuousAOE(this);
    }
    public override void UpdateBuff(IBuffable buffable) {
        // Debug.Log("apply dot tick on " + buffable);
    }

    public override void EndBuff(IBuffable buffable) {
        // called ONLY ONCE when the buff is removed to the pawn
    }
}
