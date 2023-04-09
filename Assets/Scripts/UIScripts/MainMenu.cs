using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.SceneManagement;

public class MainMenu : View {

    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private string startScene;

    void Start() { Initialize(); }

    public override void Initialize() {
        startButton.onClick.AddListener( () => SceneManager.LoadScene( startScene ) ); 
        quitButton.onClick.AddListener( () => Quit() );
    }

    private void Quit() {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit(); 
        #endif
    }
}
