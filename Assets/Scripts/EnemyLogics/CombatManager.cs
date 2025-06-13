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
        FRAME_COUNT ++;
        if (FRAME_COUNT % (60 * 10) == 0) OnTenSecondsTick();
    }

    private void OnTenSecondsTick() {
        // called when comes to integer minutes (1 min, 2 min)

        // make spawn interval a bit faster as game goes on
        spawn_wait_time *= (float)Math.Pow(spawn_interval_modifier_each_minute, 1.0 / 6);
        SetSpawnActivity(false);
        SetSpawnActivity(true);
    }

    public int GetCurrentFrame() {
        return FRAME_COUNT;
    }

    public void HandleEnemyDeath(GameObject enemy) {
        kill_count += 1;
        if (canvas_manager != null) canvas_manager.UpdateKillCount(kill_count);

        if (current_enemies.Contains(enemy)) {
            current_enemies.Remove(current_enemies.Find((x) => x.Equals(enemy)));
            // Debug.Log("removed enemy" + enemy.ToString());
        }

        SpawnDrops(enemy);

        // Debug.Log("kill count now is " + kill_count); // TODO: get ref to update UI
    }

    private bool RollChance(float chance_) {
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
            for(int i = 0; i < enemy_types.Count; i++) {
                if (RollChance(enemy_spawn_chances[i])) {
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

    private IEnumerator CR_SpawnThisEnemy(float wait_time_, GameObject enemy_, Vector2 location_) {
        yield return new WaitForSeconds(wait_time_);
        SpawnThisEnemy(enemy_, location_);
    }

    private void SpawnThisEnemy(GameObject enemy_, Vector2 location_) {
        var enemy_obj = Instantiate(enemy_, location_, Quaternion.identity);
        enemy_obj.GetComponent<EnemyMaster>().target = player.transform;
        current_enemies.Add(enemy_obj);
}

    public void SetSpawnActivity(bool is_active) {
        is_spawning = is_active;
        if (is_active) {
            spawn_timer = SpawnEnemy(spawn_wait_time);
            StartCoroutine(spawn_timer);
        } else {
            StopCoroutine(spawn_timer);
        }
    }

    public void HandleShowDamageUI(int damage_, PawnMaster reciever_, GameEvents.DamageType damage_type_, Vector2 location) {
        canvas_manager.DisplayDamage(damage_, reciever_, damage_type_, location);
    }

    private Vector2 GetRandomSpawnLocation() {
        Vector2 player_location = player.transform.position;
        float angle = UnityEngine.Random.Range(0.0f, Mathf.PI*2);
        Vector2 offset = (spawn_distance + UnityEngine.Random.Range(-spawn_tolerance, spawn_tolerance)) * new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
        return player_location + offset;
    }

    private Vector2 GetRandomLocationInCircle(Vector2 initial_location, float radius) {
        float angle = UnityEngine.Random.Range(0.0f, Mathf.PI*2);
        Vector2 offset = UnityEngine.Random.Range(0f, radius) * new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
        return initial_location + offset;
    }

    private void SpawnDrops(GameObject enemy) {
        Vector2 initial_location = enemy.transform.position;

        // apply DOTween sequence for items in drops and random spread within a range
        var seq = DOTween.Sequence();

        for (int i = 0; i < drops.Count; i++) {
            if (RollChance(drop_chances[i])) {
                GameObject drop = drops[i];
                var drop_obj = Instantiate(drop, initial_location, Quaternion.identity);
                Vector2 end_location = GetRandomLocationInCircle(initial_location, drop_radius);
                seq.Join(drop_obj.transform.DOMove(end_location, 1f));
            }
        }
    }
    
}
