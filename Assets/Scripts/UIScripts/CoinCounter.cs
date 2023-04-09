using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class CoinCounter : View {

    public int startingCoins;
    private int coinCount;
    
    public TMP_Text text;

    // Start is called before the first frame update
    void Start() {
        coinCount = startingCoins;
    }

    public override void Initialize() {}

    public void addCoins( int coinsAdded ) {
        coinCount = coinsAdded + coinCount;
        text.text = coinCount.ToString();
    }
}
