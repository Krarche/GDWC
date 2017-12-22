using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Network;

public class LobbyViewController : MonoBehaviour {

    public static LobbyViewController singleton;


    public Button joinQueue1v1Button;
    public Button cancelQueueButton;

    // Use this for initialization
    void Start () {
        singleton = this;
    }
	
	// Update is called once per frame
	void Update () {
		
	}


    public void OnJoinQueueResponse(bool success) {
        if(success) {
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

    public void ButtonJoinQueue1v1() {
        // disable button
        joinQueue1v1Button.interactable = false;
        // notify network
        NetworkMasterClient.JoinQueue1v1();
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
