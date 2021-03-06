﻿using System;
using Data;
using Tools.JSON;

namespace Tools {

    public class DataParser {

        /*
         * Call those lines in the LoadingScreen.Start() function for an example
         * 
            DataParser.loadDataJSON();
         * 
         */

        public static string getMacroContent(string macro) {
            string[] path = macro.Replace(" ", "").Split(',');
            if (path.Length == 1)
                return ""; // should not happen
            Data.GameData data = DataManager.GAME_DATA[path[0]];
            int i = 1;
            object content = null;
            if (data is SpellData)
                content = GameData.getFieldContent<SpellData>((SpellData)data, path[i]);
            else if (data is BuffData)
                content = GameData.getFieldContent<BuffData>((BuffData)data, path[i]);
            i++;
            while (i < path.Length) { // going down nested objects
                if (content is Array) {
                    int index = int.Parse(path[i]);
                    if (content is EffectSpell[]) {
                        content = ((EffectSpell[])content)[index];
                    }
                    if (content is EffectBuff[]) {
                        content = ((EffectBuff[])content)[index];
                    }
                    if (content is EffectCondition[]) {
                        content = ((EffectCondition[])content)[index];
                    }
                } else {
                    if (content is EffectSpell)
                        content = GameData.getFieldContent<EffectSpell>((EffectSpell)content, path[i]);
                    else if (content is EffectBuff)
                        content = GameData.getFieldContent<EffectBuff>((EffectBuff)content, path[i]);
                    else if (content is EffectHandlerDirectDamage) {
                        content = GameData.getFieldContent<EffectHandlerDirectDamage>((EffectHandlerDirectDamage)content, path[i]).ToString();
                        i++;
                    } else if (content is EffectHandlerIndirectDamage) {
                        content = GameData.getFieldContent<EffectHandlerIndirectDamage>((EffectHandlerIndirectDamage)content, path[i]).ToString();
                        i++;
                    } else if (content is EffectHandlerHeal) {
                        content = GameData.getFieldContent<EffectHandlerHeal>((EffectHandlerHeal)content, path[i]).ToString();
                        i++;
                    } else if (content is EffectHandlerBuff) {
                        content = GameData.getFieldContent<EffectHandlerBuff>((EffectHandlerBuff)content, path[i]).ToString();
                        i++;
                    } else if (content is EffectHandlerModAP) {
                        content = GameData.getFieldContent<EffectHandlerModAP>((EffectHandlerModAP)content, path[i]).ToString();
                        i++;
                    } else if (content is EffectHandlerModMP) {
                        content = GameData.getFieldContent<EffectHandlerModMP>((EffectHandlerModMP)content, path[i]).ToString();
                        i++;
                    } else if (content is EffectConditionTurnNumberAbove) {
                        content = GameData.getFieldContent<EffectConditionTurnNumberAbove>((EffectConditionTurnNumberAbove)content, path[i]).ToString();
                        i++;
                    } else if (content is EffectConditionTurnNumberBelow) {
                        content = GameData.getFieldContent<EffectConditionTurnNumberBelow>((EffectConditionTurnNumberBelow)content, path[i]).ToString();
                        i++;
                    } else if (content is EffectConditionHealthAbove) {
                        content = GameData.getFieldContent<EffectConditionHealthAbove>((EffectConditionHealthAbove)content, path[i]).ToString();
                        i++;
                    } else if (content is EffectConditionHealthBelow) {
                        content = GameData.getFieldContent<EffectConditionHealthBelow>((EffectConditionHealthBelow)content, path[i]).ToString();
                        i++;
                    } else if (content is EffectConditionAPAbove) {
                        content = GameData.getFieldContent<EffectConditionAPAbove>((EffectConditionAPAbove)content, path[i]).ToString();
                        i++;
                    } else if (content is EffectConditionAPBelow) {
                        content = GameData.getFieldContent<EffectConditionAPBelow>((EffectConditionAPBelow)content, path[i]).ToString();
                        i++;
                    } else if (content is EffectConditionMPAbove) {
                        content = GameData.getFieldContent<EffectConditionMPAbove>((EffectConditionMPAbove)content, path[i]).ToString();
                        i++;
                    } else if (content is EffectConditionMPBelow) {
                        content = GameData.getFieldContent<EffectConditionMPBelow>((EffectConditionMPBelow)content, path[i]).ToString();
                        i++;
                    } else if (content is EffectConditionHasBuff) {
                        content = GameData.getFieldContent<EffectConditionHasBuff>((EffectConditionHasBuff)content, path[i]).ToString();
                        i++;
                    } else if (content is EffectConditionHasNotBuff) {
                        content = GameData.getFieldContent<EffectConditionHasNotBuff>((EffectConditionHasNotBuff)content, path[i]).ToString();
                        i++;
                    }
                }
                i++;
            }
            return content.ToString();
        }

