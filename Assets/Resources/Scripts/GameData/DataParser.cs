using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class DataParser {

    /*
     * Call those lines in the LoadingScreen.Start() function for an example
     * 
        DataParser.buildSpellAndBuffData();
        Debug.Log(DataParser.BUFF_DATA["B001"].description);
        Debug.Log(DataParser.SPELL_DATA["S001"].description);
        Debug.Log(DataParser.SPELL_DATA["S002"].description);
     * 
     */

    public static Dictionary<string, GameData> GAME_DATA = new Dictionary<string, GameData>();
    public static Dictionary<string, Spell> SPELL_DATA = new Dictionary<string, Spell>();
    public static Dictionary<string, Buff> BUFF_DATA = new Dictionary<string, Buff>();
    public static Dictionary<string, CellType> CELL_DATA = new Dictionary<string, CellType>();
    public static Dictionary<string, MapData> MAP_DATA = new Dictionary<string, MapData>();

    public static string getMacroContent(string macro) {
        string[] path = macro.Replace(" ", "").Split(',');
        if (path.Length == 1)
            return ""; // should not happen
        GameData data = GAME_DATA[path[0]];
        Debug.Log("(" + 0 + "/" + path.Length + ") " + path[0] + " : " + data.ToString());
        int i = 1;
        object content = null;
        if (data is Spell)
            content = GameData.getFieldContent<Spell>((Spell)data, path[i]);
        else if (data is Buff)
            content = GameData.getFieldContent<Buff>((Buff)data, path[i]);
        Debug.Log("(" + i + "/" + path.Length + ") " + path[i] + " : " + content);
        i++;
        while (i < path.Length) { // going down nested objects
            if (content is Array) {
                Debug.Log("(" + i + "/" + path.Length + ") " + path[i] + " : " + content);
                int index = int.Parse(path[i]);
                if (content is EffectSpell[]) {
                    content = ((EffectSpell[])content)[index];
                }
                if (content is EffectBuff[]) {
                    content = ((EffectBuff[])content)[index];
                }
                Debug.Log("(" + i + "/" + path.Length + ") " + path[i] + " : " + content);
            } else {
                Debug.Log("(" + i + "/" + path.Length + ") " + path[i] + " : " + content);
                if (content is EffectSpell)
                    content = GameData.getFieldContent<EffectSpell>((EffectSpell)content, path[i]);
                else if (content is EffectBuff)
                    content = GameData.getFieldContent<EffectBuff>((EffectBuff)content, path[i]);
                else if (content is EffectHandlerDamage) {
                    content = GameData.getFieldContent<EffectHandlerDamage>((EffectHandlerDamage)content, path[i]).ToString();
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
                }
                if (i < path.Length)
                    Debug.Log("(" + i + "/" + path.Length + ") " + path[i] + " : " + content);
            }
            i++;
        }
        return content.ToString();
    }

    public static void buildSpellAndBuffData() {
        ObjectJSON data = ParserJSON.getObjectJSONFromAsset("DATA");
        ArrayJSON buffs = data.getArrayJSON("buffs");
        ArrayJSON spells = data.getArrayJSON("spells");
        ArrayJSON cellTypes = data.getArrayJSON("cellTypes");
        ArrayJSON maps = data.getArrayJSON("maps");

        // parse and save data
        foreach (Spell s in buildSpells(spells)) {
            if (s != null) {
                GAME_DATA[s.id] = s;
                SPELL_DATA[s.id] = s;
            }
        }
        foreach (Buff b in buildBuffs(buffs)) {
            if (b != null) {
                GAME_DATA[b.id] = b;
                BUFF_DATA[b.id] = b;
            }
        }
        foreach (CellType ct in buildCellTypes(cellTypes)) {
            if (ct != null) {
                GAME_DATA[ct.id] = ct;
                CELL_DATA[ct.id] = ct;
            }
        }
        foreach (MapData m in buildMaps(maps)) {
            if (m != null) {
                GAME_DATA[m.id] = m;
                MAP_DATA[m.id] = m;
            }
        }

        // fill description macro
        foreach (Spell s in SPELL_DATA.Values) {
            string macro = StringParsingTool.getNextMacro(s.description);
            while (macro != "") {
                s.description = s.description.Replace(macro, getMacroContent(StringParsingTool.getBetweenMacro(macro)));
                macro = StringParsingTool.getNextMacro(s.description);
            }
        }
        foreach (Buff b in BUFF_DATA.Values) {
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
            output[i] = buildMap((ObjectJSON)array[i]);
        return output;
    }

    public static MapData buildMap(ObjectJSON map) {
        MapData output = new MapData();
        output.id = map.getString("id");
        output.name = map.getString("name");
        output.cells = buildMapCells(map.getArrayJSON("cells"));
        output.spawns = buildMapSpawns(map.getArrayJSON("spanws"));
        return output;
    }

    public static CellType[] buildMapCells(ArrayJSON array) {
        CellType[] output = new CellType[array.Length];
        for (int i = 0; i < array.Length; i++)
            output[i] = CELL_DATA[(string)array[i]];
        return output;
    }

    public static int[] buildMapSpawns(ArrayJSON array) {
        int[] output = new int[array.Length];
        for (int i = 0; i < array.Length; i++)
            output[i] = int.Parse((string)array[i]);
        return output;
    }

    public static CellType[] buildCellTypes(ArrayJSON array) {
        CellType[] output = new CellType[array.Length];
        for (int i = 0; i < array.Length; i++)
            output[i] = buildCellType((ObjectJSON)array[i]);
        return output;
    }

    public static CellType buildCellType(ObjectJSON cellType) {
        CellType output = new CellType();
        output.id = cellType.getString("id");
        output.name = cellType.getString("name");
        output.blockMovement = cellType.getBool("blockMovement");
        output.blockLineOfSight = cellType.getBool("blockLineOfSight");
        return output;
    }

    public static Buff[] buildBuffs(ArrayJSON array) {
        Buff[] output = new Buff[array.Length];
        for (int i = 0; i < array.Length; i++)
            output[i] = buildBuff((ObjectJSON)array[i]);
        return output;
    }

    public static Buff buildBuff(ObjectJSON buff) {
        Buff output = new Buff();
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
        output.affectAlly = effect.containsValue("affectAlly");
        output.affectEnemy = effect.containsValue("affectEnemy");
        output.affectSelf = effect.containsValue("affectSelf");
        output.affectCell = effect.containsValue("affectCell");
        output.minArea = effect.getInt("minArea", 0);
        output.maxArea = effect.getInt("maxArea", 0);
        output.areaType = Spell.stringToRangeAreaType(effect.getString("areaType", ""));
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
        return output;
    }

    public static Spell[] buildSpells(ArrayJSON array) {
        Spell[] output = new Spell[array.Length];
        for (int i = 0; i < array.Length; i++)
            output[i] = buildSpell((ObjectJSON)array[i]);
        return output;
    }

    public static Spell buildSpell(ObjectJSON spell) {
        Spell output = new Spell();
        output.id = spell.getString("id");
        output.name = spell.getString("name");
        output.iconPath = spell.getString("iconPath");
        output.description = spell.getString("description");
        output.cost = spell.getInt("cost");
        output.cooldown = spell.getInt("cooldown");
        output.minRange = spell.getInt("minRange");
        output.maxRange = spell.getInt("maxRange");
        output.rangeType = Spell.stringToRangeAreaType(spell.getString("rangeType", ""));
        output.priority = spell.getInt("priority");
        output.effects = buildEffectSpells(spell.getArrayJSON("effects"));
        return output;
    }

    public static EffectSpell[] buildEffectSpells(ArrayJSON effects) {
        EffectSpell[] output = new EffectSpell[effects.Length];
        for (int i = 0; i < effects.Length; i++)
            output[i] = buildEffectSpell((ObjectJSON)effects[i]);
        return output;
    }

    public static EffectSpell buildEffectSpell(ObjectJSON effect) {
        EffectSpell output = new EffectSpell();
        output.affectAlly = effect.containsValue("affectAlly");
        output.affectEnemy = effect.containsValue("affectEnemy");
        output.affectSelf = effect.containsValue("affectSelf");
        output.affectCell = effect.containsValue("affectCell");
        output.minArea = effect.getInt("minArea", 0);
        output.maxArea = effect.getInt("maxArea", 0);
        output.areaType = Spell.stringToRangeAreaType(effect.getString("areaType", ""));
        output.effectHandler = buildEffectHandler(effect.getObjectJSON("effectHandler"));
        return output;
    }

    public static EffectHandler buildEffectHandler(ObjectJSON effectHandler) {
        EffectHandler output;
        if (effectHandler != null && effectHandler.containsValue("class")) { // should always contain it
            string className = effectHandler.getString("class");
            if (className == "EffectHandlerDamage") {
                output = new EffectHandlerDamage();
                ((EffectHandlerDamage)output).damage = effectHandler.getInt("damage");
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
                ((EffectHandlerModMP)output).direction = effectHandler.getInt("direction");
            } else {
                output = null;
            }
            return output;
        } else
            return null;
    }
}
