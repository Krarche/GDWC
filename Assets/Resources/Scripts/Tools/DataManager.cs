using System.Collections.Generic;
using Data;

namespace Tools {

    public class DataManager {

        static DataManager() {
            DataParser.loadDataJSON();
        }

        public static Dictionary<string, GameData> GAME_DATA = new Dictionary<string, GameData>();
        public static Dictionary<string, SpellData> SPELL_DATA = new Dictionary<string, SpellData>();
        public static Dictionary<string, BuffData> BUFF_DATA = new Dictionary<string, BuffData>();
        public static Dictionary<string, CellData> CELL_DATA = new Dictionary<string, CellData>();
        public static Dictionary<string, MapData> MAP_DATA = new Dictionary<string, MapData>();

        public static void registerData(string key, SpellData data) {
            SPELL_DATA[key] = data;
            GAME_DATA[key] = data;
        }

        public static void registerData(string key, BuffData data) {
            BUFF_DATA[key] = data;
            GAME_DATA[key] = data;
        }

        public static void registerData(string key, CellData data) {
            CELL_DATA[key] = data;
            GAME_DATA[key] = data;
        }

        public static void registerData(string key, MapData data) {
            MAP_DATA[key] = data;
            GAME_DATA[key] = data;
        }
    }
}