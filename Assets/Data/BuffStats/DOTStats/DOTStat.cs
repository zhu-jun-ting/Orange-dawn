using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DOTStat_", menuName = "BuffStat/DotStat", order = 1)]
public class DOTStat : StatMaster {
    [Header("the compoenent for a valid DOT buff")]
    public float dot_damage_per_second;

    public override void StartBuff(IBuffable buffable) {
        // called ONLY ONCE when the buff is added to the pawn
        // @param buffable : the pawn that should apply this buff to

    }
    public override void UpdateBuff(IBuffable buffable) {
        // called on update for EVERY TICK INTERVAL 
        // @param interval : how many ticks interval should this update being called
        //      e.g. interval = 2 mean every TICK_INTERVAL * 2 seconds between each call of UpdateBuff
        // P.S. interval should be implemented by a timer inside this function
        Debug.Log(buffable); 
        buffable.Damage(dot_damage_per_second * CombatManager.TICK_INTERVAL, GameEvents.DamageType.DotDamage, 0f, null);
    }

    public override void EndBuff(IBuffable buffable) {
        // called ONLY ONCE when the buff is removed to the pawn
    }
}
