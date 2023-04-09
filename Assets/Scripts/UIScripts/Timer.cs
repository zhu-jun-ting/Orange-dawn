using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class Timer : View {

    private static bool isTimerActive = false;

    // In seconds
    public float startTime;
    private float timeLeft;
    
    public TMP_Text timerText;

    // Start is called before the first frame update
    void Start() {
        isTimerActive = true;
        timeLeft = startTime;
    }

    // Update is called once per frame
    public override void Update() {
        if ( isTimerActive ) {
            
            timeLeft -= Time.deltaTime;

            if ( timeLeft < 0 ) { timeLeft = 0; }

            updateTimer( timeLeft );

            if ( timeLeft == 0 ) {
                ( (CoinCounter)FindObjectOfType(typeof(CoinCounter)) ).addCoins( 1 );
                isTimerActive = false;
                // ( (ChunkManager)FindObjectOfType(typeof(ChunkManager)) ).lockPlayer();
                // isTimerActive = false;

            }
            
        }
    }

    public override void Initialize() {}

    public override void Show() {
        base.Show();
        isTimerActive = true;
    }

    public override void Hide() {
        base.Hide();
        isTimerActive = false;
    }

    private void updateTimer( float newTime ) {
        newTime = Mathf.Ceil( newTime );

        float min = Mathf.FloorToInt( newTime / 60 );
        float sec = Mathf.FloorToInt( newTime % 60 );

        timerText.text = string.Format( "{0:00} : {1:00}", min, sec );
    }
}