        public static void loadDataJSON() {
            ObjectJSON data = ParserJSON.getObjectJSONFromAsset("DATA");
            ArrayJSON buffs = data.getArrayJSON("buffs");
            ArrayJSON spells = data.getArrayJSON("spells");
            ArrayJSON cellTypes = data.getArrayJSON("cellTypes");
            ArrayJSON maps = data.getArrayJSON("maps");

            // parse and save data
            foreach (SpellData s in buildSpells(spells)) {
                if (s != null) {
                    DataManager.registerData(s.id, s);
                }
            }
            foreach (BuffData b in buildBuffs(buffs)) {
                if (b != null) {
                    DataManager.registerData(b.id, b);
                }
            }
            foreach (CellData ct in buildCellTypes(cellTypes)) {
                if (ct != null) {
                    DataManager.registerData(ct.id, ct);
                }
            }
            foreach (MapData m in buildMaps(maps)) {
                if (m != null) {
                    DataManager.registerData(m.id, m);
                }
            }

            // fill description macro
            foreach (SpellData s in DataManager.SPELL_DATA.Values) {
                for (int i = 0; i < 2; i++) {
                    string macro = StringParsingTool.getNextMacro(s.description[i]);
                    while (macro != "") {
                        s.description[i] = s.description[0].Replace(macro, getMacroContent(StringParsingTool.getBetweenMacro(macro)));
                        macro = StringParsingTool.getNextMacro(s.description[i]);
                    }
                }
            }
            foreach (BuffData b in DataManager.BUFF_DATA.Values) {
                string macro = StringParsingTool.getNextMacro(b.description);
                while (macro != "") {
                    b.description = b.description.Replace(macro, getMacroContent(StringParsingTool.getBetweenMacro(macro)));
                    macro = StringParsingTool.getNextMacro(b.description);
                }
            }
        }

        public static MapData[] buildMaps(ArrayJSON array) {
            MapData[] output = new MapData[array.Length];
            for (int i = 0; i < array.Length; i++)
                output[i] = buildMap(array.getObjectJSONAt(i));
            return output;
        }

        public static MapData buildMap(ObjectJSON map) {
            MapData output = new MapData();
            output.id = map.getString("id");
            output.name = map.getString("name");
            output.width = map.getInt("width");
            output.height = map.getInt("height");
            output.cells = buildMapCells(map.getArrayJSON("cells"));
            output.spawns = buildMapSpawns(map.getArrayJSON("spawns"));
            output.buildCellDataIdList();
            return output;
        }

        public static CellData[] buildMapCells(ArrayJSON array) {
            CellData[] output = new CellData[array.Length];
            for (int i = 0; i < array.Length; i++) {
                output[i] = DataManager.CELL_DATA[array.getStringAt(i)];
            }
            return output;
        }

        public static int[] buildMapSpawns(ArrayJSON array) {
            int[] output = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
                output[i] = int.Parse(array.getStringAt(i));
            return output;
        }

        public static CellData[] buildCellTypes(ArrayJSON array) {
            CellData[] output = new CellData[array.Length];
            for (int i = 0; i < array.Length; i++)
                output[i] = buildCellType(array.getObjectJSONAt(i));
            return output;
        }

