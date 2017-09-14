using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Logic;

public class ButtonHandler : MonoBehaviour {

    public bool useAlpha = false;
    public float alphaThreshold = 0.1f;
    public short type = 0;

    // Use this for initialization
    void Awake() {
        if (useAlpha) {
            Button btn = GetComponent<Button>();
            btn.onClick.AddListener(Handler);
            this.GetComponent<Image>().alphaHitTestMinimumThreshold = alphaThreshold;
        }
        Button button = GetComponent<Button>();
        button.onClick.AddListener(Handler);
    }

    // Update is called once per frame
    void Update() {

    }

    void Handler() {
        if(GameLogicClient.game != null) {
            GameLogicClient.game.buttonInput(type);
        }
    }
}