using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

using DG.Tweening;
using UnityEngine.SceneManagement;

public class StatisticPanel : MonoBehaviour
{
    [Header("Layout Groups")]
    [SerializeField] private VerticalLayoutGroup titleLayout;
    [SerializeField] private VerticalLayoutGroup valueLayout;
    [Header("Entry Prefab")]
    [SerializeField] private GameObject statisticEntryPrefab; // Should have a TMP_Text component

    [Header("Victory/Defeat Groups")]
    [SerializeField] private List<Transform> victoryGroup = new List<Transform>();
    [SerializeField] private List<Transform> defeatGroup = new List<Transform>();

    [Header("Panel Animation")]
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private float flyDistance = 500f;
    [SerializeField] private float flyDuration = 0.5f;

    [Header("Buttons")]
    [SerializeField] private UnityEngine.UI.Button quitButton;
    [SerializeField] private UnityEngine.UI.Button endlessButton;

    private readonly string[] statTitleKeys = new string[] {
        "Stat_Coins",
        "Stat_TakenDamage",
        "Stat_DealtDamage",
        "Stat_Healed",
        "Stat_DiscardedCards",
        "Stat_AcquiredCards",
        "Stat_CardsTriggered",
        "Stat_EnemiesKilled",
        "Stat_ObjectsDestroyed",
        "Stat_LevelCleared",
        "Stat_Level"
    };
    private string[] statTitles;

    private readonly System.Func<int>[] statGetters = new System.Func<int>[] {
        () => GameEvents.totalCoins,
        () => GameEvents.totalTakenDamage,
        () => GameEvents.totalDealtDamage,
        () => GameEvents.totalHealed,
        () => GameEvents.totalDiscardedCards,
        () => GameEvents.totalAcquiredCards,
        () => GameEvents.totalCardsTriggered,
        () => GameEvents.totalEnemiesKilled,
        () => GameEvents.totalObjectsDestroyed,
        () => GameEvents.totalLevelCleared,
        () => GameEvents.totalLevel
    };

    private List<TMP_Text> valueTexts = new List<TMP_Text>();
    private bool isPanelActive = false;
    private bool isVictory = false;
    private Vector2 panelStartPos;

    void Awake()
    {
        if (panelRect == null) panelRect = GetComponent<RectTransform>();
        panelStartPos = panelRect.anchoredPosition;
        // Localize stat titles at runtime
        statTitles = new string[statTitleKeys.Length];
        for (int i = 0; i < statTitleKeys.Length; i++)
        {
            statTitles[i] = GameSettings.LocalizeText(statTitleKeys[i]);
        }
    }

    void Start()
    {
        SetupPanel();
        UpdateValues();
        GameEvents.instance.OnGameEnd += OnGameEnd;
        quitButton.onClick.AddListener(OnQuitClicked);
        endlessButton.onClick.AddListener(OnEndlessClicked);
        HidePanelImmediate();
    }

    void OnDestroy()
    {
        GameEvents.instance.OnGameEnd -= OnGameEnd;
        quitButton.onClick.RemoveListener(OnQuitClicked);
        endlessButton.onClick.RemoveListener(OnEndlessClicked);
        HidePanelImmediate();
    }

    public void SetupPanel()
    {
        // Clear old children
        foreach (Transform child in titleLayout.transform) Destroy(child.gameObject);
        foreach (Transform child in valueLayout.transform) Destroy(child.gameObject);
        valueTexts.Clear();
        for (int i = 0; i < statTitles.Length; i++)
        {
            // Title entry
            var titleObj = Instantiate(statisticEntryPrefab, titleLayout.transform);
            var titleText = titleObj.GetComponentInChildren<TMP_Text>();
            if (titleText != null) titleText.text = statTitles[i];
            // Value entry
            var valueObj = Instantiate(statisticEntryPrefab, valueLayout.transform);
            var valueText = valueObj.GetComponentInChildren<TMP_Text>(true);
            if (valueText != null) valueTexts.Add(valueText);
        }
    }

    public void UpdateValues()
    {
        for (int i = 0; i < valueTexts.Count && i < statGetters.Length; i++)
        {
            valueTexts[i].text = statGetters[i]().ToString();
        }
    }

    private void OnGameEnd(bool victory)
    {
        isVictory = victory;
        UpdateValues();
        ShowVictoryDefeatGroup(victory);
        ShowPanel();
    }

    private void ShowVictoryDefeatGroup(bool victory)
    {
        // Hide all first
        foreach (var t in victoryGroup) if (t != null) t.gameObject.SetActive(false);
        foreach (var t in defeatGroup) if (t != null) t.gameObject.SetActive(false);

        // Show only the relevant group
        if (victory)
        {
            foreach (var t in victoryGroup) if (t != null) t.gameObject.SetActive(true);
        }
        else
        {
            foreach (var t in defeatGroup) if (t != null) t.gameObject.SetActive(true);
        }
    }

    private void ShowPanel()
    {
        if (isPanelActive) return;
        isPanelActive = true;
        gameObject.SetActive(true);
        // Move panel up (outside) instantly, then fly in
        panelRect.anchoredPosition = panelStartPos + new Vector2(0, flyDistance);
        panelRect.DOAnchorPos(panelStartPos, flyDuration).SetEase(Ease.OutBack).OnComplete(PauseGame);
    }

    private void HidePanelImmediate()
    {
        isPanelActive = false;
        gameObject.SetActive(false);
        if (panelRect != null)
            panelRect.anchoredPosition = panelStartPos + new Vector2(0, flyDistance);
    }

    private void HidePanelAnimated()
    {
        if (!isPanelActive) return;
        isPanelActive = false;
        ResumeGame();
        panelRect.DOAnchorPos(panelStartPos + new Vector2(0, flyDistance), flyDuration).SetEase(Ease.InBack).OnComplete(() => {
            gameObject.SetActive(false);
        });
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    private void OnQuitClicked()
    {
        ResumeGame();
        SceneManager.LoadScene("MainMenu");
        if (GameEvents.instance != null)
            GameEvents.instance.GameReset(); // Reset game state if needed
    }

    private void OnEndlessClicked()
    {
        HidePanelAnimated();
        GameSettings.instance.isEndlessMode = true; // Set endless mode flag
    }
}
