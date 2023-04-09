using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LifeStealStat_", menuName = "BuffStat/LifeStealStat", order = 3)]
public class LifeStealStat : StatMaster {
    [Header("the compoenent for a valid LifeSteal buff")]
    public float additional_lifesteal_percent;

    public override void StartBuff(IBuffable buffable) {
        // called ONLY ONCE when the buff is added to the pawn
        // @param buffable : the pawn that should apply this buff to
        buffable.AddLifeStealPercent(additional_lifesteal_percent);

    }
    public override void UpdateBuff(IBuffable buffable) {
        // called on update for EVERY TICK INTERVAL 
    }

    public override void EndBuff(IBuffable buffable) {
        // called ONLY ONCE when the buff is removed to the pawn
    }
}
