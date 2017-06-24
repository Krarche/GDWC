using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour {

    public float alphaThreshold = 0.1f;
    public short type = 0;

    // Use this for initialization
    void Start() {
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(Handler);
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = alphaThreshold;
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