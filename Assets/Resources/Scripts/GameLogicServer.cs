using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogicServer : GameLogic {

    public override void registerAction() {
        throw new NotImplementedException();
    }

    public override void resolveAction(Order o) {
        Entity e = entityList[o.entityId];
        if (o is MovementOrder) {
            MovementOrder mo = (MovementOrder)o;
            e.setCurrentCell(grid.GetCell(mo.cellId));
        }
    }

    public override void resolveTurn() {
        throw new NotImplementedException();
    }
}
