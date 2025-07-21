using UnityEngine;

public class RoomGrid : MonoBehaviour
{
    public FloorManager.RoomType roomType;
    public Vector2Int gridLocation;

    // Optionally add more parameters here

    public void Initialize(FloorManager.RoomType type, Vector2Int location)
    {
        roomType = type;
        gridLocation = location;
    }

    private void Start()
    {

    }

    public void DestroyBackwardTrigger()
    {
        // If this is not the starting room, destroy PlayerChoice trigger towards the last room visited
        var fm = FloorManager.instance;
        if (fm == null || fm.visitedRooms.Count < 2) return;
        Vector2Int lastRoom = fm.visitedRooms[fm.visitedRooms.Count - 2];
        Vector2Int myRoom = gridLocation;
        Vector2Int diff = lastRoom - myRoom;
        GameEvents.Dir? blockDir = null;
        if (diff == new Vector2Int(0, 1)) blockDir = GameEvents.Dir.Up;
        else if (diff == new Vector2Int(0, -1)) blockDir = GameEvents.Dir.Down;
        else if (diff == new Vector2Int(-1, 0)) blockDir = GameEvents.Dir.Left;
        else if (diff == new Vector2Int(1, 0)) blockDir = GameEvents.Dir.Right;
        if (blockDir.HasValue)
        {
            foreach (var pc in GetComponentsInChildren<PlayerChoice>(true))
            {
                if (pc.direction == blockDir.Value)
                {
                    Destroy(pc.gameObject);
                }
            }
        }
    }
}