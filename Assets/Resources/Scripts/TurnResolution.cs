using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnResolution {

    void resolveTurn() {

    }

    void resolveAction() {

    }

    // this function is called if the spell can be used (line of sight, cost, cooldown, etc must already be verified)
    void resolveSpell(Spell spell, Entity origin, Cell target) {
        foreach (EffectSpell effect in spell.effects) {
            resolveSpellEffect(effect, origin, target);
        }
    }

    // this function will check the conditions for each affected entity, and apply if the effect if necessary
    void resolveSpellEffect(EffectSpell effect, Entity origin, Cell target) {
        foreach (Entity e in getSpellEffectTargets(effect, origin, target)) {
            if (areConditionsValid(effect.conditions, origin, e)) {
                handleEffect(effect.effectHandler, origin, e);
            }
        }
    }

    // returns all the entities affected by the spell at the targeted location
    Entity[] getSpellEffectTargets(EffectSpell effect, Entity origin, Cell target) {
        return null;
    }

    // handle the effect, spell or buff alike
    void handleEffect(EffectHandler handler, Entity origin, Entity target) {
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

    void handleEffectDirectDamage(EffectHandlerDirectDamage handler, Entity origin, Entity target) {
        indirectDamageEntity(origin, target, handler.damage);
    }

    void handleEffectIndirectDamage(EffectHandlerIndirectDamage handler, Entity origin, Entity target) {
        directDamageEntity(origin, target, handler.damage);
    }

    void handleEffectHeal(EffectHandlerHeal handler, Entity origin, Entity target) {
        healEntity(origin, target, handler.heal);
    }

    void handleEffectBuff(EffectHandlerBuff handler, Entity origin, Entity target) {
        addBuffEntity(origin, target, handler.buffId);
    }

    void handleEffectModAP(EffectHandlerModAP handler, Entity origin, Entity target) {
        modAPEntity(origin, target, handler.AP * handler.direction);
    }

    void handleEffectModMP(EffectHandlerModMP handler, Entity origin, Entity target) {
        modMPEntity(origin, target, handler.MP * handler.direction);
    }

    bool areConditionsValid(EffectCondition[] conditions, Entity origin, Entity target) {
        foreach (EffectCondition condition in conditions) {
            if (!isConditionValid(condition, origin, target))
                return false;
        }
        return true;
    }

    bool isConditionValid(EffectCondition condition, Entity origin, Entity target) {
        return true;
    }

    void directDamageEntity(Entity origin, Entity target, int damage) {
        damageEntity(origin, target, damage + (origin.damageModifier - target.resistanceModifier));
    }

    void indirectDamageEntity(Entity origin, Entity target, int damage) {
        damageEntity(origin, target, damage + (origin.damageModifier - target.resistanceModifier));
    }

    void damageEntity(Entity origin, Entity target, int damage) {
        target.damage(damage);
    }

    void healEntity(Entity origin, Entity target, int heal) {
        target.heal(heal);
    }

    void addBuffEntity(Entity origin, Entity target, string buffId) {

    }

    void removeBuffEntity(Entity origin, Entity target, string buffId) {

    }

    void modAPEntity(Entity origin, Entity target, int AP) {
        target.modAP(AP);
    }

    void modMPEntity(Entity origin, Entity target, int MP) {
        target.modMP(MP);
    }

    void modRangeEntity(Entity origin, Entity target, int range) {
        target.modRange(range);
    }

    void moveEntity(Entity origin) {

    }

    void spellEntity(Entity origin) {

    }

}
