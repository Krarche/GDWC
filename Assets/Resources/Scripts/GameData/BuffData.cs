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
    public BuffData buff;
    public Entity origin;
    public Entity target;

    public int remainingDuration;
    public int remainingCharges;
}
