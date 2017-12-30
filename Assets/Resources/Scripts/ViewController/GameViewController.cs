using Logic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameViewController : MonoBehaviour {

    public GameObject rootPanel;

    public GameObject quickActionPanel;
    public GameObject movementActionPanel;
    public GameObject slowActionPanel;
    public Image quickSpell1;
    public Image quickSpell2;
    public Image quickSpell3;
    public Image quickSpell4;
    public Image slowSpell1;
    public Image slowSpell2;
    public Image slowSpell3;
    public Image slowSpell4;

    public Button quickActionButton;
    public Button movementActionButton;
    public Button slowActionButton;

    public Text timer;
    public Text turnCount;

    private GameObject currentPanel;

    // Use this for initialization
    void Start() {
        currentPanel = rootPanel;
    }

    // Update is called once per frame
    void Update() {

    }

    #region EVENT_SYSTEM

    public void SwitchToRoot() {
        if (GameLogicClient.game != null) {
            GameLogicClient.game.buttonInput(ButtonType.ACTION_ROOT);
        }
        currentPanel.SetActive(false);
        currentPanel = rootPanel;
        currentPanel.SetActive(true);
    }

    public void SwitchToQuickAction() {
        if (GameLogicClient.game != null) {
            GameLogicClient.game.buttonInput(ButtonType.ACTION_QUICK_SPELL);
        }
        currentPanel.SetActive(false);
        currentPanel = quickActionPanel;
        currentPanel.SetActive(true);
    }

    public void SwitchToMovementAction() {
        if (GameLogicClient.game != null) {
            GameLogicClient.game.buttonInput(ButtonType.ACTION_MOVEMENT);
        }
        currentPanel.SetActive(false);
        currentPanel = movementActionPanel;
        currentPanel.SetActive(true);
    }

    public void SwitchToSlowAction() {
        if (GameLogicClient.game != null) {
            GameLogicClient.game.buttonInput(ButtonType.ACTION_SLOW_SPELL);
        }
        currentPanel.SetActive(false);
        currentPanel = slowActionPanel;
        currentPanel.SetActive(true);
    }

    public void SelectSpell1() {
        if (GameLogicClient.game != null) {
            GameLogicClient.game.buttonInput(ButtonType.SPELL_0);
        }
    }

    public void SelectSpell2() {
        if (GameLogicClient.game != null) {
            GameLogicClient.game.buttonInput(ButtonType.SPELL_1);
        }
    }

    public void SelectSpell3() {
        if (GameLogicClient.game != null) {
            GameLogicClient.game.buttonInput(ButtonType.SPELL_2);
        }
    }

    public void SelectSpell4() {
        if (GameLogicClient.game != null) {
            GameLogicClient.game.buttonInput(ButtonType.SPELL_3);
        }
    }

    public void Cancel() {
        if (GameLogicClient.game != null) {
            GameLogicClient.game.buttonInput(ButtonType.CANCEL);
        }
    }

    public void Confirm() {
        if (GameLogicClient.game != null) {
            GameLogicClient.game.buttonInput(ButtonType.CONFIRM);
        }
    }

    #endregion

    #region EXTERN_CONTROL

    public void SetTurnCount(int turnNumber) {
        turnCount.text = "Turn " + turnNumber;
    }

    public void SetTimerValue(int seconds, bool hide = false) {
        if (hide) {
            timer.gameObject.SetActive(false);
        } else {
            timer.gameObject.SetActive(true);
            timer.text = seconds + "s";
        }
    }

    public void SetQuickActionButtonEnabled(bool value) {
        quickActionButton.interactable = value;
    }

    public void SetMovementActionButtonEnabled(bool value) {
        movementActionButton.interactable = value;
    }

    public void SetSlowActionButtonEnabled(bool value) {
        slowActionButton.interactable = value;
    }

    public void resetView() { // call at the start of the turn
        SetQuickActionButtonEnabled(true);
        SetMovementActionButtonEnabled(true);
        SetSlowActionButtonEnabled(true);
        SwitchToRoot();
    }

    public void SetHP(int current, int max, int[] ghostModificators) {

    }

    public void SetAP(int current, int max, int[] ghostModificators) {

    }

    public void SetMP(int current, int max, int[] ghostModificators) {

    }

    #endregion

}
