using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class DataManager {

    /*
     * Call those lines in the LoadingScreen.Start() function for an example
     * 
        DataParser.buildSpellAndBuffData();
        Debug.Log(DataParser.BUFF_DATA["B001"].description);
        Debug.Log(DataParser.SPELL_DATA["S001"].description);
        Debug.Log(DataParser.SPELL_DATA["S002"].description);
     * 
     */

    public static Dictionary<string, GameData> GAME_DATA = new Dictionary<string, GameData>();
    public static Dictionary<string, Spell> SPELL_DATA = new Dictionary<string, Spell>();
    public static Dictionary<string, Buff> BUFF_DATA = new Dictionary<string, Buff>();
    public static Dictionary<string, CellType> CELL_DATA = new Dictionary<string, CellType>();
    public static Dictionary<string, MapData> MAP_DATA = new Dictionary<string, MapData>();

    public static void registerData(string key, Spell data) {
        SPELL_DATA[key] = data;
        GAME_DATA[key] = data;
    }

    public static void registerData(string key, Buff data) {
        BUFF_DATA[key] = data;
        GAME_DATA[key] = data;
    }

    public static void registerData(string key, CellType data) {
        CELL_DATA[key] = data;
        GAME_DATA[key] = data;
    }

    public static void registerData(string key, MapData data) {
        MAP_DATA[key] = data;
        GAME_DATA[key] = data;
    }

}
