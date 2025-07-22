using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelDatabase", menuName = "Levels/LevelDatabase")]
public class LevelDatabase : ScriptableObject
{
    public static LevelDatabase instance;

    private void OnEnable()
    {
        instance = this;
    }

    [System.Serializable]
    public class LevelEntry
    {
        public int levelId;
        public Level levelAsset;
    }

    public List<LevelEntry> levels = new List<LevelEntry>();
    private Dictionary<int, Level> _lookup;

    public void Init()
    {
        if (_lookup == null)
        {
            _lookup = new Dictionary<int, Level>();
            foreach (var entry in levels)
            {
                if (entry.levelId != 0 && entry.levelAsset != null)
                    _lookup[entry.levelId] = entry.levelAsset;
            }
        }
    }

    public static Level GetLevel(int levelId)
    {
        if (instance == null) return null;
        instance.Init();
        instance._lookup.TryGetValue(levelId, out var level);
        return level;
    }

    public static List<Level> FindLevels(System.Func<Level, bool> predicate)
    {
        var result = new List<Level>();
        if (instance == null || instance.levels == null || instance.levels.Count == 0) return result;
        foreach (var entry in instance.levels)
        {
            if (entry.levelAsset == null) continue;
            if (predicate(entry.levelAsset))
                result.Add(entry.levelAsset);
        }
        return Shuffle(result);
    }

    public static Level GetRandomLevel(System.Func<Level, bool> predicate)
    {
        if (instance == null || instance.levels == null || instance.levels.Count == 0) return null;
        List<Level> filteredLevels = FindLevels(predicate);
        if (filteredLevels.Count == 0) return null;
        return filteredLevels[Random.Range(0, filteredLevels.Count)];
    }

    public static List<T> Shuffle<T>(List<T> list)
    {
        var rng = new System.Random();
        var shuffled = new List<T>(list);
        int n = shuffled.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = shuffled[k];
            shuffled[k] = shuffled[n];
            shuffled[n] = value;
        }
        return shuffled;
    }
}
