using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction : int {
    NORTH, EAST, SOUTH, WEST, COUNT
}

public class Cell {

    public CellData data;
    public int x, y, cellId;
    public Cell[] adjacent = { null, null, null, null };
    public int[,] distance;
    public Vector3 position { get { return new Vector3(x, 0, y); } }
    public Grid grid;
    public GameLogic game;

    public GameObject inWorld;

    private Stack<Color> colorModifiers = new Stack<Color>();

    public Cell(int x, int y, GameObject o, CellData data = null) {
        distance = new int[x, y];
        for (int i = 0; i < x; i++) {
            for (int j = 0; j < y; j++) {
                distance[i, j] = -1;
            }
        }
        this.data = data != null ? data : new CellData();
        inWorld = o;
    }

    public bool blockLineOfSight() {
        return data.blockLineOfSight;
    }

    public bool blockMovement() {
        return data.blockMovement;
    }

    public void addColor(Color c) {
        colorModifiers.Push(c);
        inWorld.GetComponent<MeshRenderer>().material.color = c;
    }

    public void removeColor() {
        colorModifiers.Pop();
        if (colorModifiers.Count > 0)
            inWorld.GetComponent<MeshRenderer>().material.color = colorModifiers.Peek();
        else
            inWorld.GetComponent<MeshRenderer>().material.color = Color.white;
    }

}