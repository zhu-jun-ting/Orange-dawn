using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class CoinCounter : View {

    public int _startingCoins = 1000; // Default value, can be overridden in inspector
    public static int startingCoins;
    public static int coinCurrent;
    
    public TMP_Text text;

    public static CoinCounter instance;

    void Awake() {
        instance = this;
        startingCoins = this._startingCoins; // Set the static starting coins value
    }

    // Start is called before the first frame update
    void Start() {
        coinCurrent = startingCoins;
        text.text = coinCurrent.ToString();

        GameEvents.instance.OnUpdateCoins += AddCoin;
    }

    void OnDisable() {
        GameEvents.instance.OnUpdateCoins -= AddCoin;
    }

    public override void Initialize() {}

    public void AddCoin(int diffCoin) {
        coinCurrent = diffCoin + coinCurrent;
        text.text = coinCurrent.ToString();
    }

    public static bool CanCostCoin(int diffCoin) {
        return coinCurrent + diffCoin >= 0;
    }
}
