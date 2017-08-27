using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EffectCondition {
}

public abstract class EffectConditionGlobal : EffectCondition {
}

public class EffectConditionTurnNumberAbove : EffectConditionGlobal {
    public int turnNumber;
}
public class EffectConditionTurnNumberBelow : EffectConditionGlobal {
    public int turnNumber;
}

public abstract class EffectConditionTarget : EffectCondition {
    public int target;

    public static int CONDITION_TARGET_ORIGIN = 0;
    public static int CONDITION_TARGET_TARGET = 1;

    public static int stringToConditionTarget(string str) {
        if (str == "ORIGIN")
            return CONDITION_TARGET_ORIGIN;
        if (str == "TARGET")
            return CONDITION_TARGET_TARGET;
        return CONDITION_TARGET_ORIGIN;
    }
}

public class EffectConditionHealthAbove : EffectConditionTarget {
    public int health;
    public bool percent = false;
}
public class EffectConditionHealthBelow : EffectConditionTarget {
    public int health;
    public bool percent = false;
}

public class EffectConditionAPAbove : EffectConditionTarget {
    public int AP;
    public bool percent = false;
}
public class EffectConditionAPBelow : EffectConditionTarget {
    public int AP;
    public bool percent = false;
}

public class EffectConditionMPAbove : EffectConditionTarget {
    public int MP;
    public bool percent = false;
}
public class EffectConditionMPBelow : EffectConditionTarget {
    public int MP;
    public bool percent = false;
}

public class EffectConditionHasBuff : EffectConditionTarget {
    public string buffId = "NONE";
}
public class EffectConditionHasNotBuff : EffectConditionTarget {
    public string buffId = "NONE";
}
