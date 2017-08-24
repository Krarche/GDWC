using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogicServer : GameLogic {

    public static ulong gameCount = 0;

    public static GameLogicServer createGame() {
        GameLogicServer game = new GameLogicServer();
        game.gameId = ++gameCount;
        return game;
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
}