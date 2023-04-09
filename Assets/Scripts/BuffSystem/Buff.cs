using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff
{

    public enum BuffType {default_buff, hot, dot, stun, speed}

    public BuffType buff_type; 
    public float duration;
    public Sprite icon;
    public string buff_name;
    public string buff_description;
    // buff stat
    protected StatMaster stat;

    public void StartBuff(IBuffable buffable) {
        // called ONLY ONCE when the buff is added to the pawn
        // @param buffable : the pawn that should apply this buff to
        stat.StartBuff(buffable);
    }

    public void UpdateBuff(IBuffable buffable) {
        // called on update for EVERY TICK INTERVAL (current is .5 sec per tick)
        stat.UpdateBuff(buffable);
    }

    public void EndBuff(IBuffable buffable) {
        // called ONLY ONCE when the buff is removed to the pawn
        stat.EndBuff(buffable);
    }

    public Buff(StatMaster stat_) {
        stat = stat_;

        icon = stat.buff_icon;
        duration = stat.buff_duration;
        buff_name = stat.buff_name;
        buff_description = stat.buff_description;
    }
}
