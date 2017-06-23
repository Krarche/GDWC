using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSpell {
    public int minAera;
    public int maxAera;
    public int areaType;

    public bool affectAlly = true;
    public bool affectEnemy = true;
    public bool affectSelf = true;
    public bool affectCell = true;

    public EffectHandler effectHandler;

    public void apply(Entity e) {
        if (effectHandler != null) {
            effectHandler.effect(e);
        }
    }
    public void apply(Cell c) {
        if (effectHandler != null) {
            effectHandler.effect(c);
        }
    }
}
