using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class TurnResolution {

    GameLogic parent;

    public TurnResolution(GameLogic parent) {
        this.parent = parent;
    }

    void resolveAction(Action action) {

    }

    // this function is called if the spell can be used (line of sight, cost, cooldown, etc must already be verified)
    public void resolveMovement(Entity origin, int[] path) {
        moveEntity(origin);
        int destinationCellId = path[path.Length - 1];
        origin.orderMoveToCell(parent.grid.GetCell(destinationCellId));
        //TEST RETRAIT DE MP
        origin.CurrentMP -= path.Length;
        // TODO : move along path
    }

    // this function is called if the spell can be used (line of sight, cost, cooldown, etc must already be verified)
    public void resolveSpell(SpellData spell, Entity origin, Cell target, bool priority) {
        spellEntity(origin);
        foreach (EffectSpell effect in spell.effects) {
            resolveSpellEffect(effect, origin, target, priority);
        }
    }

    // this function will check the conditions for each affected entity, and apply if the effect if necessary
    public void resolveSpellEffect(EffectSpell effect, Entity origin, Cell target, bool priority) {
        if (areGlobalConditionsValid(effect.conditions)) {
            if (areOriginConditionsValid(effect.conditions, origin)) {
                foreach (Cell c in getSpellEffectAffectedCells(effect, origin, target, priority)) {
                    Entity e = c.currentEntity; // get entity on cell
                    if (e != null) {
                        if (e == origin && effect.affectSelf[priority ? 0 : 1]
                            || e.teamId != origin.teamId && effect.affectEnemy[priority ? 0 : 1]
                            || e != origin && e.teamId == origin.teamId && effect.affectAlly[priority ? 0 : 1]) {
                            if (areTargetConditionsValid(effect.conditions, e)) {
                                if (priority) {
                                    handleEntityEffect(effect.quickHandler, origin, e);
                                } else {
                                    handleEntityEffect(effect.slowHandler, origin, e);
                                }
                            }
                        }
                    }
                    if (effect.affectCell[priority ? 0 : 1]) {
                        if (priority) {
                            handleCellEffect(effect.quickHandler, origin, c);
                        } else {
                            handleCellEffect(effect.slowHandler, origin, c);
                        }
                    }
                }
            }
        }
    }


    public void resolveEntityBuffs(Entity e, string trigger) {
        Debug.Log("resolveEntityBuffs (" + trigger + ")");
        foreach (BuffInstance bi in e.buffs) {
            resolveBuff(bi.buff, bi.origin, bi.target.currentCell, trigger);
        }
    }

    public void resolveBuff(BuffData buff, Entity origin, Cell target, string trigger) {
        foreach (EffectBuff effect in buff.effects) {
            resolveBuffEffect(effect, origin, target, trigger);
        }
    }

    public void resolveBuffEffect(EffectBuff effect, Entity origin, Cell target, string trigger) {
        if (areGlobalConditionsValid(effect.conditions)) {
            if (areOriginConditionsValid(effect.conditions, origin)) {
                // look if effect is trigered
                Type effectType = typeof(EffectBuff);
                FieldInfo myFieldInfo = effectType.GetField(trigger);
                EffectHandler effectHandler = (EffectHandler)myFieldInfo.GetValue(effect);
                if (effectHandler != null) {
                    foreach (Cell c in getBuffEffectAffectedCells(effect, origin, target)) {
                        Entity e = c.currentEntity; // get entity on cell
                        if (e != null) {
                            if (e == origin && effect.affectSelf
                                || e.teamId != origin.teamId && effect.affectEnemy
                                || e != origin && e.teamId == origin.teamId && effect.affectAlly) {
                                if (areTargetConditionsValid(effect.conditions, e)) {
                                    handleEntityEffect(effectHandler, origin, e);
                                }
                            }
                        }
                        if (effect.affectCell) {
                            handleCellEffect(effectHandler, origin, c);
                        }
                    }
                }
            }
        }
    }

    // returns all the cells affected by the spell at the targeted location
    public List<Cell> getSpellEffectAffectedCells(EffectSpell effect, Entity origin, Cell target, bool priority) {
        int areaType = effect.areaType[priority ? 0 : 1];
        int minArea = effect.minArea[priority ? 0 : 1];
        int maxArea = effect.maxArea[priority ? 0 : 1];

        return parent.grid.getCellsInRange(target, minArea, maxArea, areaType);
    }
    // returns all the cells affected by the buff at the targeted location
    public List<Cell> getBuffEffectAffectedCells(EffectBuff effect, Entity origin, Cell target) {
        int areaType = effect.areaType;
        int minArea = effect.minArea;
        int maxArea = effect.maxArea;

        return parent.grid.getCellsInRange(target, minArea, maxArea, areaType);
    }

    // ######################################################

    public bool areGlobalConditionsValid(EffectCondition[] conditions) {
        foreach (EffectCondition condition in conditions) {
            if (condition is EffectConditionGlobal)
                if (!isGlobalConditionValid(condition))
                    return false;
        }
        return true;
    }
    public bool isGlobalConditionValid(EffectCondition condition) {
        if (condition is EffectConditionTurnNumberAbove) {
            return isTurnAbove((EffectConditionTurnNumberAbove)condition);
        } else if (condition is EffectConditionTurnNumberBelow) {
            return isTurnBelow((EffectConditionTurnNumberBelow)condition);
        }
        return true;
    }

    public bool isTurnAbove(EffectConditionTurnNumberAbove condition) {
        return condition.turnNumber > parent.currentTurn;
    }
    public bool isTurnBelow(EffectConditionTurnNumberBelow condition) {
        return condition.turnNumber < parent.currentTurn;
    }

    public bool areOriginConditionsValid(EffectCondition[] conditions, Entity origin) {
        foreach (EffectCondition condition in conditions) {
            if (condition is EffectConditionTarget
                && ((EffectConditionTarget)condition).target == EffectConditionTarget.CONDITION_TARGET_ORIGIN)
                if (!isTargetConditionValid(condition, origin))
                    return false;
        }
        return true;
    }
    public bool areTargetConditionsValid(EffectCondition[] conditions, Entity target) {
        foreach (EffectCondition condition in conditions) {
            if (condition is EffectConditionTarget
                && ((EffectConditionTarget)condition).target == EffectConditionTarget.CONDITION_TARGET_TARGET)
                if (!isTargetConditionValid(condition, target))
                    return false;
        }
        return true;
    }
    public bool isTargetConditionValid(EffectCondition condition, Entity target) {
        if (condition is EffectConditionHealthAbove) {
            isTargetHealthAbove((EffectConditionHealthAbove)condition, target);
        } else if (condition is EffectConditionHealthBelow) {
            isTargetHealthBelow((EffectConditionHealthBelow)condition, target);
        } else if (condition is EffectConditionAPAbove) {
            isTargetAPAbove((EffectConditionAPAbove)condition, target);
        } else if (condition is EffectConditionAPBelow) {
            isTargetAPBelow((EffectConditionAPBelow)condition, target);
        } else if (condition is EffectConditionMPAbove) {
            isTargetMPAbove((EffectConditionMPAbove)condition, target);
        } else if (condition is EffectConditionMPBelow) {
            isTargetMPBelow((EffectConditionMPBelow)condition, target);
        }
        return true;
    }

    public bool isTargetHealthAbove(EffectConditionHealthAbove condition, Entity target) {
        return true;
    }
    public bool isTargetHealthBelow(EffectConditionHealthBelow condition, Entity target) {
        return true;
    }

    public bool isTargetAPAbove(EffectConditionAPAbove condition, Entity target) {
        return true;
    }
    public bool isTargetAPBelow(EffectConditionAPBelow condition, Entity target) {
        return true;
    }

    public bool isTargetMPAbove(EffectConditionMPAbove condition, Entity target) {
        return true;
    }
    public bool isTargetMPBelow(EffectConditionMPBelow condition, Entity target) {
        return true;
    }

    // ######################################################

    // handle the entity effect, spell or buff alike
    public void handleEntityEffect(EffectHandler handler, Entity origin, Entity target) {
        if (handler is EffectHandlerDirectDamage) {
            handleEffectDirectDamage((EffectHandlerDirectDamage)handler, origin, target);
        } else if (handler is EffectHandlerIndirectDamage) {
            handleEffectIndirectDamage((EffectHandlerIndirectDamage)handler, origin, target);
        } else if (handler is EffectHandlerHeal) {
            handleEffectHeal((EffectHandlerHeal)handler, origin, target);
        } else if (handler is EffectHandlerBuff) {
            handleEffectBuff((EffectHandlerBuff)handler, origin, target);
        } else if (handler is EffectHandlerDebuff) {
            handleEffectDebuff((EffectHandlerDebuff)handler, origin, target);
        } else if (handler is EffectHandlerModAP) {
            handleEffectModAP((EffectHandlerModAP)handler, origin, target);
        } else if (handler is EffectHandlerModMP) {
            handleEffectModMP((EffectHandlerModMP)handler, origin, target);
        } else if (handler is EffectHandlerModRange) {
            handleEffectModRange((EffectHandlerModRange)handler, origin, target);
        } else if (handler is EffectHandlerStun) {
            handleEffectStun((EffectHandlerStun)handler, origin, target);
        } else if (handler is EffectHandlerUnstun) {
            handleEffectUnstun((EffectHandlerUnstun)handler, origin, target);
        } else if (handler is EffectHandlerPush) {
            handleEffectPush((EffectHandlerPush)handler, origin, target);
        } else if (handler is EffectHandlerPull) {
            handleEffectPull((EffectHandlerPull)handler, origin, target);
        }
    }
    // handle the cell effect, spell or buff alike
    public void handleCellEffect(EffectHandler handler, Entity origin, Cell target) {
        Debug.Log("handleCellEffect");
        if (handler is EffectHandlerDash) {
            handleEffectDash((EffectHandlerDash)handler, origin, target);
        } else if (handler is EffectHandlerWarp) {
            handleEffectWarp((EffectHandlerWarp)handler, origin, target);
        }
    }



    // ######################################################

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
        addBuffEntity(origin, target, handler.buffId, handler.duration);
    }
    public void handleEffectDebuff(EffectHandlerDebuff handler, Entity origin, Entity target) {
        removeBuffEntity(origin, target, handler.buffId);
    }

    public void handleEffectModAP(EffectHandlerModAP handler, Entity origin, Entity target) {
        modAPEntity(origin, target, handler.AP * handler.direction);
    }
    public void handleEffectModMP(EffectHandlerModMP handler, Entity origin, Entity target) {
        modMPEntity(origin, target, handler.MP * handler.direction);
    }
    public void handleEffectModRange(EffectHandlerModRange handler, Entity origin, Entity target) {
        modRangeEntity(origin, target, handler.range);
    }

    public void handleEffectStun(EffectHandlerStun handler, Entity origin, Entity target) {
        stunEntity(origin, target);
    }
    public void handleEffectUnstun(EffectHandlerUnstun handler, Entity origin, Entity target) {
        unstunEntity(origin, target);
    }

    public void handleEffectPush(EffectHandlerPush handler, Entity origin, Entity target) {
        pushEntity(origin, target, handler.distance);
    }
    public void handleEffectPull(EffectHandlerPull handler, Entity origin, Entity target) {
        pullEntity(origin, target, handler.distance);
    }

    public void handleEffectDash(EffectHandlerDash handler, Entity origin, Cell target) {
        dashEntity(origin, target);
    }
    public void handleEffectWarp(EffectHandlerWarp handler, Entity origin, Cell target) {
        warpEntity(origin, target);
    }

    // ######################################################

    public void directDamageEntity(Entity origin, Entity target, int damage) {
        damageEntity(origin, target, damage + (origin.damageModifier - target.resistanceModifier));
    }
    public void indirectDamageEntity(Entity origin, Entity target, int damage) {
        damageEntity(origin, target, damage + (origin.damageModifier - target.resistanceModifier));
    }

    public void damageEntity(Entity origin, Entity target, int damage) {
        Debug.Log("damageEntity " + damage);
        target.damage(damage);
        resolveEntityBuffs(origin, "onDamageHandler");
    }
    public void healEntity(Entity origin, Entity target, int heal) {
        Debug.Log("healEntity " + heal);
        target.heal(heal);
        resolveEntityBuffs(origin, "onHealHandler");
    }

    public void addBuffEntity(Entity origin, Entity target, string buffId, int duration) {
        Debug.Log("addBuffEntity " + buffId);
        if (!target.hasBuff(buffId)) {
            BuffInstance bi = target.addBuffInstance(origin, buffId, duration);
            resolveBuff(bi.buff, bi.origin, bi.target.currentCell, "onGainedHandler");
            resolveEntityBuffs(origin, "onBuffedHandler");
        } else {
            target.refreshBuffInstance(origin, buffId, duration);
        }
    }
    public void removeBuffEntity(Entity origin, Entity target, string buffId) {
        Debug.Log("removeBuffEntity " + buffId);
        if (target.hasBuff(buffId)) {
            BuffInstance bi = target.removeBuffInstance(buffId);
            resolveBuff(bi.buff, bi.origin, bi.target.currentCell, "onLostHandler");
        }
    }

    public void modAPEntity(Entity origin, Entity target, int AP) {
        Debug.Log("modAPEntity " + AP);
        target.modAP(AP);
    }
    public void modMPEntity(Entity origin, Entity target, int MP) {
        Debug.Log("modMPEntity " + MP);
        target.modMP(MP);
    }
    public void modRangeEntity(Entity origin, Entity target, int range) {
        Debug.Log("modRangeEntity " + range);
        target.modRange(range);
    }

    public void stunEntity(Entity origin, Entity target) {
        Debug.Log("stunEntity");
        target.stun();
    }
    public void unstunEntity(Entity origin, Entity target) {
        Debug.Log("unstunEntity");
        target.unstun();
    }

    public void pushEntity(Entity origin, Entity target, int distance) {
        Debug.Log("pushEntity " + distance);
        resolveEntityBuffs(origin, "onMoveHandler");
    }
    public void pullEntity(Entity origin, Entity target, int distance) {
        Debug.Log("pullEntity " + distance);
        resolveEntityBuffs(origin, "onMoveHandler");
    }

    public void dashEntity(Entity origin, Cell target) {
        Debug.Log("dashEntity");
        origin.orderMoveToCell(target);
        resolveEntityBuffs(origin, "onMoveHandler");
    }
    public void warpEntity(Entity origin, Cell target) {
        Debug.Log("warpEntity");
        origin.setCurrentCell(target);
        resolveEntityBuffs(origin, "onMoveHandler");
    }

    // ######################################################

    public void moveEntity(Entity origin) {
        resolveEntityBuffs(origin, "onMoveHandler");
    }
    public void spellEntity(Entity origin) {
        resolveEntityBuffs(origin, "onSpellHandler");
    }

}
