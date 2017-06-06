﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Order {
    public abstract short getPriority();
}

public class MovementOrder : Order{
    int cellId;

    public MovementOrder(int c) {
        cellId = c;
    }

    public override short getPriority() {
        return 1;
    }
}

public class SpellOrder : Order{
    int spellId;
    int cellId;

    public SpellOrder(int s, int c) {
        spellId = s;
        cellId = c;
    }

    public override short getPriority() {
        return (spellId<1000) ? (short)0 : (short)2;
    }
}