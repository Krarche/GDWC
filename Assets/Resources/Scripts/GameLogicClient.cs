using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogicClient : GameLogic {

    public static GameLogicClient game;

    public GameLogicClient(): base() {
        game = this;
    }

    public override void startTurn() {
        base.startTurn();
    }

    public override void registerLocalAction() {

    }

    public override void registerForeignAction() {

    }

    public override void resolveTurn() {
        base.resolveTurn();
    }

    public override void resolveAction(Order o) {
        Entity e = entityList[o.entityId];
        if (o is MovementOrder) {
            MovementOrder mo = (MovementOrder)o;
            e.setCurrentCell(grid.GetCell(mo.cellId));
        }
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
                buttonSpell(type);
                break;
            case BUTTON_TYPE_SPELL_1:
                buttonSpell(type);
                break;
            case BUTTON_TYPE_SPELL_2:
                buttonSpell(type);
                break;
            case BUTTON_TYPE_SPELL_3:
                buttonSpell(type);
                break;
            case BUTTON_TYPE_CONFIRM:
                buttonConfirm();
                break;
            case BUTTON_TYPE_CANCEL:
                buttonCancel();
                break;
        }
    }

    private void buttonSpell(short spellIndex) {

    }

    private void buttonCancel() {

    }

    private void buttonConfirm() {

    }
}
