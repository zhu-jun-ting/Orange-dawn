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

    [System.Serializable]
    public class WeightedRoomPrefab
    {
        public GameObject prefab;
        [Range(0f, 1f)] public float weight = 1f;
    }

    [Header("Room Prefabs (Weighted)")]
    public List<WeightedRoomPrefab> battleRoomPrefabs = new List<WeightedRoomPrefab>();
    public List<WeightedRoomPrefab> shopRoomPrefabs = new List<WeightedRoomPrefab>();
    public List<WeightedRoomPrefab> eventRoomPrefabs = new List<WeightedRoomPrefab>();
    public List<WeightedRoomPrefab> bonefireRoomPrefabs = new List<WeightedRoomPrefab>();
    public List<WeightedRoomPrefab> miniGameRoomPrefabs = new List<WeightedRoomPrefab>();
    public Vector2 roomOffset = new Vector2(20, 0); // Offset between rooms

    public Dictionary<Vector2Int, MapGrid> mapGrids = new Dictionary<Vector2Int, MapGrid>();
    public Vector2Int playerRoom = Vector2Int.zero;
    public int roomsEntered = 0;
    public List<Vector2Int> visitedRooms = new List<Vector2Int>();
    public List<Vector2Int> closedRooms = new List<Vector2Int>();

    public static RoomGrid GetCurrentRoomGrid()
    {
        if (instance.mapGrids.TryGetValue(instance.playerRoom, out MapGrid grid))
        {
            return grid.roomObject?.GetComponent<RoomGrid>();
        }
        return null;
    }

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    private void OnDisable()
    {
        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnPlayerChoseNextRoom -= HandlePlayerNextRoom;      
            GameEvents.instance.OnLevelStart -= HandleLevelStart;
            GameEvents.instance.OnLevelCleared -= HandleLevelCleared;
        }
    }

    private void Start()
    {
        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnPlayerChoseNextRoom += HandlePlayerNextRoom;
            GameEvents.instance.OnLevelStart += HandleLevelStart;
            GameEvents.instance.OnLevelCleared += HandleLevelCleared;
        }
        
        CreateRoomAndNeighbors(playerRoom);
        GetCurrentRoomGrid()?.SetSigns();
        visitedRooms.Add(playerRoom);
    }

    private void HandleLevelStart()
    {
        // Call the current RoomGrid's OnLevelStart
        var currentRoom = GetCurrentRoomGrid();
        if (currentRoom != null)
        {
            currentRoom.OnLevelStart();
        }
    }

    private void HandleLevelCleared()
    {
        // Call the current RoomGrid's OnLevelCleared
        var currentRoom = GetCurrentRoomGrid();
        if (currentRoom != null)
        {
            currentRoom.OnLevelCleared();
        }
    }

    private List<int> loadedLevelIds = new List<int>();
    private int lastBattleLevelCleared = 0;
    private void HandlePlayerNextRoom(GameEvents.Dir dir)
    {
        // Player can not reenter a room that previously entered so close all doors towards that room
        closedRooms.Add(playerRoom);
        var currentRoom = GetCurrentRoomGrid();
        currentRoom.OnRoomLeave();

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
            roomGrid.OnRoomLoaded();

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
                    // If all sequential levels are loaded, pick a random one from the list
                    else
                    {
                        var randomLevel = battleLevels[UnityEngine.Random.Range(0, battleLevels.Count)];
                        levelToLoad = randomLevel.levelId;
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

    [System.Serializable]
    public class RoomTypeWeight
    {
        public RoomType type;
        [Range(0f, 1f)] public float weight = 0.2f;
    }

    [Header("Room Type Weights")]
    public List<RoomTypeWeight> roomTypeWeights = new List<RoomTypeWeight>
    {
        new RoomTypeWeight { type = RoomType.Battle, weight = 0.2f },
        new RoomTypeWeight { type = RoomType.Shop, weight = 0.2f },
        new RoomTypeWeight { type = RoomType.Event, weight = 0.2f },
        new RoomTypeWeight { type = RoomType.Bonefire, weight = 0.2f },
        new RoomTypeWeight { type = RoomType.MiniGame, weight = 0.2f }
    };

    private RoomType GetRandomRoomType(Vector2Int pos)
    {
        // Always make starting room a None
        if (pos == Vector2Int.zero) return RoomType.Battle;
        // Weighted random selection
        float totalWeight = 0f;
        foreach (var entry in roomTypeWeights) totalWeight += entry.weight;
        if (totalWeight <= 0f) return RoomType.Battle;
        float rand = Random.value * totalWeight;
        float accum = 0f;
        foreach (var entry in roomTypeWeights)
        {
            accum += entry.weight;
            if (rand <= accum)
                return entry.type;
        }
        return roomTypeWeights.Count > 0 ? roomTypeWeights[0].type : RoomType.Battle;
    }

    private GameObject GetPrefabForType(RoomType type)
    {
        List<WeightedRoomPrefab> list = null;
        switch (type)
        {
            case RoomType.Battle: list = battleRoomPrefabs; break;
            case RoomType.Shop: list = shopRoomPrefabs; break;
            case RoomType.Event: list = eventRoomPrefabs; break;
            case RoomType.Bonefire: list = bonefireRoomPrefabs; break;
            case RoomType.MiniGame: list = miniGameRoomPrefabs; break;
            default: return null;
        }
        if (list == null || list.Count == 0) return null;
        float totalWeight = 0f;
        foreach (var item in list) totalWeight += item.weight;
        if (totalWeight <= 0f) return list[Random.Range(0, list.Count)].prefab;
        float rand = Random.value * totalWeight;
        float accum = 0f;
        foreach (var item in list)
        {
            accum += item.weight;
            if (rand <= accum)
                return item.prefab;
        }
        return list[list.Count - 1].prefab;
    }

    public int GetRoomsPassed() => roomsEntered;
}