        public static CellData buildCellType(ObjectJSON cellType) {
            CellData output = new CellData();
            output.id = cellType.getString("id");
            output.name = cellType.getString("name");
            output.modelPath = cellType.getString("modelPath");
            output.blockMovement = cellType.getBool("blockMovement");
            output.blockLineOfSight = cellType.getBool("blockLineOfSight");
            return output;
        }

        public static BuffData[] buildBuffs(ArrayJSON array) {
            BuffData[] output = new BuffData[array.Length];
            for (int i = 0; i < array.Length; i++)
                output[i] = buildBuff(array.getObjectJSONAt(i));
            return output;
        }

        public static BuffData buildBuff(ObjectJSON buff) {
            BuffData output = new BuffData();
            output.id = buff.getString("id");
            output.name = buff.getString("name");
            output.iconPath = buff.getString("iconPath");
            output.description = buff.getString("description");
            output.effects = buildEffectBuffs(buff.getArrayJSON("effects"));
            return output;
        }

        public static EffectBuff[] buildEffectBuffs(ArrayJSON effects) {
            EffectBuff[] output = new EffectBuff[effects.Length];
            for (int i = 0; i < effects.Length; i++)
                output[i] = buildEffectBuff((ObjectJSON)effects[i]);
            return output;
        }

        public static EffectBuff buildEffectBuff(ObjectJSON effect) {
            EffectBuff output = new EffectBuff();
            output.affectAlly = effect.getBool("affectAlly");
            output.affectEnemy = effect.getBool("affectEnemy");
            output.affectSelf = effect.getBool("affectSelf");
            output.affectCell = effect.getBool("affectCell");
            output.minArea = effect.getInt("minArea", 0);
            output.maxArea = effect.getInt("maxArea", 0);
            output.areaType = SpellData.stringToRangeAreaType(effect.getString("areaType", ""));
            output.onGainedHandler = buildEffectHandler(effect.getObjectJSON("onGainedHandler"));
            output.onLostHandler = buildEffectHandler(effect.getObjectJSON("onLostHandler"));
            output.onDamageHandler = buildEffectHandler(effect.getObjectJSON("onDamageHandler"));
            output.onHealHandler = buildEffectHandler(effect.getObjectJSON("onHealHandler"));
            output.onSpellHandler = buildEffectHandler(effect.getObjectJSON("onSpellHandler"));
            output.onBuffedHandler = buildEffectHandler(effect.getObjectJSON("onBuffedHandler"));
            output.onEnterHandler = buildEffectHandler(effect.getObjectJSON("onEnterHandler"));
            output.onLeaveHandler = buildEffectHandler(effect.getObjectJSON("onLeaveHandler"));
            output.onTurnStartHandler = buildEffectHandler(effect.getObjectJSON("onTurnStartHandler"));
            output.onTurnEndHandler = buildEffectHandler(effect.getObjectJSON("onTurnEndHandler"));
            output.conditions = buildEffectConditions(effect.getArrayJSON("conditions"));
            return output;
        }

        public static SpellData[] buildSpells(ArrayJSON array) {
            SpellData[] output = new SpellData[array.Length];
            for (int i = 0; i < array.Length; i++)
                output[i] = buildSpell(array.getObjectJSONAt(i));
            return output;
        }

        public static SpellData buildSpell(ObjectJSON spell) {
            SpellData output = new SpellData();
            output.id = spell.getString("id");
            output.name = spell.getString("name");
            output.iconPath = spell.getString("iconPath");
            output.description = spell.getStringArray("description");
            output.cost = spell.getIntArray("cost");
            output.cooldown = spell.getIntArray("cooldown");
            output.minRange = spell.getIntArray("minRange");
            output.maxRange = spell.getIntArray("maxRange");
            output.rangeType = SpellData.stringToRangeAreaType(spell.getStringArray("rangeType"));
            output.priority = spell.getIntArray("priority");
            output.effects = buildEffectSpells(spell.getArrayJSON("effects"));
            return output;
        }

