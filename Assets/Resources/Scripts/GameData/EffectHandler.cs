using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EffectHandler {
    public virtual void effect(Entity e) {

    }
    public virtual void effect(Cell e) {

    }
}

public class EffectHandlerDirectDamage : EffectHandler {
    public int damage;
}

public class EffectHandlerIndirectDamage : EffectHandler {
    public int damage;
}

public class EffectHandlerHeal : EffectHandler {
    public int heal;
}

public class EffectHandlerBuff : EffectHandler {
    // Buff buff
    public string buffId;
    public int duration;
}

public class EffectHandlerModAP : EffectHandler {
    public int AP;
    public int direction = 1;
}

public class EffectHandlerModMP : EffectHandler {
    public int MP;
    public int direction = 1;
}

public class EffectHandlerModRange : EffectHandler {
    public int range;
}

public class EffectHandlerStun : EffectHandler {
}

public class EffectHandlerUnstun : EffectHandler {
}

public class EffectHandlerPush : EffectHandler {
}

public class EffectHandlerDash : EffectHandler {
}

public class EffectHandlerWarp : EffectHandler {
}


