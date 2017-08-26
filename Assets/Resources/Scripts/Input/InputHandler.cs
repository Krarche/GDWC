using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonUp(0)) {
            click(Input.mousePosition);
        }

        if (Input.touchCount == 1) {
            click(Input.touches[0].position);
        }
    }

    public void click(Vector2 pos) {
        Ray ray = Camera.main.ScreenPointToRay(pos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) {
            Debug.Log("raycast hit!!");
            clickCell(hit.transform.gameObject);
        }
    }

    public void clickCell(GameObject cellObject) {
        GameLogicClient.game.targetAction(GameLogicClient.game.grid.worldObjects[cellObject]);
    }
}