        public static EffectSpell[] buildEffectSpells(ArrayJSON effects) {
            if (effects != null) {
                EffectSpell[] output = new EffectSpell[effects.Length];
                for (int i = 0; i < effects.Length; i++)
                    output[i] = buildEffectSpell(effects.getObjectJSONAt(i));
                return output;
            }
            return new EffectSpell[0];
        }

        public static EffectSpell buildEffectSpell(ObjectJSON effect) {
            EffectSpell output = new EffectSpell();
            output.affectAlly = effect.getBoolArray("affectAlly");
            output.affectEnemy = effect.getBoolArray("affectEnemy");
            output.affectSelf = effect.getBoolArray("affectSelf");
            output.affectCell = effect.getBoolArray("affectCell");
            output.minArea = effect.getIntArray("minArea");
            output.maxArea = effect.getIntArray("maxArea");
            output.areaType = SpellData.stringToRangeAreaType(effect.getStringArray("areaType"));
            output.quickHandler = buildEffectHandler(effect.getObjectJSON("quickHandler"));
            output.slowHandler = buildEffectHandler(effect.getObjectJSON("slowHandler"));
            output.conditions = buildEffectConditions(effect.getArrayJSON("conditions"));
            return output;
        }

        public static EffectCondition[] buildEffectConditions(ArrayJSON conditions) {
            if (conditions != null) {
                EffectCondition[] output = new EffectCondition[conditions.Length];
                for (int i = 0; i < conditions.Length; i++)
                    output[i] = buildEffectCondition(conditions.getObjectJSONAt(i));
                return output;
            }
            return new EffectCondition[0];
        }

        public static EffectCondition buildEffectCondition(ObjectJSON condition) {
            EffectCondition output;
            if (condition != null && condition.containsValue("class")) { // should always contain it
                string className = condition.getString("class");
                if (className == "EffectConditionTurnNumberAbove") {
                    output = new EffectConditionTurnNumberAbove();
                    ((EffectConditionTurnNumberAbove)output).turnNumber = condition.getInt("turnNumber");
                } else if (className == "EffectConditionTurnNumberBelow") {
                    output = new EffectConditionTurnNumberBelow();
                    ((EffectConditionTurnNumberBelow)output).turnNumber = condition.getInt("turnNumber");
                } else if (className == "EffectConditionHealthAbove") {
                    output = new EffectConditionHealthAbove();
                    ((EffectConditionHealthAbove)output).target = EffectConditionTarget.stringToConditionTarget(condition.getString("target"));
                    ((EffectConditionHealthAbove)output).health = condition.getInt("health");
                    ((EffectConditionHealthAbove)output).percent = condition.getBool("percent");
                } else if (className == "EffectConditionHealthBelow") {
                    output = new EffectConditionHealthBelow();
                    ((EffectConditionHealthBelow)output).target = EffectConditionTarget.stringToConditionTarget(condition.getString("target"));
                    ((EffectConditionHealthBelow)output).health = condition.getInt("health");
                    ((EffectConditionHealthBelow)output).percent = condition.getBool("percent");
                } else if (className == "EffectConditionAPAbove") {
                    output = new EffectConditionAPAbove();
                    ((EffectConditionAPAbove)output).target = EffectConditionTarget.stringToConditionTarget(condition.getString("target"));
                    ((EffectConditionAPAbove)output).AP = condition.getInt("AP");
                    ((EffectConditionAPAbove)output).percent = condition.getBool("percent");
                } else if (className == "EffectConditionAPBelow") {
                    output = new EffectConditionAPBelow();
                    ((EffectConditionAPBelow)output).target = EffectConditionTarget.stringToConditionTarget(condition.getString("target"));
                    ((EffectConditionAPBelow)output).AP = condition.getInt("AP");
                    ((EffectConditionAPBelow)output).percent = condition.getBool("percent");
                } else if (className == "EffectConditionMPAbove") {
                    output = new EffectConditionMPAbove();
                    ((EffectConditionMPAbove)output).target = EffectConditionTarget.stringToConditionTarget(condition.getString("target"));
                    ((EffectConditionMPAbove)output).MP = condition.getInt("MP");
                    ((EffectConditionMPAbove)output).percent = condition.getBool("percent");
                } else if (className == "EffectConditionMPBelow") {
                    output = new EffectConditionMPBelow();
                    ((EffectConditionMPBelow)output).target = EffectConditionTarget.stringToConditionTarget(condition.getString("target"));
                    ((EffectConditionMPBelow)output).MP = condition.getInt("MP");
                    ((EffectConditionMPBelow)output).percent = condition.getBool("percent");
                } else if (className == "EffectConditionHasBuff") {
                    output = new EffectConditionHasBuff();
                    ((EffectConditionHasBuff)output).target = EffectConditionTarget.stringToConditionTarget(condition.getString("target"));
                    ((EffectConditionHasBuff)output).buffId = condition.getString("buffId");
                } else if (className == "EffectConditionHasNotBuff") {
                    output = new EffectConditionHasNotBuff();
                    ((EffectConditionHasNotBuff)output).target = EffectConditionTarget.stringToConditionTarget(condition.getString("target"));
                    ((EffectConditionHasNotBuff)output).buffId = condition.getString("buffId");
                } else {
                    output = null;
                }
                return output;
            } else
                return null;
        }

