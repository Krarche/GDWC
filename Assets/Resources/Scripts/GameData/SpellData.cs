using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpellData : GameData {
    
    public string iconPath;
    public string description;
    public int cost;
    public int cooldown;
    public int minRange;
    public int maxRange;
    public int rangeType;
    public int priority;
    public EffectSpell[] effects;

    public int areaType {
        get { return effects[0].areaType; }
    }

    public int minArea {
        get { return effects[0].minArea; }
    }

    public int maxArea {
        get { return effects[0].maxArea; }
    }

    public void use(Grid g, Entity user, Cell target) {

    }

    public const int RANGE_AREA_POINT = 0;
    public const int RANGE_AREA_CIRCLE = 1;
    public const int RANGE_AREA_ORTHOGONAL = 2;
    public const int RANGE_AREA_DIAGONAL = 3;

    public static int stringToRangeAreaType(string str) {
        if (str == "circle")
            return RANGE_AREA_CIRCLE;
        if (str == "orthogonal")
            return RANGE_AREA_ORTHOGONAL;
        if (str == "diagonal")
            return RANGE_AREA_DIAGONAL;
        return RANGE_AREA_POINT;
    }
}

public class SpellInstance {
    public Entity owner;
    public SpellData spell;
    public int cooldown;

    public int getCost() {
        return spell.cost;
    }

    public bool isCooldownUp() {
        return cooldown == 0;
    }

    public void use(Grid g, Entity user, Cell target) {
        spell.use(g, user, target);
    }

    public int rangeType {
        get { return spell.rangeType; }
    }

    public int minRange {
        get { return spell.minRange; }
    }

    public int maxRange {
        get { return spell.maxRange; }
    }

    public int areaType {
        get { return spell.areaType; }
    }

    public int minArea {
        get { return spell.minArea; }
    }

    public int maxArea {
        get { return spell.maxArea; }
    }
}