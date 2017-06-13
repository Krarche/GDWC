using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogicServer : GameLogic {

    public GameLogicServer(): base() {

    }

    public override void registerAction() {
        throw new NotImplementedException();
    }

    public override void resolveAction(Order o) {
        base.resolveAction(o);
    }

    public override void resolveTurn() {
        throw new NotImplementedException();
    }
}
