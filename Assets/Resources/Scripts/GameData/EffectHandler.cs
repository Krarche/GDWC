using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EffectHandler {
    public virtual void effect(Entity e) {

    }
    public virtual void effect(Cell e) {

    }
}

public class EffectHandlerDamage : EffectHandler {
    int damage;
    public override void effect(Entity e) {
        // e.damage(damage);
    }
}

public class EffectHandlerHeal : EffectHandler {
    int heal;
    public override void effect(Entity e) {
        // e.heal(heal);
    }
}

public class EffectHandlerBuff : EffectHandler {
    // Buff buff
    public override void effect(Entity e) {
        // e.addBuff(buff);
    }
}

public class EffectHandlerModAP : EffectHandler {
    int AP;
    public override void effect(Entity e) {
        e.modAP(AP);
    }
}

public class EffectHandlerModMP : EffectHandler {
    int MP;
    public override void effect(Entity e) {
        e.modMP(MP);
    }
}

public class EffectHandlerModRange : EffectHandler {
    int range;
    public override void effect(Entity e) {
        e.rangeModifier += range;
    }
}

public class EffectHandlerStun : EffectHandler {
    public override void effect(Entity e) {
        e.stun();
    }
}

public class EffectHandlerUnstun : EffectHandler {
    public override void effect(Entity e) {
        e.unstun();
    }
}


