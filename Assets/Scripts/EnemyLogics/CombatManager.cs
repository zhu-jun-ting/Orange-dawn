

using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    private int kill_count = 0;

    [Header("spawn objects")]
    public List<GameObject> enemy_types;
    public List<float> enemy_spawn_chances;

    [Header("Spawn Area Checks")]
    public List<Transform> allowedSpawnAreas; // Assign in Inspector

    [Header("spawn parameters")]
    public float spawn_wait_time;
    public float spawn_distance;
    public float spawn_tolerance;
    public bool is_spawning;

    [Header("dropping objects")]

    public List<GameObject> drops;
    public List<float> drop_chances;
    private Dictionary<float, GameObject> random_drops; // TODO: unity can not serialize this, maybe setup another structure

    [Header("dropping paramters")]
    public float drop_radius;


    // private Transform canvas_manager_object;
    private ICanvasManager canvas_manager;
    private GameObject player;
    private IEnumerator spawn_timer;
    private List<GameObject> current_enemies;
    private List<GameObject> current_drops;


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
        current_enemies = new List<GameObject>();

        SetSpawnActivity(is_spawning); // TODO: for debug only
        FRAME_COUNT = 0;

        instance = this;
    }

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
        spawn_wait_time *= (float)Math.Pow(spawn_interval_modifier_each_minute, 1.0 / 6);
        SetSpawnActivity(false);
        SetSpawnActivity(is_spawning);
    }

    public int GetCurrentFrame()
    {
        return FRAME_COUNT;
    }

    public void HandleEnemyDeath(GameObject enemy)
    {
        kill_count += 1;
        if (canvas_manager != null) canvas_manager.UpdateKillCount(kill_count);

        if (current_enemies.Contains(enemy))
        {
            current_enemies.Remove(current_enemies.Find((x) => x.Equals(enemy)));
            // Debug.Log("removed enemy" + enemy.ToString());
        }

        SpawnDrops(enemy);

        // Debug.Log("kill count now is " + kill_count); // TODO: get ref to update UI
    }

    private bool RollChance(float chance_)
    {
        return UnityEngine.Random.Range(0f, 1f) < chance_;
    }

    // spawn enemy at outside of the circle
    private IEnumerator SpawnEnemy(float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);

            // var enemy = enemy_types[UnityEngine.Random.Range(0, enemy_types.Count)];
            Vector2 location = GetRandomSpawnLocation();

            // wait for the alert to stop to generate enemy
            for (int i = 0; i < enemy_types.Count; i++)
            {
                if (RollChance(enemy_spawn_chances[i]))
                {
                    Vector2 displacement = new Vector2(UnityEngine.Random.Range(0.1f, 0.2f), UnityEngine.Random.Range(0.1f, 0.2f));
                    IEnumerator cr_spawn_enemy = CR_SpawnThisEnemy(WARNING_TIME, enemy_types[i], location + displacement);
                    StartCoroutine(cr_spawn_enemy);
                }
            }



            // set up the alert prefab
            var alert_obj = Instantiate(alert_prefab, location, Quaternion.identity);
            // Debug.Log("spawn");
        }
    }

    private IEnumerator CR_SpawnThisEnemy(float wait_time_, GameObject enemy_, Vector2 location_)
    {
        yield return new WaitForSeconds(wait_time_);
        SpawnThisEnemy(enemy_, location_);
    }

    private void SpawnThisEnemy(GameObject enemy_, Vector2 location_)
    {
        var enemy_obj = Instantiate(enemy_, location_, Quaternion.identity);
        enemy_obj.GetComponent<EnemyMaster>().target = player.transform;
        current_enemies.Add(enemy_obj);
    }

    public void SetSpawnActivity(bool is_active)
    {
        if (is_active)
        {
            spawn_timer = SpawnEnemy(spawn_wait_time);
            StartCoroutine(spawn_timer);
        }
        else
        {
            if (spawn_timer != null)
            {
                StopCoroutine(spawn_timer);
                spawn_timer = null;
            }
        }
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

    private Vector2 GetRandomLocationInCircle(Vector2 initial_location, float radius)
    {
        float angle = UnityEngine.Random.Range(0.0f, Mathf.PI * 2);
        Vector2 offset = UnityEngine.Random.Range(0f, radius) * new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
        return initial_location + offset;
    }

    private void SpawnDrops(GameObject enemy)
    {
        Vector2 initial_location = enemy.transform.position;

        // apply DOTween sequence for items in drops and random spread within a range
        var seq = DOTween.Sequence();

        for (int i = 0; i < drops.Count; i++)
        {
            if (RollChance(drop_chances[i]))
            {
                GameObject drop = drops[i];
                var drop_obj = Instantiate(drop, initial_location, Quaternion.identity);
                Vector2 end_location = GetRandomLocationInCircle(initial_location, drop_radius);
                seq.Join(drop_obj.transform.DOMove(end_location, 1f));
                Debug.Log("Spawned drop: " + drop.name + " at " + end_location);
            }
        }
    }

    /// <summary>
    /// Try to get a spawnable location within a circle, checking against allowed areas.
    /// Returns null if no valid location found after maxIteration attempts.
    /// </summary>
    public Vector2? TryGetSpawnLocation(Vector2 origin, float radius, int maxIteration = 5)
    {
        for (int i = 0; i < maxIteration; i++)
        {
            // Generate random point in circle
            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2);
            float dist = UnityEngine.Random.Range(0f, radius);
            Vector2 candidate = origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;

            if (IsInsideAllowedAreas(candidate))
                return candidate;

            // Mirror over origin
            Vector2 mirror = origin - (candidate - origin);
            if (IsInsideAllowedAreas(mirror))
                return mirror;

            // Mirror X
            Vector2 mirrorX = new Vector2(origin.x - (candidate.x - origin.x), candidate.y);
            if (IsInsideAllowedAreas(mirrorX))
                return mirrorX;

            // Mirror Y
            Vector2 mirrorY = new Vector2(candidate.x, origin.y - (candidate.y - origin.y));
            if (IsInsideAllowedAreas(mirrorY))
                return mirrorY;
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
    public static void PlayFx(GameObject fx, Vector2 location, float scale, float duration = 1f)
    {
        if (fx == null) return;

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
            fxObj.transform.localScale = Vector3.one * scale;
        }

        // Destroy after duration (only if not already scheduled)
        if (duration > 0)
        {
            // Only destroy if this is a new instance
            if (!fx.scene.IsValid())
                Destroy(fxObj, duration);
        }
    }



    public static void PlayFx(string fxName, Vector2 location, float scale, float duration = 1f)
    {
        var instance = CombatManager.instance;
        if (instance == null || instance.oneTimeFx == null) return;

        // Instantiate a new FX object from the prefab
        GameObject fxObj = Instantiate(instance.oneTimeFx);
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
                        break;
                    }
                }
            }
            if (foundClip != null)
            {
                childAnimator.Play(fxName, 0, 0f);
                destroyDelay = foundClip.length;
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
        if (current_enemies == null || current_enemies.Count == 0)
            return Vector2.zero;

        GameObject nearestEnemy = null;
        float minDistSqr = float.MaxValue;

        foreach (var enemy in current_enemies)
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
}
