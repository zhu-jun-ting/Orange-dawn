using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBuffable
{
    public void ApplyBuff(Buff buff_);
    // void BuffDamage(float amount_);
    void TakeDamage(float damage_, PawnMaster reciever_, GameObject instigator_ = null, GameEvents.DamageType damage_type_ = GameEvents.DamageType.Normal, Transform location_ = null, float hit_back_factor_ = 0f, Gun source_ = null);
    void UpdatePlayerContinuousAOE(ContiniousAOEStat stat_);
    void AddLifeStealPercent(float percent_);
    
}