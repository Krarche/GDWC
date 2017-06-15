using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff {
    List<EffectBuff> effectList;

    public int buffMaxDurationTime;
}

public class BuffInstance {
    Buff type;
    Entity origin;
    Entity target;

    int remainingDuration;
    int remainingCharges;
}
