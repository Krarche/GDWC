using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnResolution {

    GameLogic parent;

    public TurnResolution(GameLogic parent) {
        this.parent = parent;
    }

    void resolveAction() {

    }

    // this function is called if the spell can be used (line of sight, cost, cooldown, etc must already be verified)
    public void resolveMovement(Entity origin, int[] path) {
        int destinationCellId = path[path.Length - 1];
        origin.orderMoveToCell(destinationCellId);
        // TODO : move along path
    }

    // this function is called if the spell can be used (line of sight, cost, cooldown, etc must already be verified)
    public void resolveSpell(SpellData spell, Entity origin, Cell target) {
        foreach (EffectSpell effect in spell.effects) {
            resolveSpellEffect(effect, origin, target, true);
        }
    }

    // this function will check the conditions for each affected entity, and apply if the effect if necessary
    public void resolveSpellEffect(EffectSpell effect, Entity origin, Cell target, bool priority) {
        foreach (Entity e in getSpellEffectTargets(effect, origin, target)) {
            if (areConditionsValid(effect.conditions, origin, e)) {
                if (priority) {
                    handleEffect(effect.quickHandler, origin, e);
                } else {
                    handleEffect(effect.slowHandler, origin, e);
                }
            }
        }
    }

    // returns all the entities affected by the spell at the targeted location
    public Entity[] getSpellEffectTargets(EffectSpell effect, Entity origin, Cell target) {
        return null;
    }

    // handle the effect, spell or buff alike
    public void handleEffect(EffectHandler handler, Entity origin, Entity target) {
        if (handler is EffectHandlerDirectDamage) {
            handleEffectDirectDamage((EffectHandlerDirectDamage)handler, origin, target);
        } else if (handler is EffectHandlerIndirectDamage) {
            handleEffectIndirectDamage((EffectHandlerIndirectDamage)handler, origin, target);
        } else if (handler is EffectHandlerHeal) {
            handleEffectHeal((EffectHandlerHeal)handler, origin, target);
        } else if (handler is EffectHandlerBuff) {
            handleEffectBuff((EffectHandlerBuff)handler, origin, target);
        } else if (handler is EffectHandlerModAP) {
            handleEffectModAP((EffectHandlerModAP)handler, origin, target);
        } else if (handler is EffectHandlerModMP) {
            handleEffectModMP((EffectHandlerModMP)handler, origin, target);
        }
    }

    public void handleEffectDirectDamage(EffectHandlerDirectDamage handler, Entity origin, Entity target) {
        directDamageEntity(origin, target, handler.damage);
    }

    public void handleEffectIndirectDamage(EffectHandlerIndirectDamage handler, Entity origin, Entity target) {
        indirectDamageEntity(origin, target, handler.damage);
    }

    public void handleEffectHeal(EffectHandlerHeal handler, Entity origin, Entity target) {
        healEntity(origin, target, handler.heal);
    }

    public void handleEffectBuff(EffectHandlerBuff handler, Entity origin, Entity target) {
        addBuffEntity(origin, target, handler.buffId);
    }

    public void handleEffectModAP(EffectHandlerModAP handler, Entity origin, Entity target) {
        modAPEntity(origin, target, handler.AP * handler.direction);
    }

    public void handleEffectModMP(EffectHandlerModMP handler, Entity origin, Entity target) {
        modMPEntity(origin, target, handler.MP * handler.direction);
    }

    public bool areConditionsValid(EffectCondition[] conditions, Entity origin, Entity target) {
        foreach (EffectCondition condition in conditions) {
            if (!isConditionValid(condition, origin, target))
                return false;
        }
        return true;
    }

    public bool isConditionValid(EffectCondition condition, Entity origin, Entity target) {
        return true;
    }

    public void directDamageEntity(Entity origin, Entity target, int damage) {
        damageEntity(origin, target, damage + (origin.damageModifier - target.resistanceModifier));
    }

    public void indirectDamageEntity(Entity origin, Entity target, int damage) {
        damageEntity(origin, target, damage + (origin.damageModifier - target.resistanceModifier));
    }

    public void damageEntity(Entity origin, Entity target, int damage) {
        target.damage(damage);
    }
    
    public void healEntity(Entity origin, Entity target, int heal) {
        target.heal(heal);
    }

    public void addBuffEntity(Entity origin, Entity target, string buffId) {

    }

    public void removeBuffEntity(Entity origin, Entity target, string buffId) {

    }

    public void modAPEntity(Entity origin, Entity target, int AP) {
        target.modAP(AP);
    }

    public void modMPEntity(Entity origin, Entity target, int MP) {
        target.modMP(MP);
    }

    public void modRangeEntity(Entity origin, Entity target, int range) {
        target.modRange(range);
    }

    public void moveEntity(Entity origin) {

    }

    public void spellEntity(Entity origin) {

    }

}
