using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectBuff {
    public int minAera;
    public int maxAera;
    public int aeraType;

    public bool affectAlly = true;
    public bool affectEnemy = true;
    public bool affectSelf = true;
    public bool affectCell = true;

    public int buffMaxTriggerTime;

    // entity
    EffectHandler onGainedHandler; // when the entity gained the buff
    EffectHandler onLostHandler; // when the entity lost the buff
    EffectHandler onMoveHandler; // when the entity starts a movement
    EffectHandler onHealHandler; // when the entity received heal
    EffectHandler onSpellHandler; // each time the entity uses an abality
    EffectHandler onDamageHandler; // when the entity received damage
    EffectHandler onBuffedHandler; // when the entity received a new buff

    // cell
    EffectHandler onEnterHandler; // when an entity enter the cell
    EffectHandler onLeaveHandler; // when an entity leave the cell

    // common
    EffectHandler onTurnStartHandler; // when a new turn starts
    EffectHandler onTurnEndHandler; // when a turn ends


    public void onGained(Entity e) {
        if (onGainedHandler != null) {
            EffectHandler temp = onBuffedHandler;
            onGainedHandler = null;
            temp.effect(e);
            onGainedHandler = temp;
        }
    }
    public void onLost(Entity e) {
        if (onLostHandler != null) {
            EffectHandler temp = onLostHandler;
            onLostHandler = null;
            temp.effect(e);
            onLostHandler = temp;
        }
    }
    public void onMove(Entity e) {
        if (onMoveHandler != null) {
            EffectHandler temp = onMoveHandler;
            onMoveHandler = null;
            temp.effect(e);
            onMoveHandler = temp;
        }
    }
    public void onHeal(Entity e) {
        if (onHealHandler != null) {
            EffectHandler temp = onHealHandler;
            onHealHandler = null;
            temp.effect(e);
            onHealHandler = temp;
        }
    }
    public void onSpell(Entity e) {
        if (onSpellHandler != null) {
            EffectHandler temp = onSpellHandler;
            onSpellHandler = null;
            temp.effect(e);
            onSpellHandler = temp;
        }
    }
    public void onDamage(Entity e) {
        if (onDamageHandler != null) {
            EffectHandler temp = onDamageHandler;
            onDamageHandler = null;
            temp.effect(e);
            onDamageHandler = temp;
        }
    }
    public void onBuffed(Entity e) {
        if (onBuffedHandler != null) {
            EffectHandler temp = onBuffedHandler;
            onBuffedHandler = null;
            temp.effect(e);
            onBuffedHandler = temp;
        }
    }
    public void onTurnStart(Entity e) {
        if (onTurnStartHandler != null) {
            EffectHandler temp = onTurnStartHandler;
            onTurnStartHandler = null;
            temp.effect(e);
            onTurnStartHandler = temp;
        }
    }
    public void onTurnEnd(Entity e) {
        if (onTurnEndHandler != null) {
            EffectHandler temp = onTurnEndHandler;
            onTurnEndHandler = null;
            temp.effect(e);
            onTurnEndHandler = temp;
        }
    }
    public void onTurnStart(Cell c) {
        if (onTurnStartHandler != null) {
            EffectHandler temp = onTurnStartHandler;
            onTurnStartHandler = null;
            temp.effect(c);
            onTurnStartHandler = temp;
        }
    }
    public void onTurnEnd(Cell c) {
        if (onTurnEndHandler != null) {
            EffectHandler temp = onTurnEndHandler;
            onTurnEndHandler = null;
            temp.effect(c);
            onTurnEndHandler = temp;
        }
    }
    public void onEnter(Cell c) {
        if (onTurnStartHandler != null) {
            EffectHandler temp = onTurnStartHandler;
            onTurnStartHandler = null;
            temp.effect(c);
            onTurnStartHandler = temp;
        }
    }
    public void onLeave(Cell c) {
        if (onTurnStartHandler != null) {
            EffectHandler temp = onTurnStartHandler;
            onTurnStartHandler = null;
            temp.effect(c);
            onTurnStartHandler = temp;
        }
    }

}