using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuffData : GameData {
    
    public string iconPath;
    public string description;
    public EffectBuff[] effects;
}

public class BuffInstance {
    BuffData type;
    Entity origin;
    Entity target;

    int remainingDuration;
    int remainingCharges;
}
