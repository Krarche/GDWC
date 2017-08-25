using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogicClient : GameLogic {

    public static GameLogicClient game;

    public GameLogicClient(string mapId): base(mapId) {
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

    public const short BUTTON_TYPE_SPELL_0 = 0;
    public const short BUTTON_TYPE_SPELL_1 = 1;
    public const short BUTTON_TYPE_SPELL_2 = 2;
    public const short BUTTON_TYPE_SPELL_3 = 3;
    public const short BUTTON_TYPE_CONFIRM = 4;
    public const short BUTTON_TYPE_CANCEL = 5;

    public void buttonInput(short type) {
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
                buttonConfirm();
                break;
            case BUTTON_TYPE_CANCEL:
                buttonCancel();
                break;
        }
    }


    public static short ACTION_SELECTION_STATE_ROOT = 0;
    public static short ACTION_SELECTION_STATE_QUICK = 1;
    public static short ACTION_SELECTION_STATE_SLOW = 2;
    public static short ACTION_SELECTION_STATE_MOVEMENT = 3;
    public static short currentActionSelectionState = 0;

    private void buttonMovementHandler() {

    }

    private void buttonQuickSpellHandler() {

    }

    private void buttonSlowSpellHandler() {

    }

    public static short SELECTED_SPELL_NONE = -1;
    public static short SELECTED_SPELL_1 = 0;
    public static short SELECTED_SPELL_2 = 1;
    public static short SELECTED_SPELL_3 = 2;
    public static short SELECTED_SPELL_4 = 3;
    public short currentSelectedSpell = -1;

    private void buttonSpellHandler(short spellIndex) {

    }

    private void targetAction(Cell target) {

    }

    private void registerSpellAction() {

    }

    private void registerMovementAction() {

    }

    private void buttonCancel() {

    }

    private void buttonConfirm() {

    }
}
