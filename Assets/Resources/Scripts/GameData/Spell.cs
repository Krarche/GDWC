using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Spell : GameData {
    
    public string iconPath;
    public string description;
    public int cost;
    public int cooldown;
    public int minRange;
    public int maxRange;
    public int rangeType;
    public int priority;
    public EffectSpell[] effects;

    public void onAlly(Entity target) {
        foreach (EffectSpell e in effects) {
            if (e.affectAlly) {
                e.apply(target);
            }
        }
    }

    public void onEnnemy(Entity target) {
        foreach (EffectSpell e in effects) {
            if (e.affectEnemy) {
                e.apply(target);
            }
        }
    }

    public void onSelf(Entity target) {
        foreach (EffectSpell e in effects) {
            if (e.affectSelf) {
                e.apply(target);
            }
        }
    }

    public void onCell(Cell target) {
        foreach (EffectSpell e in effects) {
            if (e.affectCell) {
                e.apply(target);
            }
        }
    }

    public void use(Grid g, Entity user, Cell target) {

    }

    public static int RANGE_AREA_POINT = 0;
    public static int RANGE_AREA_CIRCLE = 1;
    public static int RANGE_AREA_ORTHOGONAL = 2;
    public static int RANGE_AREA_DIAGONAL = 3;

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
    public Spell spell;
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
}