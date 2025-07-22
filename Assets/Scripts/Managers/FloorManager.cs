using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FloorManager : MonoBehaviour
{
    public static FloorManager instance;

    public enum RoomType { Battle, Shop, Event, Bonefire, MiniGame, None }

    [System.Serializable]
    public class MapGrid
    {
        public Vector2Int gridPos;
        public RoomType roomType;
        public GameObject roomObject;
        public bool isCreated;
    }

    [Header("Room Prefabs")]
    public GameObject battleRoomPrefab;
    public GameObject shopRoomPrefab;
    public GameObject eventRoomPrefab;
    public GameObject bonefireRoomPrefab;
    public GameObject miniGameRoomPrefab;
    public Vector2 roomOffset = new Vector2(20, 0); // Offset between rooms

    public Dictionary<Vector2Int, MapGrid> mapGrids = new Dictionary<Vector2Int, MapGrid>();
    public Vector2Int playerRoom = Vector2Int.zero;
    public int roomsEntered = 0;
    public List<Vector2Int> visitedRooms = new List<Vector2Int>();
    public List<Vector2Int> closedRooms = new List<Vector2Int>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    private void OnDisable()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnPlayerChoseNextRoom -= HandlePlayerNextRoom;
    }

    private void Start()
    {
        GameEvents.instance.OnPlayerChoseNextRoom += HandlePlayerNextRoom;
        CreateRoomAndNeighbors(playerRoom);
        visitedRooms.Add(playerRoom);
    }

    private List<int> loadedLevelIds = new List<int>();
    private int lastBattleLevelCleared = 0;
    private void HandlePlayerNextRoom(GameEvents.Dir dir)
    {
        // Player can not reenter a room that previously entered so close all doors towards that room
        closedRooms.Add(playerRoom);

        Vector2Int offset = DirToOffset(dir);
        playerRoom += offset;
        roomsEntered++;
        visitedRooms.Add(playerRoom);
        CreateRoomAndNeighbors(playerRoom);
        CleanupFarRooms();
        var grid = mapGrids[visitedRooms[visitedRooms.Count - 1]];
        RoomGrid roomGrid = grid.roomObject?.GetComponent<RoomGrid>();
        if (roomGrid != null)
        {
            roomGrid.DestroyBackwardTrigger();
            roomGrid.SetSigns();
            roomGrid.ShutDoorsToClosedRooms();
            CombatManager.instance.allowedSpawnAreas.Clear();
            CombatManager.instance.allowedSpawnAreas.AddRange(roomGrid.canSpawnAreas);

            // Level loading logic
            var levelDb = LevelDatabase.instance;
            if (levelDb != null)
            {
                int levelToLoad = -1;
                if (grid.roomType == RoomType.Battle)
                {
                    // Sequential battle levels
                    var battleLevels = LevelDatabase.FindLevels(l => l.roomType == RoomType.Battle)
                        .OrderBy(l => l.levelId).ToList();
                    int nextIdx = lastBattleLevelCleared;
                    if (nextIdx < battleLevels.Count)
                    {
                        var nextLevel = battleLevels[nextIdx];
                        if (nextLevel != null && !loadedLevelIds.Contains(nextLevel.levelId))
                        {
                            levelToLoad = nextLevel.levelId;
                            loadedLevelIds.Add(levelToLoad);
                            lastBattleLevelCleared++;
                        }
                    }
                }
                else
                {
                    // Random non-repeated level of this type
                    var levelsOfType = LevelDatabase.FindLevels(l => l.roomType == grid.roomType && !loadedLevelIds.Contains(l.levelId));
                    if (levelsOfType.Count > 0)
                    {
                        var chosen = levelsOfType[UnityEngine.Random.Range(0, levelsOfType.Count)];
                        levelToLoad = chosen.levelId;
                        loadedLevelIds.Add(levelToLoad);
                    }
                    else
                    {
                        // If all levels of this type have been loaded, pick a random one of that type (allow repeats)
                        var allLevelsOfType = LevelDatabase.FindLevels(l => l.roomType == grid.roomType);
                        if (allLevelsOfType.Count > 0)
                        {
                            var chosen = allLevelsOfType[UnityEngine.Random.Range(0, allLevelsOfType.Count)];
                            levelToLoad = chosen.levelId;
                        }
                    }
                }
                if (levelToLoad > 0)
                {
                    GameEvents.instance.LoadLevel(levelToLoad);
                }
            }
        }
    }

    private Vector2Int DirToOffset(GameEvents.Dir dir)
    {
        switch (dir)
        {
            case GameEvents.Dir.Up: return new Vector2Int(0, 1);
            case GameEvents.Dir.Down: return new Vector2Int(0, -1);
            case GameEvents.Dir.Left: return new Vector2Int(-1, 0);
            case GameEvents.Dir.Right: return new Vector2Int(1, 0);
            default: return Vector2Int.zero;
        }
    }

    private void CreateRoomAndNeighbors(Vector2Int center)
    {
        // Create center room if not exists
        CreateRoom(center);
        // Create 4 neighbors
        foreach (GameEvents.Dir dir in System.Enum.GetValues(typeof(GameEvents.Dir)))
        {
            Vector2Int neighbor = center + DirToOffset(dir);
            CreateRoom(neighbor);
        }
    }

    private void CleanupFarRooms()
    {
        List<Vector2Int> toRemove = new List<Vector2Int>();
        foreach (var kvp in mapGrids)
        {
            Vector2Int pos = kvp.Key;
            MapGrid grid = kvp.Value;
            if (grid.roomObject != null && Vector2Int.Distance(pos, playerRoom) >= 5f)
            {
                Destroy(grid.roomObject);
                grid.roomObject = null;
            }
        }
    }

    private void CreateRoom(Vector2Int pos)
    {
        if (mapGrids.ContainsKey(pos) && mapGrids[pos].isCreated) return;
        RoomType type = GetRandomRoomType(pos);
        GameObject prefab = GetPrefabForType(type);
        Vector3 spawnPos = new Vector3(pos.x * roomOffset.x, pos.y * roomOffset.y, 0);
        GameObject roomObj = null;
        if (prefab != null)
        {
            roomObj = Instantiate(prefab, spawnPos, Quaternion.identity);
            var gridComp = roomObj.GetComponent<RoomGrid>();
            if (gridComp != null)
                gridComp.Initialize(type, pos);
        }
        mapGrids[pos] = new MapGrid { gridPos = pos, roomType = type, roomObject = roomObj, isCreated = true };
    }

    private RoomType GetRandomRoomType(Vector2Int pos)
    {
        // Always make starting room a Battle
        if (pos == Vector2Int.zero) return RoomType.Battle;
        // Random selection
        RoomType[] types = new RoomType[] { RoomType.Battle, RoomType.Shop, RoomType.Event, RoomType.Bonefire, RoomType.MiniGame };
        return types[Random.Range(0, types.Length)];
    }

    private GameObject GetPrefabForType(RoomType type)
    {
        switch (type)
        {
            case RoomType.Battle: return battleRoomPrefab;
            case RoomType.Shop: return shopRoomPrefab;
            case RoomType.Event: return eventRoomPrefab;
            case RoomType.Bonefire: return bonefireRoomPrefab;
            case RoomType.MiniGame: return miniGameRoomPrefab;
            default: return null;
        }
    }

    public int GetRoomsPassed() => roomsEntered;
}
