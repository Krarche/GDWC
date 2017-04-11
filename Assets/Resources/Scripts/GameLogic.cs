using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour {

    public Grid map;
    public static GameLogic main;
    public GameObject playerPrefab;


    // Use this for initialization
    void Start () {
        main = this;
        map = new Grid();
        map.initialisation(15, 15);
        playerPrefab = Resources.Load<GameObject>("Prefabs/PlayerPrefab");
    }

    // Update is called once per frame
    void Update() {

    }

    private void OnDrawGizmos() {
		if (map != null) {
			Gizmos.color = Color.green;
			Vector3 pos = new Vector3 ();
			for (int i = 0; i < map.sizeX; i++) {
				for (int j = 0; j < map.sizeY; j++) {
					pos.x = i;
					pos.z = j;
					Gizmos.DrawSphere (pos, 0.1f);
				}
			}
		}
    }



    public void createPlayer(int cellPositionId)
    {
        Cell cell = map.GetCell(cellPositionId);
        GameObject.Instantiate(playerPrefab, new Vector3(cell.x, 0, cell.y), Quaternion.identity);
    }



}
