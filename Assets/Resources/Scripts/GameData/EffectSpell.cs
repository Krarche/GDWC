using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSpell {
    public int[] minArea;
    public int[] maxArea;
    public int[] areaType;

    public bool[] affectAlly = { true , true};
    public bool[] affectEnemy = { true, true };
    public bool[] affectSelf = { true, true };
    public bool[] affectCell = { true, true };

    public EffectCondition[] conditions;

    public EffectHandler quickHandler;
    public EffectHandler slowHandler;
}
