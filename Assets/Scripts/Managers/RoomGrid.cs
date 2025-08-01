
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
    /// <summary>
    /// Shut doors to closed rooms. Optionally specify how many doors should remain open (openedDoors).
    /// If openedDoors == -1, use random open rule (at least one open).
    /// If openedDoors in 1~3, ensure exactly that many doors are open (if possible).
    /// Other values are treated as -1.
    /// </summary>
    public void ShutDoorsToClosedRooms(int openedDoors = -1)
    {
        var fm = FloorManager.instance;
        if (fm == null) return;
        var directions = new[] {
            (GameEvents.Dir.Up, doorUp),
            (GameEvents.Dir.Down, doorDown),
            (GameEvents.Dir.Left, doorLeft),
            (GameEvents.Dir.Right, doorRight)
        };

        // Collect all unvisited, openable doors
        List<(GameEvents.Dir dir, FloorDoor door, Vector2Int neighborPos)> openableDoors = new List<(GameEvents.Dir, FloorDoor, Vector2Int)>();
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
            else
            {
                if (door != null) openableDoors.Add((dir, door, neighborPos));
            }
                
        }

        // if any of the 3 doors to a boss room, then leave only that room open
        if (openableDoors.Any(entry => GetNeighborRoomGrid(entry.dir).roomType == FloorManager.RoomType.Boss))
        {
            foreach (var entry in openableDoors)
            {
                if (GetNeighborRoomGrid(entry.dir).roomType == FloorManager.RoomType.Boss)
                    entry.door.alwaysActive = false;
                else
                    entry.door.alwaysActive = true; // Ensure other doors are closed
            }
        }
        else
        {
            // Determine how many doors to open
            int doorsToOpen = -1;
            if (openedDoors >= 1 && openedDoors <= 3)
                doorsToOpen = Mathf.Min(openedDoors, openableDoors.Count);
            // else treat as -1 (random, at least one open)

            if (openableDoors.Count > 0)
            {
                // Reset all openable doors to closed (alwaysActive=true)
                foreach (var entry in openableDoors)
                    entry.door.alwaysActive = true;

                if (doorsToOpen == -1)
                {
                    doorsToOpen = Random.Range(1, Mathf.Min(4, openableDoors.Count + 1));
                }
                
                List<int> indices = new List<int>();
                for (int i = 0; i < openableDoors.Count; i++) indices.Add(i);
                for (int i = 0; i < doorsToOpen; i++)
                {
                    if (indices.Count == 0) break;
                    int pick = Random.Range(0, indices.Count);
                    openableDoors[indices[pick]].door.alwaysActive = false;
                    indices.RemoveAt(pick);
                }
            }
        }

        // Hide all visual signs first
        if (upSign != null) upSign.transform.parent.gameObject.SetActive(false);
        if (downSign != null) downSign.transform.parent.gameObject.SetActive(false);
        if (leftSign != null) leftSign.transform.parent.gameObject.SetActive(false);
        if (rightSign != null) rightSign.transform.parent.gameObject.SetActive(false);

        // Show visuals when GameEvents.LevelCleared event is triggered
        GameEvents.instance.OnLevelCleared -= OnLevelClearedShowSigns; // Prevent duplicate subscription
        GameEvents.instance.OnLevelCleared += OnLevelClearedShowSigns;
    }

    // Helper method to show the signs when LevelCleared is triggered
    private void OnLevelClearedShowSigns()
    {
        if (FloorManager.instance.playerRoom != gridLocation) return; // Only show signs for the current room
        if (upSign != null) upSign.transform.parent.gameObject.SetActive(!(doorUp != null && doorUp.alwaysActive));
        if (downSign != null) downSign.transform.parent.gameObject.SetActive(!(doorDown != null && doorDown.alwaysActive));
        if (leftSign != null) leftSign.transform.parent.gameObject.SetActive(!(doorLeft != null && doorLeft.alwaysActive));
        if (rightSign != null) rightSign.transform.parent.gameObject.SetActive(!(doorRight != null && doorRight.alwaysActive));

        // Unsubscribe after showing signs to avoid repeated calls
        GameEvents.instance.OnLevelCleared -= OnLevelClearedShowSigns;
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

    public virtual void OnLevelStart()
    {

    }

    public virtual void OnLevelCleared()
    {

    }

    public virtual void OnRoomLoaded()
    {
        // This can be overridden by derived classes to handle room-specific loading logic
    }

    public virtual void OnRoomLeave()
    {
        // This can be overridden by derived classes to handle room-specific leave logic
    }
}