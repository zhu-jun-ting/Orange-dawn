using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class DifficultyMenu : View {
    [SerializeField] private Button easyButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button hardButton;

    [Header("Optional: Panel to hide after selection")]
    public Transform difficultyPanel;

    [Header("Difficulty Info Text")]
    [SerializeField] private TMP_Text difficultyInfoText;
    
    public void Start() {
        Initialize();
    }

    public override void Initialize()
    {
        easyButton.onClick.AddListener(() => SetDifficulty(1f, 1f, "Easy"));
        normalButton.onClick.AddListener(() => SetDifficulty(1.5f, 1.5f, "Normal"));
        hardButton.onClick.AddListener(() => SetDifficulty(2f, 2f, "Hard"));
    }

    private void SetDifficulty(float healthMod, float damageMod, string difficultyName) {
        if (GameSettings.instance != null) {
            GameSettings.instance.enemyHealthModifier = healthMod;
            GameSettings.instance.enemyDamageModifier = damageMod;
        }
        if (difficultyInfoText != null) {
            difficultyInfoText.text = $"{difficultyName}";
        }
        if (difficultyPanel != null) {
            difficultyPanel.gameObject.SetActive(false);
        }
        // Optionally, you can add feedback or transition here
    }
}
