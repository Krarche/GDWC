using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell {
    List<EffectSpell> effectList;

    public string name;
    public int cost;
    public int cooldown;
    public int minRange;
    public int maxRange;
    public int rangeType;
    public string iconPath;
    public bool priority;

    public void onAlly(Entity target) {
        foreach (EffectSpell e in effectList) {
            if (e.affectAlly) {
                e.apply(target);
            }
        }
    }

    public void onEnnemy(Entity target) {
        foreach (EffectSpell e in effectList) {
            if (e.affectEnemy) {
                e.apply(target);
            }
        }
    }

    public void onSelf(Entity target) {
        foreach (EffectSpell e in effectList) {
            if (e.affectSelf) {
                e.apply(target);
            }
        }
    }

    public void onCell(Cell target) {
        foreach (EffectSpell e in effectList) {
            if (e.affectCell) {
                e.apply(target);
            }
        }
    }

    public void use(Grid g, Entity user, Cell target) {

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