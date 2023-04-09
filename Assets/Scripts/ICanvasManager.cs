using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICanvasManager
{
    public void UpdateKillCount(int kill_count);
    public void DisplayDamage( int damage_, PawnMaster reciever_, GameEvents.DamageType damage_type_, Vector2 location_ );
}