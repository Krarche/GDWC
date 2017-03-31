using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction : int {
    NORTH, EAST, SOUTH, WEST, COUNT
}

public class Cell {

	public CellType type;
	public int x, y, id;
    public Cell[] adjacent = { null, null, null, null };
    public int[,] distance;

    public Cell(int x, int y) {
        distance = new int[x, y];
        for(int i=0; i<x; i++) {
            for(int j=0; j<y; j++) {
                distance[i, j] = -1;
            }
        }
        type = new CellType(Type.NORMAL);
    }
}