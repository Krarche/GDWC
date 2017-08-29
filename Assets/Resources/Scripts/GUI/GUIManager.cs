using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour {

    public static GUIManager gui;

    //Texts sync with Player values
    public Text healthText;
    public Text APText;
    public Text MPText;

    void Start()
    {
        gui = this;
    }

    public void linkWithLocalEntity(Entity localEntity) {
        localEntity.ChangeEntityStats += player_changeStats;
    }

    private void player_changeStats(object sender, ChangeEntityStatsEventArgs e) {
        Entity localPlayer = (Entity) sender;
        if (e.changeHealth)   healthText.text = (localPlayer.currentHealth.ToString() + " / " + localPlayer.maxHealth.ToString());
        if (e.changeAP)       APText.text = (localPlayer.currentAP.ToString() + " / " + localPlayer.maxAP.ToString());
        if (e.changeMP)       MPText.text = (localPlayer.currentMP.ToString() + " / " + localPlayer.maxMP.ToString());
    }

}

public class ChangeEntityStatsEventArgs : System.EventArgs {
    public bool changeHealth = false;
    public bool changeAP = false;
    public bool changeMP = false;
}



