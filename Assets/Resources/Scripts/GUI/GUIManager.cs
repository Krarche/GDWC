using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour {

    public static GUIManager gui;

    //GUI elements to hide/show
    public GameObject rootActions;
    public GameObject quickActions;
    public GameObject movementActions;
    public GameObject slowActions;
    public GameObject currentActive;

    //Texts sync with Player values
    public Text healthText;
    public Text APText;
    public Text MPText;

    //Images to fill
    public Image healthToFill;

    void Start()
    {
        gui = this;
        currentActive = rootActions;
    }

    public void changeActionSelectionState(ActionSelectionState state)
    {
        switch (state)
        {
            case ActionSelectionState.QUICK :
                changeActiveActionSelectionState(rootActions, quickActions);
                break;
             case ActionSelectionState.MOVEMENT :
                changeActiveActionSelectionState(rootActions, movementActions);
                break;
             case ActionSelectionState.SLOW :
                changeActiveActionSelectionState(rootActions, slowActions);
                break;
             case ActionSelectionState.ROOT :
                changeActiveActionSelectionState(currentActive, rootActions);
                break;
        }
    }

    private void changeActiveActionSelectionState(GameObject StatetoDeactivate, GameObject StatetoActivate) {
        StatetoDeactivate.SetActive(false);
        StatetoActivate.SetActive(true);
        currentActive = StatetoActivate;
    }

    public void linkWithLocalEntity(Entity localEntity) {
        localEntity.ChangeEntityStats += player_changeStats;
    }

    private void player_changeStats(object sender, ChangeEntityStatsEventArgs e) {
        Entity localPlayer = (Entity) sender;
        Debug.Log(localPlayer.currentHealth);
        if (e.changeHealth)   changeHealth(localPlayer.currentHealth, localPlayer.maxHealth);
        if (e.changeAP)       APText.text = (localPlayer.currentAP.ToString() + " / " + localPlayer.maxAP.ToString());
        if (e.changeMP)       MPText.text = (localPlayer.currentMP.ToString() + " / " + localPlayer.maxMP.ToString());
    }

    private void changeHealth(int currentHealth, int maxHealth) {
        healthText.text = (currentHealth.ToString() + " / " + maxHealth.ToString());
        healthToFill.fillAmount = (float) currentHealth / maxHealth;
    }
}

public class ChangeEntityStatsEventArgs : System.EventArgs {
    public bool changeHealth = false;
    public bool changeAP = false;
    public bool changeMP = false;
}

