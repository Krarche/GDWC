using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Network;

public class LobbyViewController : MonoBehaviour {

    public static LobbyViewController singleton;
    public Lobby lobby;
    public LoadoutViewController loadoutViewController;

    public Text lobbyTitle;

    private GameObject currentPanel;
    public GameObject playPanel;
    public GameObject scoreScreenPanel;

    public Button joinQueue1v1Button;
    public Button cancelQueueButton;


    // Use this for initialization
    void Start () {
        singleton = this;
        currentPanel = playPanel;
        loadoutViewController.SetLoadoutJSON("{\"spells\":[\"S001\",\"S002\",\"S003\",\"S004\"]}");
    }

    public void SwitchToPlayPanel() {
        currentPanel.SetActive(false);
        currentPanel = playPanel;
        lobbyTitle.text = "Play !";
        currentPanel.SetActive(true);
    }

    public void SwitchToScoreScreenPanel() {
        currentPanel.SetActive(false);
        currentPanel = scoreScreenPanel;
        lobbyTitle.text = "Score screen";
        currentPanel.SetActive(true);
    }

    public void ButtonJoinQueue1v1() {
        // check if loadout is valid
        if (!loadoutViewController.isLoadoutValid()) { // if invalid loadout
            string errorMessage = loadoutViewController.GetErrorMessage();
        } else { // can join queue
            // disable button
            joinQueue1v1Button.interactable = false;
            // notify network
            NetworkMasterClient.JoinQueue1v1();
        }
    }

    public void OnJoinQueueResponse(bool success) {
        if (success) {
            // hide queue buttons
            joinQueue1v1Button.gameObject.SetActive(false);
            //show unqueue button
            cancelQueueButton.gameObject.SetActive(true);
        } else {
            // show error
            // enable queue buttons
            joinQueue1v1Button.interactable = true;
        }
    }

    public void ButtonLeaveQueue() {
        // hide button
        cancelQueueButton.gameObject.SetActive(false);
        // show and enable queue buttons
        joinQueue1v1Button.gameObject.SetActive(true);
        joinQueue1v1Button.interactable = true;
        // notify network
        NetworkMasterClient.LeaveQueue();
    }

}
