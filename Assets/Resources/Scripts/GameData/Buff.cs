using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Buff : GameData {

    public string name;
    public string iconPath;
    public string description;
    public EffectBuff[] effects;
}

public class BuffInstance {
    Buff type;
    Entity origin;
    Entity target;

    int remainingDuration;
    int remainingCharges;
}
