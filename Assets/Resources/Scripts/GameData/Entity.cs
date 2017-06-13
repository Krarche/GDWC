﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour {

    public int destinationCellId = NO_DESTINATION_CELL_ID;
    public int currentCellId = NO_DESTINATION_CELL_ID;
    public static int NO_DESTINATION_CELL_ID = -1;

    public int entityId;
    public Queue<Order> orders = new Queue<Order>();

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if(GameLogicClient.game == null) {
            if (destinationCellId != NO_DESTINATION_CELL_ID) {
                Cell destCell = GameLogicClient.game.map.GetCell(destinationCellId);
                Vector3 pos = gameObject.transform.position;
                Vector3 dest = new Vector3(destCell.x, 0, destCell.y);
                Vector3 dir = dest - pos;
                if (dir.magnitude > 0.1f) {
                    gameObject.transform.position = pos + dir / 2;
                } else {
                    currentCellId = destinationCellId;
                    destinationCellId = NO_DESTINATION_CELL_ID;
                    gameObject.transform.position = dest;
                }
            }
        }
    }

    public void setCurrentCell(Cell c) {
        transform.position = c.position;
        currentCellId = c.id;
    }

    public void orderMoveToCell(int destinationCellId) {
        this.destinationCellId = destinationCellId;
    }

    public int getCurrentCell() {
        if (destinationCellId != NO_DESTINATION_CELL_ID)
            return destinationCellId;
        return currentCellId;
    }

    public void addOrder(Order o) {
        orders.Enqueue(o);
    }
}
