using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterViewController : MonoBehaviour {

    public Data.Entity entity;
    public Image HPFill;
    public Text HPValue;
    public Text APValue;
    public Text MPValue;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    void UpdateStats(Data.Entity e) {
        UpateHP(e.currentHealth, e.maxHealth);
        UpateAP(e.currentAP, e.maxAP);
        UpateMP(e.currentMP, e.maxMP);
    }

    void UpateHP(int current, int max) {
        HPFill.fillAmount = (float)current / (float)max;
        HPValue.text = "" + current + "/" + max;
    }

    void UpateAP(int current, int max) {

        APValue.text = "" + current;
    }

    void UpateMP(int current, int max) {

        MPValue.text = "" + current;
    }
}
