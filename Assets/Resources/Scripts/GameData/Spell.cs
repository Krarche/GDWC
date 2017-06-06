using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell {

    public string name;
    public int cost;
    public int cooldown;
    public int minRange;
    public int maxRange;
    public int rangeType;
    public int minAera;
    public int maxAera;
    public int aeraType;
    public string iconPath;
    public bool priority;

    public void onAlly(Entity target) {

    }

    public void onEnnemy(Entity target) {

    }

    public void onCell(Cell target) {

    }

    public void onSelf() {

    }

    public void onAll(Entity target) {

    }

    public void onTarget(Entity target) {

    }

    public void onAera(Cell target) {

    }

    public void use(Grid g, Entity user, Cell target) {

    }
}

public class SpellInstance {

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