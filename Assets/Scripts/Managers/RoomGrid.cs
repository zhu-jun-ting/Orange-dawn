
using UnityEngine;
using System.Collections.Generic;

public class RoomGrid : MonoBehaviour
{
    public FloorManager.RoomType roomType;
    public Vector2Int gridLocation;

    [Header("Room Information")]
    public List<Transform> canSpawnAreas;

    [Header("Room Doors")]
    public FloorDoor doorUp;
    public FloorDoor doorDown;
    public FloorDoor doorLeft;
    public FloorDoor doorRight;

    [Header("Room Signs")]
    public Transform signHolder;
    public SpriteRenderer upSign;
    public SpriteRenderer downSign;
    public SpriteRenderer leftSign;
    public SpriteRenderer rightSign;

    // Optionally add more parameters here

    public void Initialize(FloorManager.RoomType type, Vector2Int location)
    {
        roomType = type;
        gridLocation = location;
    }

    private void Start()
    {

    }

    public void ShutDoorsToClosedRooms()
    {
        var fm = FloorManager.instance;
        if (fm == null) return;
        var directions = new[] {
            (GameEvents.Dir.Up, doorUp),
            (GameEvents.Dir.Down, doorDown),
            (GameEvents.Dir.Left, doorLeft),
            (GameEvents.Dir.Right, doorRight)
        };
        foreach (var (dir, door) in directions)
        {
            Vector2Int offset = Vector2Int.zero;
            switch (dir)
            {
                case GameEvents.Dir.Up: offset = new Vector2Int(0, 1); break;
                case GameEvents.Dir.Down: offset = new Vector2Int(0, -1); break;
                case GameEvents.Dir.Left: offset = new Vector2Int(-1, 0); break;
                case GameEvents.Dir.Right: offset = new Vector2Int(1, 0); break;
            }
            Vector2Int neighborPos = gridLocation + offset;
            if (fm.visitedRooms.Contains(neighborPos) && door != null)
            {
                door.alwaysActive = true;
            }
        }
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
    
    // Returns a dictionary of RoomGrid for 4 directions (Up, Down, Left, Right) from this room
    public RoomGrid GetNeighborRoomGrid(GameEvents.Dir dir)
    {
        var fm = FloorManager.instance;
        if (fm == null) return null;
        Vector2Int offset = Vector2Int.zero;
        switch (dir)
        {
            case GameEvents.Dir.Up: offset = new Vector2Int(0, 1); break;
            case GameEvents.Dir.Down: offset = new Vector2Int(0, -1); break;
            case GameEvents.Dir.Left: offset = new Vector2Int(-1, 0); break;
            case GameEvents.Dir.Right: offset = new Vector2Int(1, 0); break;
        }
        Vector2Int neighborPos = gridLocation + offset;
        if (fm.mapGrids.TryGetValue(neighborPos, out var grid) && grid.roomObject != null)
        {
            return grid.roomObject.GetComponent<RoomGrid>();
        }
        return null;
    }

    public void SetSigns()
    {
        signHolder.gameObject.SetActive(true);
        var fm = FloorManager.instance;
        if (fm == null) return;
        // Set signs for available directions using GetNeighborRoomGrid
        upSign.sprite = GetNeighborRoomGrid(GameEvents.Dir.Up) != null ? GameSettings.GetRoomSprite(GetNeighborRoomGrid(GameEvents.Dir.Up).roomType) : null;
        downSign.sprite = GetNeighborRoomGrid(GameEvents.Dir.Down) != null ? GameSettings.GetRoomSprite(GetNeighborRoomGrid(GameEvents.Dir.Down).roomType) : null;
        leftSign.sprite = GetNeighborRoomGrid(GameEvents.Dir.Left) != null ? GameSettings.GetRoomSprite(GetNeighborRoomGrid(GameEvents.Dir.Left).roomType) : null;
        rightSign.sprite = GetNeighborRoomGrid(GameEvents.Dir.Right) != null ? GameSettings.GetRoomSprite(GetNeighborRoomGrid(GameEvents.Dir.Right).roomType) : null;

        // Determine backward direction
        GameEvents.Dir? backwardDir = null;
        if (fm.visitedRooms.Count >= 2)
        {
            Vector2Int lastRoom = fm.visitedRooms[fm.visitedRooms.Count - 2];
            Vector2Int diff = lastRoom - gridLocation;
            if (diff == new Vector2Int(0, 1)) backwardDir = GameEvents.Dir.Up;
            else if (diff == new Vector2Int(0, -1)) backwardDir = GameEvents.Dir.Down;
            else if (diff == new Vector2Int(-1, 0)) backwardDir = GameEvents.Dir.Left;
            else if (diff == new Vector2Int(1, 0)) backwardDir = GameEvents.Dir.Right;
        }

        // Set backward sign and its parent inactive
        if (backwardDir.HasValue)
        {
            switch (backwardDir.Value)
            {
                case GameEvents.Dir.Up:
                    upSign.transform.parent.gameObject.SetActive(false);
                    break;
                case GameEvents.Dir.Down:
                    downSign.transform.parent.gameObject.SetActive(false);
                    break;
                case GameEvents.Dir.Left:
                    leftSign.transform.parent.gameObject.SetActive(false);
                    break;
                case GameEvents.Dir.Right:
                    rightSign.transform.parent.gameObject.SetActive(false);
                    break;
            }
        }
    }
}