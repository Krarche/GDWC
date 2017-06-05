using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour {

    public short type = 0;

	// Use this for initialization
	void Start () {
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(Handler);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void Handler() {
        GameLogicClient.game.buttonInput(type);
    }
}
