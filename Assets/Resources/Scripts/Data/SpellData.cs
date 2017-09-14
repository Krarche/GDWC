using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Data {

    public class SpellData : GameData {

        public string iconPath;
        public string[] description = { "", "" };
        public string[] color = { "", "" };
        public int[] cost = { 0, 0 };
        public int[] cooldown = { 0, 0 };
        public int[] minRange = { 0, 0 };
        public int[] maxRange = { 0, 0 };
        public int[] rangeType = { 0, 0 };
        public int[] priority = { 0, 0 };
        public EffectSpell[] effects;

        public int[] getAreaType(int priority) {
            int[] output = new int[effects.Length];
            for (int i = 0; i < effects.Length; i++) {
                output[i] = effects[i].areaType[priority];
            }
            return output;
        }

        public int[] getMinArea(int priority) {
            int[] output = new int[effects.Length];
            for (int i = 0; i < effects.Length; i++) {
                output[i] = effects[i].minArea[priority];
            }
            return output;
        }

        public int[] getMaxArea(int priority) {
            int[] output = new int[effects.Length];
            for (int i = 0; i < effects.Length; i++) {
                output[i] = effects[i].maxArea[priority];
            }
            return output;
        }

        public const int RANGE_AREA_POINT = 0;
        public const int RANGE_AREA_CIRCLE = 1;
        public const int RANGE_AREA_ORTHOGONAL = 2;
        public const int RANGE_AREA_DIAGONAL = 3;
        public const int RANGE_AREA_LINE_PARALLEL = 4; // for area only
        public const int RANGE_AREA_LINE_PERPENDICULAR = 5; // for area only

        public static int[] stringToRangeAreaType(string[] str) {
            int[] output = new int[str.Length];
            for (int i = 0; i < str.Length; i++) {
                output[i] = stringToRangeAreaType(str[i]);
            }
            return output;
        }

        public static int stringToRangeAreaType(string str) {
            if (str == "circle")
                return RANGE_AREA_CIRCLE;
            if (str == "orthogonal")
                return RANGE_AREA_ORTHOGONAL;
            if (str == "diagonal")
                return RANGE_AREA_DIAGONAL;
            if (str == "parallele")
                return RANGE_AREA_LINE_PARALLEL;
            if (str == "perpendicular")
                return RANGE_AREA_LINE_PERPENDICULAR;
            return RANGE_AREA_POINT;
        }
    }

    public class SpellInstance {
        public Entity owner;
        public SpellData spell;
        public int cooldown;

        public int getCost(int priority) {
            return spell.cost[priority];
        }

        public bool isCooldownUp() {
            return cooldown == 0;
        }

        public int getRangeType(int priority) {
            return spell.rangeType[priority];
        }

        public int getMinRange(int priority) {
            return spell.minRange[priority];
        }

        public int getMaxRange(int priority) {
            return spell.maxRange[priority];
        }

        public int[] getAreaType(int priority) {
            return spell.getAreaType(priority);
        }

        public int[] getMinArea(int priority) {
            return spell.getMinArea(priority);
        }

        public int[] getMaxArea(int priority) {
            return spell.getMaxArea(priority);
        }
    }
}