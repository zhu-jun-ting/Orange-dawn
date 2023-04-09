using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.SceneManagement;

public class PauseMenu : View {

    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    public static bool gameIsPaused = false;

    public override void Update() { if ( Input.GetKeyDown( KeyCode.Escape ) ) { TogglePause(); } }

    private void TogglePause() {
        if ( gameIsPaused ) { CanvasManager.Hide<PauseMenu>(); }
        else { CanvasManager.Show<PauseMenu>(); }
    }

    public override void Initialize() { 
        resumeButton.onClick.AddListener( () => CanvasManager.Hide<PauseMenu>() ); 
        quitButton.onClick.AddListener( () => Quit() );
    }

    private void Quit() {
        CanvasManager.Hide<PauseMenu>();
        SceneManager.LoadScene( "MainMenu" );
    }

    public override void Show() {
        base.Show();
        Time.timeScale = 0f;
        gameIsPaused = true;

        GameObject.FindWithTag("Player").transform.Find("SwordHolder").gameObject.GetComponent<Attack>().enabled = false;
    }

    public override void Hide() {
        base.Hide();
        Time.timeScale = 1f;
        gameIsPaused = false;

        GameObject.FindWithTag("Player").transform.Find("SwordHolder").gameObject.GetComponent<Attack>().enabled = true;
    }
}
