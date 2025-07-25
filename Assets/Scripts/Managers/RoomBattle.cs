
using UnityEngine;
using System.Collections.Generic;

public class RoomBattle : RoomGrid
{

    public override void OnLevelStart()
    {

    }

    public override void OnLevelCleared()
    {

    }

    public override void OnRoomLoaded()
    {
        // This can be overridden by derived classes to handle room-specific loading logic
    }

    public override void OnRoomLeave()
    {
        // This can be overridden by derived classes to handle room-specific leave logic
    }
}