using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStat", menuName = "Stat/PlayerStat", order = 1)]
public class PlayerStat : ScriptableObject {
    [Header("the initial setup of the player (or NPC) stats, may be modified dynamically in the game.")]
    public float max_health;
    public float move_speed;
    public float dash_speed_multiplier;
    public float dash_duration;
}