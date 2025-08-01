
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Localization.Settings;
#if UNITY_LOCALIZATION
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
#endif

public class MainMenu : View {
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button languageButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private string startScene;
    public Transform difficultyPanel;

    [Header("Language Display")]
    [SerializeField] private TMP_Text languageLabel;


    // List of supported locales (should match your Unity Localization settings)
    // Use IETF language tags: "en" for English, "zh-Hans" for Simplified Chinese
    private List<string> localeCodes = new List<string> { "en", "zh-Hans" };
    private List<string> localeDisplayNames = new List<string> { "English", "中文" };
    private int currentLanguageIndex = 0;
    private int _localeIndex = 0;

    void Start() { Initialize(); }

    public override void Initialize() {
        startButton.onClick.AddListener( () => SceneManager.LoadScene( startScene ) ); 
        settingsButton.onClick.AddListener( () => difficultyPanel.gameObject.SetActive( !difficultyPanel.gameObject.activeSelf ) );
        quitButton.onClick.AddListener( () => Quit() );
        languageButton.onClick.AddListener( SwitchLanguage );
        // Set currentLanguageIndex based on current locale
        InitLanguageIndexFromLocale();
        UpdateLanguageLabel();
    }

    private void SwitchLanguage() {
        if (localeCodes.Count == 0) return;
        currentLanguageIndex = (currentLanguageIndex + 1) % localeCodes.Count;
#if UNITY_LOCALIZATION
        // Switch locale using Unity Localization
        var locales = LocalizationSettings.AvailableLocales.Locales;
        for (int i = 0; i < locales.Count; i++) {
            if (locales[i].Identifier.Code == localeCodes[currentLanguageIndex]) {
                LocalizationSettings.SelectedLocale = locales[i];
                break;
            }
        }
#endif
        if (GameSettings.instance != null) {
            // If you have a language property in GameSettings, set it here
            // GameSettings.instance.language = localeCodes[currentLanguageIndex];
        }
        _localeIndex = _localeIndex + 1 >= LocalizationSettings.AvailableLocales.Locales.Count ? 0 : _localeIndex + 1;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[_localeIndex];
        UpdateLanguageLabel();
    }

    private void UpdateLanguageLabel() {
        if (languageLabel != null && localeDisplayNames.Count > 0)
            languageLabel.text = $"{localeDisplayNames[currentLanguageIndex]}";
    }

    private void InitLanguageIndexFromLocale() {
#if UNITY_LOCALIZATION
        var selected = LocalizationSettings.SelectedLocale;
        if (selected != null) {
            for (int i = 0; i < localeCodes.Count; i++) {
                if (selected.Identifier.Code == localeCodes[i]) {
                    currentLanguageIndex = i;
                    break;
                }
            }
        }
#endif
    }

    private void Quit() {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit(); 
        #endif
    }
}
