using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapData : GameData {
    public int width, height;
    public CellData[] cells;
    public int[] spawns;
    public HashSet<string> cellDataIdList;


    public void buildCellDataIdList() {
        foreach (CellData c in cells)
            cellDataIdList.Add(c.id);
    }
}