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

    private GameObject currentPanel;

    // Use this for initialization
    void Start() {
        currentPanel = rootPanel;
    }

    // Update is called once per frame
    void Update() {

    }

    public void SwitchToRoot() {
        currentPanel.SetActive(false);
        currentPanel = rootPanel;
        currentPanel.SetActive(true);
    }

    public void SwitchToQuickAction() {
        currentPanel.SetActive(false);
        currentPanel = quickActionPanel;
        currentPanel.SetActive(true);
    }

    public void SwitchToMovementAction() {
        currentPanel.SetActive(false);
        currentPanel = movementActionPanel;
        currentPanel.SetActive(true);
    }

    public void SwitchToSlowAction() {
        currentPanel.SetActive(false);
        currentPanel = slowActionPanel;
        currentPanel.SetActive(true);
    }

    public void SelectSpell1() {

    }

    public void SelectSpell2() {

    }

    public void SelectSpell3() {

    }

    public void SelectSpell4() {

    }

    public void Cancel() {

    }

    public void Confirm() {

    }

}
