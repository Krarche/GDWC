using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSpell {
    public int minAera;
    public int maxAera;
    public int aeraType;

    public bool affectAlly = true;
    public bool affectEnemy = true;
    public bool affectSelf = true;
    public bool affectCell = true;

    public EffectHandler effect;

    public void apply(Entity e) {
        if (effect != null) {
            effect.effect(e);
        }
    }
    public void apply(Cell c) {
        if (effect != null) {
            effect.effect(c);
        }
    }
}
