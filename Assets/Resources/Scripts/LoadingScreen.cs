using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour {

    public Text progressionText;

    // Update is called once per frame
    void Update() {
        progressionText.text = (int)(SceneMaster.asyncProgress * 100) + " %";
    }
}
