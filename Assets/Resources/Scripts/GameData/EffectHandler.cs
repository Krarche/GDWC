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
    public int damage;
    public override void effect(Entity e) {
        // e.damage(damage);
    }
}

public class EffectHandlerHeal : EffectHandler {
    public int heal;
    public override void effect(Entity e) {
        // e.heal(heal);
    }
}

public class EffectHandlerBuff : EffectHandler {
    // Buff buff
    public string buffId;
    public int duration;
    public override void effect(Entity e) {
        // e.addBuff(buff);
    }
}

public class EffectHandlerModAP : EffectHandler {
    public int AP;
    public int direction = 1;
    public override void effect(Entity e) {
        e.modAP(AP);
    }
}

public class EffectHandlerModMP : EffectHandler {
    public int MP;
    public int direction = 1;
    public override void effect(Entity e) {
        e.modMP(MP);
    }
}

public class EffectHandlerModRange : EffectHandler {
    public int range;
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

public class EffectHandlerPush : EffectHandler {
    // dir
    public override void effect(Entity e) {
        //e.push(dir);
    }
}

public class EffectHandlerDash : EffectHandler {
    // dir
    public override void effect(Entity e) {
        //e.push(dir);
    }
}

public class EffectHandlerWarp : EffectHandler {
    // destination
    public override void effect(Entity e) {
        //e.setCurrentCell(destination);
    }
}


