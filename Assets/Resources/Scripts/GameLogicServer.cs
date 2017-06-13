using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogicServer : GameLogic {

    public GameLogicServer() : base() {

    }

    public override void registerAction() {
        throw new NotImplementedException();
    }

    public override void resolveAction(Order o) {
        Entity e = entityList[o.entityId];
        if (o is MovementOrder) {
            MovementOrder mo = (MovementOrder)o;
            e.setCurrentCell(map.GetCell(mo.cellId));
        }
    }

    public override void resolveTurn() {
        throw new NotImplementedException();
    }
}