        public static EffectHandler buildEffectHandler(ObjectJSON effectHandler) {
            EffectHandler output;
            if (effectHandler != null && effectHandler.containsValue("class")) { // should always contain it
                string className = effectHandler.getString("class");
                if (className == "EffectHandlerDirectDamage") {
                    output = new EffectHandlerDirectDamage();
                    ((EffectHandlerDirectDamage)output).damage = effectHandler.getInt("damage");
                } else if (className == "EffectHandlerIndirectDamage") {
                    output = new EffectHandlerIndirectDamage();
                    ((EffectHandlerIndirectDamage)output).damage = effectHandler.getInt("damage");
                } else if (className == "EffectHandlerHeal") {
                    output = new EffectHandlerHeal();
                    ((EffectHandlerHeal)output).heal = effectHandler.getInt("heal");
                } else if (className == "EffectHandlerBuff") {
                    output = new EffectHandlerBuff();
                    ((EffectHandlerBuff)output).buffId = effectHandler.getString("buffId");
                    ((EffectHandlerBuff)output).duration = effectHandler.getInt("duration");
                } else if (className == "EffectHandlerModMP") {
                    output = new EffectHandlerModMP();
                    ((EffectHandlerModMP)output).MP = effectHandler.getInt("MP");
                    ((EffectHandlerModMP)output).direction = effectHandler.getInt("direction");
                } else if (className == "EffectHandlerModAP") {
                    output = new EffectHandlerModAP();
                    ((EffectHandlerModAP)output).AP = effectHandler.getInt("AP");
                    ((EffectHandlerModAP)output).direction = effectHandler.getInt("direction");
                } else if (className == "EffectHandlerModRange") {
                    output = new EffectHandlerModRange();
                    ((EffectHandlerModRange)output).range = effectHandler.getInt("range");
                } else if (className == "EffectHandlerStun") {
                    output = new EffectHandlerStun();
                } else if (className == "EffectHandlerUnstun") {
                    output = new EffectHandlerUnstun();
                } else if (className == "EffectHandlerPush") {
                    output = new EffectHandlerPush();
                    ((EffectHandlerPush)output).distance = effectHandler.getInt("distance");
                } else if (className == "EffectHandlerPull") {
                    output = new EffectHandlerPull();
                    ((EffectHandlerPull)output).distance = effectHandler.getInt("distance");
                } else if (className == "EffectHandlerDash") {
                    output = new EffectHandlerDash();
                } else if (className == "EffectHandlerWarp") {
                    output = new EffectHandlerWarp();
                } else {
                    output = null;
                }
                return output;
            } else
                return null;
        }
    }
}