using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Order {
    public int entityId;

    public abstract short getPriority();
}

public class MovementOrder : Order{
    public int cellId;

    public MovementOrder(int c, int entityId) {
        this.cellId = c;
        this.entityId = entityId;
    }

    public override short getPriority() {
        return 1;
    }
}

public class SpellOrder : Order{
    public string spellId;
    public int cellId;

    public SpellOrder(string s, int c) {
        spellId = s;
        cellId = c;
    }

    public override short getPriority() {
        return (String.CompareOrdinal(spellId, "S500") < 0) ? (short)0 : (short)2;
    }
}