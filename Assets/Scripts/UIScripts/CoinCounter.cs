using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class CoinCounter : View {

    public int startingCoins;
    public int coinCurrent;
    
    public TMP_Text text;

    public static CoinCounter instance;

    void Awake() {
        instance = this;
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

    public bool CanSpendCoins(int amount) {
        return coinCurrent >= amount;
    }
}
