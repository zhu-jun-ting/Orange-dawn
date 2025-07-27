using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour

{
    public static SoundManager instance;

    [Header("Audio Sources")]
    [Tooltip("Template for music AudioSource (should be disabled in scene)")]
    public AudioSource musicSourceTemplate;
    public AudioSource sfxSource;

    [Header("Audio Clips")] 
    public List<NamedAudioClip> musicClips = new List<NamedAudioClip>();
    public List<NamedAudioClip> sfxClips = new List<NamedAudioClip>();

    private Dictionary<string, AudioClip> musicDict = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxDict = new Dictionary<string, AudioClip>();

    // List of currently playing music AudioSources
    private static List<AudioSource> currentMusicSources = new List<AudioSource>();
    private static float musicVolume = 1f;

    [System.Serializable]
    public class NamedAudioClip
    {
        public string key;
        public AudioClip clip;
        [Range(0f, 2f)] public float volume = 1f;
    }

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
        InitDictionaries();
        // Clean up any music sources from previous scenes
        foreach (var src in currentMusicSources) if (src != null) Destroy(src.gameObject);
        currentMusicSources.Clear();
    }

    private void InitDictionaries()
    {
        musicDict.Clear();
        foreach (var entry in musicClips)
            if (!string.IsNullOrEmpty(entry.key) && entry.clip != null)
                musicDict[entry.key] = entry.clip;
        sfxDict.Clear();
        foreach (var entry in sfxClips)
            if (!string.IsNullOrEmpty(entry.key) && entry.clip != null)
                sfxDict[entry.key] = entry.clip;
    }

    // Play music by key with fade in
    public static AudioSource PlayMusic(string key, float volume = 1f, bool loop = true, float fadeIn = 0.5f)
    {
        if (instance == null || instance.musicSourceTemplate == null) return null;
        var named = instance.musicClips.Find(x => x.key == key);
        if (named != null && named.clip != null)
        {
            var src = Instantiate(instance.musicSourceTemplate, instance.transform);
            src.gameObject.SetActive(true);
            src.clip = named.clip;
            src.volume = 0f;
            src.loop = loop;
            src.Play();
            currentMusicSources.Add(src);
            instance.StartCoroutine(FadeMusicVolume(src, 0f, volume * named.volume * musicVolume, fadeIn));
            return src;
        }
        return null;
    }

    // Fade out and stop music
    public static void StopMusic(AudioSource src, float fadeOut = 0.5f)
    {
        if (src != null)
        {
            instance.StartCoroutine(FadeOutAndStop(src, fadeOut));
        }
    }

    // Fade out and stop all music
    public static void StopAllMusic(float fadeOut = 0.5f)
    {
        foreach (var src in currentMusicSources)
        {
            if (src != null) instance.StartCoroutine(FadeOutAndStop(src, fadeOut));
        }
        currentMusicSources.Clear();
    }

    // Coroutine to fade in/out music volume
    private static System.Collections.IEnumerator FadeMusicVolume(AudioSource src, float from, float to, float duration)
    {
        if (src == null) yield break;
        float t = 0f;
        src.volume = from;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        src.volume = to;
    }

    private static System.Collections.IEnumerator FadeOutAndStop(AudioSource src, float duration)
    {
        if (src == null) yield break;
        float startVol = src.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(startVol, 0f, t / duration);
            yield return null;
        }
        src.volume = 0f;
        src.Stop();
        currentMusicSources.Remove(src);
        if (src != null) Destroy(src.gameObject);
    }

    // (StopMusic and StopAllMusic replaced above with fade out)

    // Play sfx by key
    public static void PlaySFX(string key, float volume = 1f)
    {
        if (instance == null || instance.sfxSource == null) return;
        var named = instance.sfxClips.Find(x => x.key == key);
        if (named != null && named.clip != null)
        {
            instance.sfxSource.PlayOneShot(named.clip, volume * named.volume);
        }
    }

    // Play sfx by a list of keys (randomly pick one)
    public static void PlaySFX(List<string> keys, float volume = 1f)
    {
        if (keys == null || keys.Count == 0) return;
        int idx = Random.Range(0, keys.Count);
        PlaySFX(keys[idx], volume);
    }

    // Set music volume
    public static void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        foreach (var src in currentMusicSources)
        {
            if (src != null) src.volume = volume;
        }
    }

    // Set sfx volume
    public static void SetSFXVolume(float volume)
    {
        if (instance == null || instance.sfxSource == null) return;
        instance.sfxSource.volume = volume;
    }


    // Register to GameEvents for sound triggers
    void Start()
    {
        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnHitPawn += (damage, pawn, instigator, type, loc, hitBack, gun) => PlaySFX("Hit2");
            GameEvents.instance.OnHealPawn += (heal, pawn, instigator, loc) => PlaySFX(new List<string> { "Heal1", "Heal2" });
            // GameEvents.instance.OnPawnDie += (pawn, dmg, inst, type, gun) => PlaySFX("die", 1f);
            GameEvents.instance.OnUpdateCoins += (diff) => PlaySFX(new List<string> { "Coin1", "Coin2" });
            GameEvents.instance.OnLevelCleared += () => { if (CombatManager.instance.currentLevel.clearRequirement != Level.LevelClearRequirement.TimeLimit) PlaySFX("Cleared"); };
            GameEvents.instance.OnHitWall += (bullet, position, wallObject) => PlaySFX("Punch");
            GameEvents.instance.OnTriggerActionCard += (card, target) => PlaySFX("Pop");
            GameEvents.instance.OnPawnDie += (pawn, dmg, inst, type, gun) => PlaySFX(new List<string> { "Death1", "Death2" });
            GameEvents.instance.OnDestroyObject += (pawn, gun) => PlaySFX(new List<string> { "Death1", "Death2" });
            GameEvents.instance.OnSpawnObject += (obj) => PlaySFX("Smoke");
            GameEvents.instance.OnShowMessage += (obj, messageType, position) =>
            {
                if (messageType == GameEvents.MessageType.FullInfo || messageType == GameEvents.MessageType.FullWarning)
                    PlaySFX("Error");
                else if (messageType == GameEvents.MessageType.LocalInfo)
                    PlaySFX("Pop");
            };
            GameEvents.instance.OnToggleBoard += (isOpen) => PlaySFX("Card2");
            GameEvents.instance.OnPlayerDodge += (player) => PlaySFX("Miss");
            GameEvents.instance.OnDropCardOnBoard += (card, gridLocation) => PlaySFX("Pop");

            GameEvents.instance.OnGameStart += () => PlayMusic("AmbJungle", loop: true);
            GameEvents.instance.OnGameStart += () => PlayMusic("Normal", loop: true);
            GameEvents.instance.OnLevelStart += () =>
            {
                if (CombatManager.instance.currentLevel.roomType == FloorManager.RoomType.Battle)
                {
                    StopMusicByKey("Normal");
                    PlayMusic("InBattle", loop: true);
                }
            };

            GameEvents.instance.OnLevelCleared += () =>
            {
                if (CombatManager.instance.currentLevel != null && CombatManager.instance.currentLevel.roomType == FloorManager.RoomType.Battle)
                {
                    StopMusicByKey("InBattle");
                    PlayMusic("Normal", loop: true);
                }
            };
        }

        // TODO: Placeholder for now.
        GameEvents.instance.GameStart();
    }

    // Stop music by key (fades out all matching)
    public static void StopMusicByKey(string key, float fadeOut = 0.5f)
    {
        if (string.IsNullOrEmpty(key)) return;
        var toStop = new List<AudioSource>();
        foreach (var src in currentMusicSources)
        {
            if (src != null && src.clip != null && instance != null)
            {
                var named = instance.musicClips.Find(x => x.key == key);
                if (named != null && named.clip == src.clip)
                {
                    toStop.Add(src);
                }
            }
        }
        foreach (var src in toStop)
        {
            StopMusic(src, fadeOut);
        }
    }
    void OnDisable()
    {
        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnHitPawn -= (damage, pawn, instigator, type, loc, hitBack, gun) => PlaySFX("Hit2");
            GameEvents.instance.OnHealPawn -= (heal, pawn, instigator, loc) => PlaySFX(new List<string> { "Heal1", "Heal2" });
            GameEvents.instance.OnUpdateCoins -= (diff) => PlaySFX(new List<string> { "Coin1", "Coin2" });
            GameEvents.instance.OnLevelCleared -= () => { if (CombatManager.instance.currentLevel.clearRequirement != Level.LevelClearRequirement.TimeLimit) PlaySFX("Cleared"); };
            GameEvents.instance.OnHitWall -= (bullet, position, wallObject) => PlaySFX("Punch");
            GameEvents.instance.OnTriggerActionCard -= (card, target) => PlaySFX("Pop");
            GameEvents.instance.OnPawnDie -= (pawn, dmg, inst, type, gun) => PlaySFX(new List<string> { "Death1", "Death2" });
            GameEvents.instance.OnDestroyObject -= (pawn, gun) => PlaySFX(new List<string> { "Death1", "Death2" });
            GameEvents.instance.OnSpawnObject -= (obj) => PlaySFX("Smoke");
            GameEvents.instance.OnShowMessage -= (obj, messageType, position) =>
            {
                if (messageType == GameEvents.MessageType.FullInfo || messageType == GameEvents.MessageType.FullWarning)
                    PlaySFX("Error");
                else if (messageType == GameEvents.MessageType.LocalInfo)
                    PlaySFX("Pop");
            };
            GameEvents.instance.OnToggleBoard -= (isOpen) => PlaySFX("Card2");
            GameEvents.instance.OnPlayerDodge -= (player) => PlaySFX("Miss");
            GameEvents.instance.OnDropCardOnBoard -= (card, gridLocation) => PlaySFX("Pop");
            GameEvents.instance.OnGameStart -= () => PlayMusic("AmbJungle", loop: true);
            GameEvents.instance.OnGameStart -= () => PlayMusic("Normal", loop: true);
            GameEvents.instance.OnLevelStart -= () =>
            {
                if (CombatManager.instance.currentLevel.roomType == FloorManager.RoomType.Battle)
                {
                    StopMusicByKey("Normal");
                    PlayMusic("InBattle", loop: true);
                }
            };
            GameEvents.instance.OnLevelCleared -= () =>
            {
                if (CombatManager.instance.currentLevel != null && CombatManager.instance.currentLevel.roomType == FloorManager.RoomType.Battle)
                {
                    StopMusicByKey("InBattle");
                    PlayMusic("Normal", loop: true);
                }
            };
        }
    }
}
