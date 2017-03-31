using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid {

	public TextAsset source;
    public int sizeX, sizeY;
    public Cell[,] cells;
    public Dictionary<GameObject, Cell> worldObjects;
    public GameObject cellBase;

    public void initialisation(int x, int y) {
        cellBase = Resources.Load<GameObject>("Prefabs/CellBase");
        worldObjects = new Dictionary<GameObject, Cell>();
        this.sizeX = x;
        this.sizeY = y;
        initPosition();
        initDistance();
    }

    private void initPosition() {
        cells = new Cell[sizeX, sizeY];
        for (int y = 0; y < sizeY; y++) {
            for (int x = 0; x < sizeX; x++) {
                GameObject o = GameObject.Instantiate(cellBase, new Vector3(x, 0, y), Quaternion.identity);
                Cell c = new Cell(sizeX, sizeY, o);
                cells[x, y] = c;
                c.inWorld = o;
                worldObjects.Add(o, c);
            }
        }

        for (int y = 0; y < sizeY; y++) {
            for (int x = 0; x < sizeX; x++) {
                Cell c = cells[x, y];
                c.x = x;
                c.y = y;
                c.id = x + sizeX * y;
                if (c.y + 1 < sizeY) {
                    c.adjacent[(int)Direction.NORTH] = cells[x, y + 1];
                }
                if (c.y - 1  > 0) {
                    c.adjacent[(int)Direction.SOUTH] = cells[x, y - 1];
                }
                if (c.x + 1 < sizeX) {
                    c.adjacent[(int)Direction.EAST] = cells[x + 1, y];
                }
                if (c.x - 1 > 0) {
                    c.adjacent[(int)Direction.WEST] = cells[x-1, y];
                }
            }
        }
    }

    private void initDistance() {
        for (int y = 0; y < sizeY; y++)
            for (int x = 0; x < sizeX; x++)
                setDistanceForPosition(cells[x, y], x, y, 0);
    }

    private void setDistanceForPosition(Cell c, int x, int y, int distance) {
        if (c.distance[x, y] == -1 || distance < c.distance[x, y]) {
            c.distance[x, y] = distance;
            for (int a = 0; a < (int)Direction.COUNT; a++) {
                if (c.adjacent[a] != null) {
                    setDistanceForPosition(c.adjacent[a], x, y, distance + 1);
                }
            }
        }
    }

    public void printGrid() {
        for (int y = 0; y < sizeY; y++) {
            for (int x = 0; x < sizeX; x++) {
                Debug.Log("  " + x + ", " + y + "  ");
            }
            Debug.Log("\n");
        }
    }

    public void generateGrid() {
        Mesh tempMesh = new Mesh();
        Material tempMat = (Material)Resources.Load("Materials/defaultMat", typeof(Material));
        for (int x = 0; x < this.sizeX; x++) {
            for (int y = 0; y < this.sizeY; y++) {
                string tempName = x + "-" + y;
                GameObject tempObject = new GameObject(tempName);
                tempObject.AddComponent<MeshFilter>().mesh = tempMesh;
                
                tempObject.transform.position.Set(x, y, 0f);

            }
        }
    }

    public List<Cell> getInRange(Cell origin, int minRange, int maxRange) {

        List<Cell> output = new List<Cell>();
        for (int y = 0; y < sizeY; y++) {
            for (int x = 0; x < sizeX; x++) {
                if (origin.distance[x,y] >= minRange && origin.distance[x, y] <= maxRange) {
                    output.Add(cells[x, y]);
                }
            }
        }

        return output;
    }

    public List<Cell> getInLine(Cell origin, int minRange, int maxRange) {

        List<Cell> output = new List<Cell>();
        for (int dir = 0; dir < (int)Direction.COUNT; dir++) {
            Cell current = origin;
            int i = 0;
            while(i <= maxRange && current != null) {
                if (i >= minRange)
                    output.Add(current);
                current = current.adjacent[dir];
            }
        }

        return output;
    }
}