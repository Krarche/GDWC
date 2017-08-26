using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSpell {
    public int minArea;
    public int maxArea;
    public int areaType;

    public bool affectAlly = true;
    public bool affectEnemy = true;
    public bool affectSelf = true;
    public bool affectCell = true;

    public EffectCondition[] conditions;

    public EffectHandler quickHandler;
    public EffectHandler slowHandler;
}
