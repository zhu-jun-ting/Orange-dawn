using UnityEngine;

/// <summary>
/// RoomEvent: A room type that contains a reference to an NPC GameObject that can be talked to.
/// </summary>
public class RoomEvent : RoomGrid
{
    [Header("Event NPC")]
    public GameObject npcToTalkTo; // Assign in inspector or dynamically
    // You can add more event-specific logic here if needed
}
