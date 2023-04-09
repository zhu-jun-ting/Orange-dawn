using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatMaster : ScriptableObject {
    // common for all buffs
    public Sprite buff_icon;
    public string buff_name;
    public string buff_description; 
    public float buff_duration;

    public virtual void StartBuff(IBuffable buffable) {}
    public virtual void UpdateBuff(IBuffable buffable) {}
    public virtual void EndBuff(IBuffable buffable) {}
}
