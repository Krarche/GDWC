using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogicClient : GameLogic {

    public static GameLogicClient game;
    public User localUser {
        get { return NetworkMasterClient.singleton.user; }
    }
    public Player localPlayer {
        get { return localUser.player; }
    }
    public Entity localEntity {
        get { return localPlayer.playerEntity; }
    }
    public Queue<Action> localActions {
        get { return localEntity.actions; }
    }
    public SpellInstance[] localSpells {
        get {
            return localEntity.spells;
        }
    }

    public GameLogicClient(string mapId) : base(mapId) {
        game = this;
    }

    public override void startTurn() {
        base.startTurn();
    }

    public override void resolveTurn() {
        base.resolveTurn();
    }

    public void clearGame() {
        game.grid.clearGrid();
        foreach (Entity e in game.entityList.Values) {
            game.removeEntity(e);
        }
        entityList.Clear();
        game = null;
    }

    public const short BUTTON_TYPE_ACTION_ROOT = 0;
    public const short BUTTON_TYPE_ACTION_MOVEMENT = 1;
    public const short BUTTON_TYPE_ACTION_QUICK_SPELL = 2;
    public const short BUTTON_TYPE_ACTION_SLOW_SPELL = 3;
    public const short BUTTON_TYPE_SPELL_0 = 10;
    public const short BUTTON_TYPE_SPELL_1 = 11;
    public const short BUTTON_TYPE_SPELL_2 = 12;
    public const short BUTTON_TYPE_SPELL_3 = 13;
    public const short BUTTON_TYPE_CONFIRM = 20;
    public const short BUTTON_TYPE_CANCEL = 21;
    public const short BUTTON_TYPE_READy = 22;

    public void buttonInput(short type) {
        Debug.Log(type);
        switch (type) {
            case BUTTON_TYPE_SPELL_0:
                buttonSpellHandler(type);
                break;
            case BUTTON_TYPE_SPELL_1:
                buttonSpellHandler(type);
                break;
            case BUTTON_TYPE_SPELL_2:
                buttonSpellHandler(type);
                break;
            case BUTTON_TYPE_SPELL_3:
                buttonSpellHandler(type);
                break;
            case BUTTON_TYPE_CONFIRM:
                buttonConfirmHandler();
                break;
            case BUTTON_TYPE_CANCEL:
                buttonCancelHandler();
                break;
        }
    }


    public static short ACTION_SELECTION_STATE_ROOT = 0;
    public static short ACTION_SELECTION_STATE_QUICK = 1;
    public static short ACTION_SELECTION_STATE_SLOW = 2;
    public static short ACTION_SELECTION_STATE_MOVEMENT = 3;
    public static short ACTION_SELECTION_STATE_READY = 4;
    public short currentActionSelectionState = 0;

    public bool isAiming {
        get {
            return currentActionSelectionState != ACTION_SELECTION_STATE_ROOT &&
                currentActionSelectionState != ACTION_SELECTION_STATE_READY;
        }
    }
    public bool isSpelling {
        get {
            return isQuickSpelling || isSlowSpelling;
        }
    }
    public bool isQuickSpelling {
        get {
            return currentActionSelectionState == ACTION_SELECTION_STATE_QUICK;
        }
    }
    public bool isSlowSpelling {
        get {
            return currentActionSelectionState == ACTION_SELECTION_STATE_SLOW;
        }
    }
    public bool isMoving {
        get {
            return currentActionSelectionState == ACTION_SELECTION_STATE_MOVEMENT;
        }
    }


    private void buttonMovementHandler() {
        currentActionSelectionState = ACTION_SELECTION_STATE_MOVEMENT;
    }

    private void buttonQuickSpellHandler() {
        currentActionSelectionState = ACTION_SELECTION_STATE_QUICK;
        currentSelectedSpell = SELECTED_SPELL_NONE;
    }

    private void buttonSlowSpellHandler() {
        currentActionSelectionState = ACTION_SELECTION_STATE_SLOW;
        currentSelectedSpell = SELECTED_SPELL_NONE;
    }

    public static short SELECTED_SPELL_NONE = -1;
    public static short SELECTED_SPELL_1 = 0;
    public static short SELECTED_SPELL_2 = 1;
    public static short SELECTED_SPELL_3 = 2;
    public static short SELECTED_SPELL_4 = 3;
    public short currentSelectedSpell = -1;

    public bool isAnySpellSelected {
        get {
            return currentSelectedSpell != SELECTED_SPELL_NONE;
        }
    }

    List<Cell> spellRangeCells = null;

    private void buttonSpellHandler(short spellIndex) {
        currentSelectedSpell = (short)(spellIndex - BUTTON_TYPE_SPELL_0);
        // display spell range preview
        if(spellRangeCells != null) {
            grid.SetCellColor(spellRangeCells, Color.white);
            spellRangeCells = null;
            if (effectRangeCells != null) {
                grid.SetCellColor(effectRangeCells, Color.white);
                effectRangeCells = null;
            }
        }

        int rangeType = localSpells[currentSelectedSpell].rangeType;
        int minRange = localSpells[currentSelectedSpell].minRange;
        int maxRange = localSpells[currentSelectedSpell].maxRange;

        spellRangeCells = grid.getCellsInRange(grid.GetCell(localEntity.currentCellId), minRange, maxRange, rangeType);

        grid.SetCellColor(spellRangeCells, Color.blue);
    }

    private Cell currentTargetCell = null;
    List<Cell> effectRangeCells = null;

    public void targetAction(Cell target) {
        //if (isAiming) {
        //    if (isSpelling) {
                if (isAnySpellSelected) {
            if(spellRangeCells.Contains(target)) {
                currentTargetCell = target;
                // show spell area
                if (effectRangeCells != null) {
                    grid.SetCellColor(effectRangeCells, Color.white);
                    effectRangeCells = null;
                }
                int areaType = localSpells[currentSelectedSpell].areaType;
                int minArea = localSpells[currentSelectedSpell].minArea;
                int maxArea = localSpells[currentSelectedSpell].maxArea;

                effectRangeCells = grid.getCellsInRange(target, minArea, maxArea, areaType);
                grid.SetCellColor(effectRangeCells, Color.red);

            }

        }
         //   } else if (isMoving) {
         //       currentTargetCell = target;
                // register movement
        //    }
        //}
    }

    private Action currentAction = null;

    private void registerSpellAction(string spellId, Cell target) {
        if (isQuickSpelling) {
            QuickSpellAction tempAction = new QuickSpellAction();
            tempAction.spellId = spellId;
            tempAction.targetCellId = target.cellId;
            currentAction = tempAction;
        } else if (isSlowSpelling) {
            SlowSpellAction tempAction = new SlowSpellAction();
            tempAction.spellId = spellId;
            tempAction.targetCellId = target.cellId;
            currentAction = tempAction;
        }
        currentAction.entityId = localEntity.entityId;
    }

    private void registerMovementAction(Cell target) {
        MovementAction tempAction = new MovementAction();
        tempAction.path = new int[] { target.cellId };
        currentAction = tempAction;
        currentAction.entityId = localEntity.entityId;
    }

    private void buttonConfirmHandler() {
        if (isAiming) {
            if (isSpelling) {
                if (isAnySpellSelected) {
                    SpellInstance spellInstance = localSpells[currentSelectedSpell];
                    registerSpellAction(spellInstance.spell.id, currentTargetCell);
                }
            } else if (isMoving) {
                registerMovementAction(currentTargetCell);
            }
        }
    }

    private void buttonCancelHandler() {
        if(localActions.Count > 0) {
            localActions.Dequeue();
        }
    }

    private void buttonReadyHandler() {

    }

    public override string generateTurnActionsJSON() {
        string output = "";
        output += "\"actions\":" + "[{";
        int i = 0;
        while (localActions.Count > 0) {
            Action action = localActions.Dequeue();
            output += "\"action" + i + "\":" + action.toJSON();
            if (localActions.Count > 0)
                output += ",";
        }
        output += "}]";
        return "{" + output + "}";
    }
}
