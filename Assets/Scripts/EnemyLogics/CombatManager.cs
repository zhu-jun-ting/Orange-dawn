using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class CombatManager : MonoBehaviour

{
    private int kill_count = 0;
    [Header("Global Settings")]
    [SerializeField] private bool _isInBattle = false;
    public static bool isInBattle
    {
        get => instance != null ? instance._isInBattle : false;
        set { if (instance != null) instance._isInBattle = value; }
    }// Global flag to indicate if the game is in battle mode

    [Header("Level Information")]
    public Level currentLevel; // The current level being played

    [Header("Spawn Area Checks")]
    public List<Transform> allowedSpawnAreas; // Assign in Inspector

    [Header("spawn parameters")]
    public float spawn_wait_time;
    public float spawn_distance;
    public float spawn_tolerance;
    public bool is_spawning;



    public enum DropItem
    {
        Health,
        Exp,
        Coin,
        Mana
    }

    [Serializable]
    public class DropItemPrefab
    {
        public DropItem dropType;
        public GameObject prefab;
    }

    [Header("Drop Prefabs")]
    public List<DropItemPrefab> dropPrefabs = new List<DropItemPrefab>();

    private Dictionary<DropItem, GameObject> dropPrefabDict;

    private void Awake()
    {
        dropPrefabDict = new Dictionary<DropItem, GameObject>();
        foreach (var item in dropPrefabs)
        {
            if (!dropPrefabDict.ContainsKey(item.dropType))
                dropPrefabDict.Add(item.dropType, item.prefab);
        }
    }

    [Header("dropping paramters")]
    public float drop_radius;


    // private Transform canvas_manager_object;
    private ICanvasManager canvas_manager;
    private GameObject player;
    private IEnumerator spawn_timer;


    [Header("game running parameters")]
    public float spawn_interval_modifier_each_minute;
    // [Tooltip("the time between each UpdateBuff is called")]
    public static float TICK_INTERVAL = 0.5f;
    public static float WARNING_TIME = 1f;


    [Header("Common Fx Management")]
    [SerializeField] private GameObject oneTimeFx; // Assign in Inspector
    [SerializeField] private GameObject lineFx; // Assign in Inspector



    [Header("DO NOT MODIFY")]
    public GameObject alert_prefab;
    private int FRAME_COUNT;
    public List<Transform> currentObjects = new List<Transform>();
    public List<GameObject> currentEnemies = new List<GameObject>();
    public List<NPCMaster> currentNPCs = new List<NPCMaster>();

    // instance
    public static CombatManager instance;
    public static bool is_update_card_registered = false;








    // Start is called before the first frame update
    void Start()
    {
        kill_count = 0;

        var canvas_manager_object = transform.parent.Find("CanvasManager");
        canvas_manager = canvas_manager_object.GetComponent<ICanvasManager>();
        if (canvas_manager == null) Debug.LogError("can not find canvas manager");

        player = GameObject.Find("Player");
        currentEnemies = new List<GameObject>();

        SetSpawnActivity(is_spawning); // TODO: for debug only
        FRAME_COUNT = 0;
        currentNPCs = FindObjectsByType<NPCMaster>(FindObjectsSortMode.None).ToList();


        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnSpawnObject -= HandlerOnSpawnObject;
            GameEvents.instance.OnSpawnObject += HandlerOnSpawnObject;
            GameEvents.instance.OnLoadLevel -= LoadLevel;
            GameEvents.instance.OnLoadLevel += LoadLevel;
            GameEvents.instance.OnLevelStart += HandleLevelStart;
            GameEvents.instance.OnLevelCleared += HandleLevelCleared;

        }

        instance = this;
    }

    public void LoadLevel(int levelIndex)
    {
        currentLevel = LevelDatabase.GetLevel(levelIndex);
        if (currentLevel == null)
        {
            Debug.LogError($"Level with index {levelIndex} not found!");
            return;
        }
        InitActiveEnemiesToSpawn();
        spawn_wait_time = currentLevel.spawnInterval;
    }

    public void HandleLevelStart()
    {
        
        SetSpawnActivity(true);
        isInBattle = true;

        if (currentLevel.clearRequirement == Level.LevelClearRequirement.TimeLimit)
        {
            StartCoroutine(LevelTimeLimitCoroutine(currentLevel.timeLimit));
        }

        if (FloorManager.instance.playerRoom == Vector2Int.zero) return; 

        if (currentLevel.clearRequirement == Level.LevelClearRequirement.DefeatAllEnemies && GameEvents.instance != null)
        {
            GameEvents.instance.ShowMessage(
                currentLevel.isBoss ? "Boss" : "Battle",
                GameEvents.MessageType.Banner,
                Vector2.zero
            );
            SoundManager.PlaySFX("DrawSword");
        }
        else if (currentLevel.roomType == FloorManager.RoomType.Bonefire && GameEvents.instance != null)
        {
            GameEvents.instance.ShowMessage(
                $"Bonfire",
                GameEvents.MessageType.Banner,
                Vector2.zero
            );
            SoundManager.PlaySFX("DrumStart");
        }
        else if (currentLevel.roomType == FloorManager.RoomType.Event && GameEvents.instance != null)
        {
            GameEvents.instance.ShowMessage(
                $"Event",
                GameEvents.MessageType.Banner,
                Vector2.zero
            );
            SoundManager.PlaySFX("DrumStart");
        }
        else if (currentLevel.roomType == FloorManager.RoomType.Shop && GameEvents.instance != null)
        {
            GameEvents.instance.ShowMessage(
                $"Shop",
                GameEvents.MessageType.Banner,
                Vector2.zero
            );
            SoundManager.PlaySFX("DrumStart");
        }
        else if (currentLevel.roomType == FloorManager.RoomType.MiniGame && GameEvents.instance != null)
        {
            GameEvents.instance.ShowMessage(
                $"MiniGame",
                GameEvents.MessageType.Banner,
                Vector2.zero
            );
            SoundManager.PlaySFX("DrumStart");
        }
    }

    private IEnumerator LevelTimeLimitCoroutine(float timeLimit)
    {
        yield return new WaitForSeconds(timeLimit);
        GameEvents.instance?.LevelCleared();
    }

    private void HandleLevelCleared()
    {
        SetSpawnActivity(false);
        isInBattle = false;
        currentEnemies.Clear();
        if (canvas_manager != null) canvas_manager.UpdateKillCount(0);
        if (currentLevel.clearRequirement == Level.LevelClearRequirement.DefeatAllEnemies && GameEvents.instance != null)
        {
            GameEvents.instance.ShowMessage(
                "Level Cleared",
                GameEvents.MessageType.Banner,
                Vector2.zero
            );
            SoundManager.PlaySFX("Cleared");
        }
    }

    void OnDisable()
    {
        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnSpawnObject -= HandlerOnSpawnObject;
            GameEvents.instance.OnLoadLevel -= LoadLevel;
        }
    }

    private void HandlerOnSpawnObject(Transform obj) => AddObject(obj);

    // Update is called once per frame
    void FixedUpdate()
    {
        FRAME_COUNT++;
        if (FRAME_COUNT % (60 * 10) == 0) OnTenSecondsTick();
    }

    private void OnTenSecondsTick()
    {
        // called when comes to integer minutes (1 min, 2 min)

        // make spawn interval a bit faster as game goes on
        // spawn_wait_time *= (float)Math.Pow(spawn_interval_modifier_each_minute, 1.0 / 6);
        // SetSpawnActivity(false);
        // SetSpawnActivity(is_spawning);
    }

    public int GetCurrentFrame()
    {
        return FRAME_COUNT;
    }

    public void HandleEnemyDeath(GameObject enemy)
    {
        kill_count += 1;
        if (canvas_manager != null) canvas_manager.UpdateKillCount(kill_count);

        if (currentEnemies.Contains(enemy))
        {
            currentEnemies.Remove(currentEnemies.Find((x) => x.Equals(enemy)));
            // Debug.Log("removed enemy" + enemy.ToString());
        }

        SpawnDrops(enemy);

        // Check if currentLevel is set to clear all enemies 
        if (currentLevel.clearRequirement == Level.LevelClearRequirement.DefeatAllEnemies)
        {
            if (currentEnemies.Count == 0 && (activeEnemiesToSpawn == null || activeEnemiesToSpawn.Count == 0))
            {
                // Trigger level clear event
                if (Time.time - GameEvents.instance.lastLevelStartOrClearTime > 1f) GameEvents.instance?.LevelCleared();
            }
        }

        // Debug.Log("kill count now is " + kill_count); // TODO: get ref to update UI
    }

    private bool RollChance(float chance_)
    {
        return UnityEngine.Random.Range(0f, 1f) < chance_;
    }

    // spawn enemy at outside of the circle
    // Callback to modify stats of spawned enemy
    public System.Action<EnemyMaster> OnModifySpawnedEnemyStats;

    // List of entries to spawn from currentLevel
    public List<Level.EnemyToSpawn> activeEnemiesToSpawn = new List<Level.EnemyToSpawn>();

    public void InitActiveEnemiesToSpawn()
    {
        activeEnemiesToSpawn.Clear();
        if (currentLevel != null && currentLevel.enemiesToSpawn != null)
        {
            foreach (var entry in currentLevel.enemiesToSpawn)
            {
                activeEnemiesToSpawn.Add(entry);
            }
        }
    }

    private IEnumerator SpawnEnemy(float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            Vector2 location = GetRandomSpawnLocation();

            // Remove entries with count <= 0
            activeEnemiesToSpawn.RemoveAll(e => e.count <= 0);
            if (activeEnemiesToSpawn.Count == 0)
            {
                SetSpawnActivity(false);
                yield break;
            }

            int spawnCount = Mathf.Min(UnityEngine.Random.Range(1, 4), activeEnemiesToSpawn.Count);
            for (int i = 0; i < spawnCount; i++)
            {
                if (activeEnemiesToSpawn.Count == 0) break;
                int idx = UnityEngine.Random.Range(0, activeEnemiesToSpawn.Count);
                var entry = activeEnemiesToSpawn[idx];
                if (entry.count <= 0) continue;

                // Spawn alert animation first
                Vector2 displacement = new Vector2(UnityEngine.Random.Range(0.1f, 0.2f), UnityEngine.Random.Range(0.1f, 0.2f));
                var alert_obj = Instantiate(alert_prefab, location + displacement, Quaternion.identity);
                yield return new WaitForSeconds(0.5f);

                // Actual spawn
                GameObject enemyObj = null;
                if (entry.enemyPrefab != null)
                {
                    enemyObj = Instantiate(entry.enemyPrefab, location + displacement, Quaternion.identity);
                    var master = enemyObj.GetComponent<EnemyMaster>();
                    if (master != null)
                    {
                        master.maxHP = entry.health * (GameSettings.instance != null ? GameSettings.instance.enemyHealthModifier : 1f);
                        master.curHP = entry.health * (GameSettings.instance != null ? GameSettings.instance.enemyHealthModifier : 1f);
                        master.attackDamage = entry.attack * (GameSettings.instance != null ? GameSettings.instance.enemyDamageModifier : 1f);
                        master.moveSpeed = entry.speed;
                        OnModifySpawnedEnemyStats?.Invoke(master);
                    }
                    currentEnemies.Add(enemyObj);
                }
                // Decrement count
                entry.count--;
                activeEnemiesToSpawn[idx] = entry;
            }

            // Remove entries with count <= 0 again
            activeEnemiesToSpawn.RemoveAll(e => e.count <= 0);
            if (activeEnemiesToSpawn.Count == 0)
            {
                SetSpawnActivity(false);
                yield break;
            }
        }
    }

    public void SetSpawnActivity(bool is_active)
    {
        if (is_active)
        {
            if (spawn_timer == null)
            {
                spawn_timer = SpawnEnemy(spawn_wait_time);
                StartCoroutine(spawn_timer);
            }
        }
        else
        {
            if (spawn_timer != null)
            {
                StopCoroutine(spawn_timer);
                spawn_timer = null;
            }
        }
        is_spawning = is_active;
    }

    public void HandleShowDamageUI(int damage_, PawnMaster reciever_, GameEvents.DamageType damage_type_, Vector2 location)
    {
        canvas_manager.DisplayDamage(damage_, reciever_, damage_type_, location);
    }

    private Vector2 GetRandomSpawnLocation()
    {
        Vector2 player_location = player.transform.position;
        float angle = UnityEngine.Random.Range(0.0f, Mathf.PI * 2);
        Vector2 offset = (spawn_distance + UnityEngine.Random.Range(-spawn_tolerance, spawn_tolerance)) * new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
        if (TryGetSpawnLocation(player_location, offset.magnitude, 5) is Vector2 spawn_location)
        {
            // if we can find a valid spawn location, return it
            return spawn_location;
        }
        else
        {
            return player_location + offset;
        }
    }

    private void SpawnDrops(GameObject enemy)
    {
        // apply DOTween sequence for items in drops and random spread within a range
        List<EnemyMaster.DropEntry> dropEntries = enemy.GetComponent<EnemyMaster>().dropEntries;
        foreach (var dropEntry in dropEntries)
        {
            if (RollChance(dropEntry.chance))
            {
                SpawnDrop(dropEntry.dropItem, enemy.transform, 1);
            }
        }
    }

    /// <summary>
    /// Spawn a specific drop item type, amount times, from a given location.
    /// Uses the dropPrefabDict to find the correct prefab.
    /// If the prefab is not found, it will not spawn anything.
    /// </summary>
    /// <param name="item">DropItems. Contains Coin, Exp, and Health</param>
    /// <param name="location">The location to spawn the drop</param>
    /// <param name="amount">The amount of drops to spawn</param>
    public void SpawnDrop(DropItem item, Transform location, int amount = 1)
    {
        if (dropPrefabDict == null || !dropPrefabDict.ContainsKey(item) || location == null) return;
        GameObject prefab = dropPrefabDict[item];
        if (prefab == null) return;
        Vector2 initial_location = location.position;
        var seq = DOTween.Sequence();
        for (int i = 0; i < amount; i++)
        {
            var drop_obj = Instantiate(prefab, initial_location, Quaternion.identity);
            Vector2 end_location = TryGetSpawnLocation(initial_location, drop_radius, minDistanceToOthers: 0.5f) ?? initial_location;
            seq.Join(drop_obj.transform.DOJump(end_location, 0.5f, 1, 1f));
        }
    }


    // -------- Spawn Logics --------

    // Track spawned objects for minDistanceToOthers checks


    /// <summary>
    /// Call this when you spawn an object that should be considered for spawn distance checks.
    /// </summary>
    public void AddObject(Transform obj)
    {
        if (obj != null && !currentObjects.Contains(obj))
            currentObjects.Add(obj);

        // Remove any nulls from the spawnedObjects list
        currentObjects.RemoveAll(obj => obj == null);
    }

    /// <summary>
    /// Try to get a spawnable location within a circle, checking against allowed areas.
    /// Returns null if no valid location found after maxIteration attempts.
    /// </summary>
    public Vector2? TryGetSpawnLocation(Vector2 origin, float radius, int maxIteration = 5, float minDistanceToOthers = 1f)
    {
        for (int i = 0; i < maxIteration; i++)
        {
            // Generate random point in circle
            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2);
            float dist = UnityEngine.Random.Range(0f, radius);
            Vector2 candidate = origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;

            // Try candidate and its mirrors
            Vector2[] candidates = new Vector2[] {
                candidate,
                origin - (candidate - origin), // mirror over origin
                new Vector2(origin.x - (candidate.x - origin.x), candidate.y), // mirror X
                new Vector2(candidate.x, origin.y - (candidate.y - origin.y)) // mirror Y
            };

            foreach (var cand in candidates)
            {
                if (IsInsideAllowedAreas(cand) && IsFarEnoughFromObjects(cand, minDistanceToOthers))
                    return cand;

                // If not far enough, try to move away from the closest object and check again (does not consume iteration)
                if (IsInsideAllowedAreas(cand))
                {
                    Vector2? moved = TryMoveAwayFromObjects(cand, minDistanceToOthers);
                    if (moved.HasValue && IsInsideAllowedAreas(moved.Value) && IsFarEnoughFromObjects(moved.Value, minDistanceToOthers))
                        return moved.Value;
                }
            }
        }
        // No valid location found
        return null;
    }


    /// <summary>
    /// Returns a random spawnable location within any allowed area.
    /// </summary>
    public Vector2? TryGetSpawnLocation(int maxIteration = 5)
    {
        for (int i = 0; i < maxIteration; i++)
        {
            if (allowedSpawnAreas == null || allowedSpawnAreas.Count == 0)
                return null;

            // Pick a random allowed area
            var area = allowedSpawnAreas[UnityEngine.Random.Range(0, allowedSpawnAreas.Count)];
            if (area == null) continue;

            var collider = area.GetComponent<Collider2D>();
            if (collider != null)
            {
                // Pick a random point within the collider's bounds
                var bounds = collider.bounds;
                Vector2 candidate = new Vector2(
                    UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                    UnityEngine.Random.Range(bounds.min.y, bounds.max.y)
                );
                if (collider.OverlapPoint(candidate))
                    return candidate;
            }
            else if (area is RectTransform rect)
            {
                // Pick a random point within the RectTransform
                Vector2 local = new Vector2(
                    UnityEngine.Random.Range(rect.rect.xMin, rect.rect.xMax),
                    UnityEngine.Random.Range(rect.rect.yMin, rect.rect.yMax)
                );
                Vector2 world = rect.TransformPoint(local);
                if (rect.rect.Contains(local))
                    return world;
            }
        }
        return null;
    }


    // Helper: check if candidate is far enough from all spawned objects
    private bool IsFarEnoughFromObjects(Vector2 candidate, float minDist)
    {
        foreach (var obj in currentObjects)
        {
            if (obj == null) continue;
            if (Vector2.Distance(candidate, (Vector2)obj.position) < minDist)
                return false;
        }
        return true;
    }

    // Helper: try to move candidate away from the closest object by the minimum distance
    private Vector2? TryMoveAwayFromObjects(Vector2 candidate, float minDist)
    {
        Transform closest = null;
        float closestDist = float.MaxValue;
        foreach (var obj in currentObjects)
        {
            if (obj == null) continue;
            float d = Vector2.Distance(candidate, (Vector2)obj.position);
            if (d < closestDist)
            {
                closestDist = d;
                closest = obj;
            }
        }
        if (closest != null && closestDist < minDist && closestDist > 0.01f)
        {
            // Move candidate away from closest object by the needed amount
            Vector2 dir = ((Vector2)candidate - (Vector2)closest.position).normalized;
            Vector2 moved = (Vector2)closest.position + dir * minDist;
            return moved;
        }
        return null;
    }


    /// <summary>
    /// Checks if a point is inside any allowed spawn area (using RectTransform or Collider2D).
    /// </summary>
    private bool IsInsideAllowedAreas(Vector2 point)
    {
        foreach (var t in allowedSpawnAreas)
        {
            if (t == null) continue;
            var collider = t.GetComponent<Collider2D>();
            if (collider != null && collider.OverlapPoint(point))
                return true;
            var rect = t as RectTransform;
            if (rect != null)
            {
                Vector2 localPoint = rect.InverseTransformPoint(point);
                if (rect.rect.Contains(localPoint))
                    return true;
            }
        }
        return false;
    }

    public static GameObject PlayFx(GameObject fx, Vector2 location, float scale = 1f, float duration = 1f, bool isLooping = false, Transform parent = null)
    {
        if (fx == null) return null;

        GameObject fxObj;

        // If fx is already in the scene (instantiated), just move and scale it
        if (fx.scene.IsValid())
        {
            fxObj = fx;
            fxObj.transform.position = location;
            fxObj.transform.localScale = Vector3.one * scale;
        }
        else
        {
            fxObj = Instantiate(fx, location, Quaternion.identity);
            if (parent != null) fxObj.transform.SetParent(parent, true);
            fxObj.transform.localScale = Vector3.one * scale;
        }

        // Handle looping if requested
        if (isLooping)
        {
            var animators = fxObj.GetComponentsInChildren<Animator>(true);
            foreach (var animator in animators)
            {
                if (animator.runtimeAnimatorController != null)
                {
                    foreach (var clip in animator.runtimeAnimatorController.animationClips)
                    {
                        if (clip != null)
                        {
                            clip.wrapMode = WrapMode.Loop;
                        }
                    }
                }
            }
        }

        // Destroy after duration (only if not already scheduled)
        if (duration > 0)
        {
            // Only destroy if this is a new instance
            if (!fx.scene.IsValid())
                Destroy(fxObj, duration);
        }

        return fxObj;
    }

    public static GameObject PlayFx(string fxName, Vector2 location, float scale = 1f, float duration = 1f, bool isLooping = false, Transform parent = null)
    {
        var instance = CombatManager.instance;
        if (instance == null || instance.oneTimeFx == null) return null;

        // Instantiate a new FX object from the prefab
        GameObject fxObj = Instantiate(instance.oneTimeFx);
        if (parent != null) fxObj.transform.SetParent(parent, true);
        fxObj.transform.position = location;
        fxObj.transform.localScale = Vector3.one * scale;

        // Find the only child with an Animator
        Animator childAnimator = null;
        if (fxObj.transform.childCount == 1)
        {
            var child = fxObj.transform.GetChild(0);
            childAnimator = child.GetComponent<Animator>();
        }
        else
        {
            // Fallback: search all children for the only Animator
            var animators = fxObj.GetComponentsInChildren<Animator>();
            if (animators.Length == 1)
                childAnimator = animators[0];
        }

        float destroyDelay = duration;
        if (childAnimator != null)
        {
            // Try to play the animation with the given name
            RuntimeAnimatorController controller = childAnimator.runtimeAnimatorController;
            AnimationClip foundClip = null;
            if (controller != null)
            {
                foreach (var clip in controller.animationClips)
                {
                    if (clip != null && clip.name == fxName)
                    {
                        foundClip = clip;
                        if (isLooping)
                        {
                            clip.wrapMode = WrapMode.Loop;
                        }
                        else
                        {
                            clip.wrapMode = WrapMode.Default;
                        }
                        break;
                    }
                }
            }
            if (foundClip != null)
            {
                childAnimator.Play(fxName, 0, 0f);
                if (!isLooping)
                    destroyDelay = foundClip.length;
                else
                    destroyDelay = duration;
            }
            else
            {
                // fallback: play default state
                childAnimator.Play(0, 0, 0f);
            }
        }

        if (destroyDelay > 0)
        {
            Destroy(fxObj, destroyDelay);
        }

        return fxObj;
    }

    /// <summary>
    /// Plays a segmented line FX using prefab lineFx, with each segment playing the animation named fxName.
    /// The line is split into as many segments as needed so that height/width <= maxRatio for each segment.
    /// All segments are destroyed after the animation duration or the given duration.
    /// </summary>
    public static void PlayFxLine(string fxName, Vector2 startPos, Vector2 endPos, float width = 1f, float maxRatio = 4f, float duration = 1f)
    {
        var instance = CombatManager.instance;
        if (instance == null || instance.lineFx == null || width <= 0f || maxRatio <= 0f) return;

        float totalLength = Vector2.Distance(startPos, endPos);
        float maxSegmentLength = width * maxRatio;
        int segment = Mathf.Max(1, Mathf.CeilToInt(totalLength / maxSegmentLength));
        float segmentLength = totalLength / segment;
        Vector2 dir = (endPos - startPos).normalized;
        float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;

        float destroyDelay = duration;
        AnimationClip foundClip = null;

        // Animator is now directly on the prefab (pivot at center)
        Animator prefabAnim = instance.lineFx.GetComponent<Animator>();
        if (prefabAnim != null && prefabAnim.runtimeAnimatorController != null)
        {
            foreach (var clip in prefabAnim.runtimeAnimatorController.animationClips)
            {
                if (clip != null && clip.name == fxName)
                {
                    foundClip = clip;
                    break;
                }
            }
            if (foundClip != null)
                destroyDelay = foundClip.length;
        }

        for (int i = 0; i < segment; i++)
        {
            float t0 = (float)i / segment;
            float t1 = (float)(i + 1) / segment;
            Vector2 segStart = Vector2.Lerp(startPos, endPos, t0);
            Vector2 segEnd = Vector2.Lerp(startPos, endPos, t1);
            Vector2 mid = (segStart + segEnd) * 0.5f;
            float segLen = Vector2.Distance(segStart, segEnd);

            GameObject fxObj = GameObject.Instantiate(instance.lineFx, mid, Quaternion.identity);
            fxObj.transform.localScale = new Vector3(width, segLen, 1f);
            fxObj.transform.rotation = Quaternion.Euler(0, 0, -angle);

            // Play animation on the attached Animator
            Animator segAnim = fxObj.GetComponent<Animator>();
            if (segAnim != null && foundClip != null)
            {
                segAnim.Play(fxName, 0, 0f);
            }
            else if (segAnim != null)
            {
                segAnim.Play(0, 0, 0f);
            }

            GameObject.Destroy(fxObj, destroyDelay);
        }
    }

    /// <summary>
    /// /// Returns the vector from the given location to the nearest enemy in current_enemies.
    /// If no enemies exist, returns Vector2.zero.
    /// </summary>
    public Vector2 GetVectorToNearestEnemy(Vector2 location)
    {
        if (currentEnemies == null || currentEnemies.Count == 0)
            return Vector2.zero;

        GameObject nearestEnemy = null;
        float minDistSqr = float.MaxValue;

        foreach (var enemy in currentEnemies)
        {
            if (enemy == null) continue;
            Vector2 enemyPos = enemy.transform.position;
            float distSqr = (enemyPos - location).sqrMagnitude;
            if (distSqr < minDistSqr)
            {
                minDistSqr = distSqr;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null)
            return ((Vector2)nearestEnemy.transform.position - location).normalized;
        else
            // Return a random unit vector if no enemies exist
            return UnityEngine.Random.insideUnitCircle.normalized;
    }




    [Header("LightningBox Settings")]
    [Tooltip("Prefab of the LightBeam to spawn")] public GameObject lightBeamPrefab;
    private float retriggerChance = 0.3f;
    private int maxChain = 2;
    private string[] enemyTags = new string[] { "Enemy" };
    private float lightningDamage = 5f; // Default damage for the lightning chain

    public void ShootLightningChain(Transform origin, float _damage = 5f, float _retriggerChance = 0.3f, int _maxChain = 2, string[] _enemyTags = null)
    {
        if (_retriggerChance < 0 || _retriggerChance > 1)
        {
            Debug.LogError("Retrigger chance must be between 0 and 1.");
            return;
        }
        retriggerChance = _retriggerChance;
        maxChain = _maxChain;
        lightningDamage = _damage;
        enemyTags = _enemyTags ?? new string[] { "Enemy" };
        ShootLightningChain(origin.position, 0);
    }

    private void ShootLightningChain(Vector2 startPos, int chainCount)
    {
        GameObject beam = Instantiate(lightBeamPrefab, startPos, Quaternion.identity);
        LightBeam beamScript = beam.GetComponent<LightBeam>();
        if (beamScript != null)
        {
            beamScript.targetTags = new System.Collections.Generic.List<string>(enemyTags);
            beamScript.useMaxLength = false; // Use actual target position
            beamScript.damage = lightningDamage; // Set the damage for the beam
            // Wait for the beam to fire, then possibly retrigger
            beamScript.StartCoroutine(RetriggerAfterBeam(beamScript, chainCount));
        }
    }

    private System.Collections.IEnumerator RetriggerAfterBeam(LightBeam beamScript, int chainCount)
    {
        // Wait for the beam to fire and deal damage
        yield return new WaitForSeconds(beamScript.duration * 0.9f);
        if (chainCount < maxChain && UnityEngine.Random.value < retriggerChance)
        {
            Vector2 end = beamScript.transform.position;
            if (beamScript != null)
            {
                // Try to get the actual beam end position if available
                var endField = beamScript.GetType().GetField("beamEnd", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (endField != null)
                {
                    end = (Vector2)endField.GetValue(beamScript);
                }
            }
            ShootLightningChain(end, chainCount + 1);
        }
    }
}
