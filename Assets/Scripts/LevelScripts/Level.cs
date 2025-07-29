using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Make Level creatable in inspector
[CreateAssetMenu(fileName = "Level", menuName = "Levels/Level")]
public class Level : ScriptableObject
{

	[Header("Level Info")]
	public int levelId;
	public FloorManager.RoomType roomType = FloorManager.RoomType.Battle;

	[Header("Battle: Enemy Info")]
	public List<EnemyToSpawn> enemiesToSpawn = new List<EnemyToSpawn>();

	// Enemy stats struct
	[System.Serializable]
	public struct EnemyToSpawn
	{
		public GameObject enemyPrefab;
		public int count;
		public int health;
		public int attack;
		public float speed;
		// Add more as needed
	}

	[Header("Battle: Setups")]
	[Range(0, 5)] public float spawnInterval = 2f; // how often to spawn enemies
	public bool isBoss = false; // is this a boss level?


	public enum LevelClearRequirement
	{
		None, // No specific requirement
		DefeatAllEnemies, // Defeat all enemies
		TimeLimit, // Complete within a time limit
		CollectItems, // Collect specific items
	}

	[Header("Level Clear Requirements")]
	[Tooltip("What is the requirement to clear this level?")]
	public LevelClearRequirement clearRequirement = LevelClearRequirement.None;
	public float timeLimit = 0f; // 0 means no time limit

}

