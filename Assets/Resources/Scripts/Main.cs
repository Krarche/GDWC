﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {

    public Grid map;
    
	// Use this for initialization
	void Start () {
        main = this;
        map = new Grid();
        map.initialisation(7, 7);
        map.printGrid();
    }
	
	// Update is called once per frame
	void Update () {

	}
    
    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Vector3 pos = new Vector3();
        for (int i = 0; i < map.sizeX; i++) {
            for (int j = 0; j < map.sizeY; j++) {
                pos.x = i;
                pos.z = j;
                Gizmos.DrawSphere(pos, 0.1f);
            }
        }
    }
}
