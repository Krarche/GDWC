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
    public const short BUTTON_TYPE_READY = 22;

    public void buttonInput(short type) {
        Debug.Log(type);
        switch (type) {
            case BUTTON_TYPE_ACTION_ROOT:
                buttonRootHandler();
                break;
            case BUTTON_TYPE_ACTION_MOVEMENT:
                buttonMovementHandler();
                break;
            case BUTTON_TYPE_ACTION_QUICK_SPELL:
                buttonQuickSpellHandler();
                break;
            case BUTTON_TYPE_ACTION_SLOW_SPELL:
                buttonSlowSpellHandler();
                break;
            case BUTTON_TYPE_SPELL_0:
                buttonSpellHandler((short)(type - BUTTON_TYPE_SPELL_0));
                break;
            case BUTTON_TYPE_SPELL_1:
                buttonSpellHandler((short)(type - BUTTON_TYPE_SPELL_0));
                break;
            case BUTTON_TYPE_SPELL_2:
                buttonSpellHandler((short)(type - BUTTON_TYPE_SPELL_0));
                break;
            case BUTTON_TYPE_SPELL_3:
                buttonSpellHandler((short)(type - BUTTON_TYPE_SPELL_0));
                break;
            case BUTTON_TYPE_CONFIRM:
                buttonConfirmHandler();
                break;
            case BUTTON_TYPE_CANCEL:
                buttonCancelHandler();
                break;
            case BUTTON_TYPE_READY:
                buttonReadyHandler();
                break;
        }
    }

    public static short ACTION_SELECTION_STATE_ROOT = 0;
    public static short ACTION_SELECTION_STATE_QUICK = 1;
    public static short ACTION_SELECTION_STATE_SLOW = 2;
    public static short ACTION_SELECTION_STATE_MOVEMENT = 3;
    public static short ACTION_SELECTION_STATE_READY = 4;
    public short currentActionSelectionState = 0;

    public bool canQuickSpell;
    public bool canSlowSpell;
    public bool canMove;

    public bool isAiming {
        get {
            return isMoving || isSpelling;
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
    public SpellInstance selectedSpell {
        get {
            if (currentSelectedSpell != SELECTED_SPELL_NONE) {
                return localSpells[currentSelectedSpell];
            }
            return null;
        }
    }

    private Cell currentTargetCell = null;
    List<Cell> movementRangeCells = null;
    List<Cell> movementPathCells = null;
    List<Cell> spellRangeCells = null;
    List<Cell> effectRangeCells = null;
    private Action currentAction = null;

    private void buttonRootHandler() {
        if (currentActionSelectionState != ACTION_SELECTION_STATE_ROOT) {
            if (isSpelling)
                clearSpellRangeCells();
            else if (isMoving)
                clearMovementRangeCells();
            currentActionSelectionState = ACTION_SELECTION_STATE_ROOT;
        }
    }

    private void buttonMovementHandler() {
        if (currentActionSelectionState != ACTION_SELECTION_STATE_MOVEMENT) {
            if (isSpelling)
                clearSpellRangeCells();
            else if (isMoving)
                clearMovementRangeCells();

            movementRangeCells = grid.getCellsInRange(grid.GetCell(localEntity.currentCellId), Math.Min(1, localEntity.currentMP), Math.Min(3, localEntity.currentMP), SpellData.RANGE_AREA_CIRCLE);
            grid.SetCellColor(movementRangeCells, Color.green);

            currentActionSelectionState = ACTION_SELECTION_STATE_MOVEMENT;
        }
    }
    private void buttonQuickSpellHandler() {
        if (currentActionSelectionState != ACTION_SELECTION_STATE_QUICK) {
            if (isMoving)
                clearMovementRangeCells();
            else if (isSpelling)
                clearSpellRangeCells();
            currentActionSelectionState = ACTION_SELECTION_STATE_QUICK;
        }
    }
    private void buttonSlowSpellHandler() {
        if (currentActionSelectionState != ACTION_SELECTION_STATE_SLOW) {
            if (isMoving)
                clearMovementRangeCells();
            else if (isSpelling)
                clearSpellRangeCells();
            currentActionSelectionState = ACTION_SELECTION_STATE_SLOW;
        }
    }

    private void clearSpellRangeCells() {
        clearEffectRangeCells();
        if (spellRangeCells != null) {
            grid.SetCellColor(spellRangeCells, Color.white);
            spellRangeCells = null;
        }
        currentSelectedSpell = SELECTED_SPELL_NONE;
    }
    private void clearEffectRangeCells() {
        if (effectRangeCells != null) {
            grid.SetCellColor(effectRangeCells, Color.white);
            effectRangeCells = null;
        }
        currentTargetCell = null;
    }
    private void clearMovementRangeCells() {
        if (movementRangeCells != null) {
            grid.SetCellColor(movementRangeCells, Color.white);
            movementRangeCells = null;
        }
        clearMovementPathCells();
    }
    private void clearMovementPathCells() {
        if (movementPathCells != null) {
            grid.SetCellColor(movementPathCells, Color.white);
            movementPathCells = null;
        }
        currentTargetCell = null;
    }

    private void buttonSpellHandler(short spellIndex) {
        if (isSpelling) {
            // clear previous spell preview
            clearSpellRangeCells();

            // switch to selected spell
            currentSelectedSpell = spellIndex;

            // display spell range preview
            int priority = isQuickSpelling ? 0 : 1;

            int rangeType = localSpells[currentSelectedSpell].getRangeType(priority);
            int minRange = localSpells[currentSelectedSpell].getMinRange(priority);
            int maxRange = localSpells[currentSelectedSpell].getMaxRange(priority);

            spellRangeCells = grid.getCellsInRange(grid.GetCell(localEntity.currentCellId), minRange, maxRange, rangeType);
            grid.SetCellColor(spellRangeCells, Color.blue);
        }
    }

    public void targetAction(Cell target) {
        //if (isAiming) {
        if (isSpelling) {
            if (isAnySpellSelected) {
                if (spellRangeCells != null && spellRangeCells.Contains(target)) {
                    // clear previous effect preview
                    clearEffectRangeCells();

                    // switch to targeted cell
                    currentTargetCell = target;

                    // display spell effect preview
                    int priority = isQuickSpelling ? 0 : 1;
                    int[] areaType = selectedSpell.getAreaType(priority);
                    int[] minArea = selectedSpell.getMinArea(priority);
                    int[] maxArea = selectedSpell.getMaxArea(priority);

                    effectRangeCells = grid.getCellsInRanges(target, minArea, maxArea, areaType);
                    grid.SetCellColor(effectRangeCells, Color.red);
                }

            }
        } else if (isMoving) {
            if (movementRangeCells != null && movementRangeCells.Contains(target)) {
                // clear previous path preview
                clearMovementPathCells();

                // switch to selected path
                currentTargetCell = target;

                // display movement path preview
                movementPathCells = new List<Cell>();
                movementPathCells.Add(grid.GetCell(localEntity.currentCellId));
                movementPathCells.Add(currentTargetCell);
                grid.SetCellColor(movementPathCells, Color.yellow);
                // register movement
            }
        }
    }

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
        resolveAction(currentAction);
    }
    private void registerMovementAction(List<Cell> target) {
        MovementAction tempAction = new MovementAction();
        tempAction.path = new int[target.Count];
        for (int i = 0; i < target.Count; i++)
            tempAction.path[i] = target[i].cellId;
        currentAction = tempAction;
        currentAction.entityId = localEntity.entityId;
        resolveAction(currentAction);
    }

    private void buttonConfirmHandler() {
        if (isAiming) {
            if (isSpelling) {
                if (isAnySpellSelected) {
                    if (currentTargetCell != null) {
                        SpellInstance spellInstance = localSpells[currentSelectedSpell];
                        registerSpellAction(spellInstance.spell.id, currentTargetCell);
                        buttonRootHandler();
                    }
                }
            } else if (isMoving) {
                if (movementPathCells != null) {
                    registerMovementAction(movementPathCells);
                    buttonRootHandler();
                }
            }
        }
    }
    private void buttonCancelHandler() {
        if (localActions.Count > 0) {
            localActions.Dequeue();
        }
    }
    private void buttonReadyHandler() {

    }

    // ################################################################

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
