using System.Collections.Generic;
using Tools.JSON;

namespace Data {

    public abstract class Action {
        public int entityId;

        public abstract short getPriority();

        public virtual string toJSON() {
            string output = "";
            output += "\"entityId\":\"" + entityId + "\"";
            return output;
        }

        public virtual bool isActionSkiped() {
            return true;
        }

        public static Action fromJSON(ObjectJSON json) {
            switch (json.getString("class")) {
                case "QuickSpellAction":
                    return QuickSpellAction.fromJSON(json);
                case "MovementAction":
                    return MovementAction.fromJSON(json);
                case "SlowSpellAction":
                    return SlowSpellAction.fromJSON(json);
                default:
                    return null;
            }
        }
    }

    public abstract class SpellAction : Action {
        public string spellId;
        public int targetCellId;

        public override bool isActionSkiped() {
            return spellId == "";
        }

        public override string toJSON() {
            string output = "";
            output += base.toJSON();
            output += ",";
            output += "\"spellId\":\"" + spellId + "\"";
            output += ",";
            output += "\"targetCellId\":\"" + targetCellId + "\"";
            return output;
        }
    }

    public class QuickSpellAction : SpellAction {

        public override short getPriority() {
            return 0;
        }

        public override string toJSON() {
            string output = "";
            output += "\"class\":\"" + "QuickSpellAction" + "\"";
            output += ",";
            output += base.toJSON();
            return "{" + output + "}";
        }

        new public static QuickSpellAction fromJSON(ObjectJSON json) {
            QuickSpellAction output = new QuickSpellAction();
            output.entityId = json.getInt("entityId");
            output.spellId = json.getString("spellId");
            output.targetCellId = json.getInt("targetCellId");
            return output;
        }
    }
    public class SlowSpellAction : SpellAction {

        public override short getPriority() {
            return 2;
        }

        public override string toJSON() {
            string output = "";
            output += "\"class\":\"" + "SlowSpellAction" + "\"";
            output += ",";
            output += base.toJSON();
            return "{" + output + "}";
        }

        new public static SlowSpellAction fromJSON(ObjectJSON json) {
            SlowSpellAction output = new SlowSpellAction();
            output.entityId = json.getInt("entityId");
            output.spellId = json.getString("spellId");
            output.targetCellId = json.getInt("targetCellId");
            return output;
        }
    }

    public class MovementAction : Action {
        public int[] path;

        public override bool isActionSkiped() {
            return path == null || path.Length == 0;
        }

        public override short getPriority() {
            return 1;
        }

        public override string toJSON() {
            string output = "";
            output += "\"class\":\"" + "MovementAction" + "\"";
            output += ",";
            output += base.toJSON();
            output += ",";
            output += "\"path\":\"" + "{"; // start path array
            for (int i = 0; i < path.Length; i++) {
                output += "\"" + path[i] + "\"";
                if (i < path.Length - 1)
                    output += ",";
            }
            output += "}"; // end path array
            return "{" + output + "}";
        }

        new public static MovementAction fromJSON(ObjectJSON json) {
            MovementAction output = new MovementAction();
            output.entityId = json.getInt("entityId");
            ArrayJSON path = json.getArrayJSON("path");
            output.path = new int[path.Length];
            for (int i = 0; i < path.Length; i++) {
                output.path[i] = path.getIntAt(i);
            }
            return output;
        }
    }

    public class MovementUnfolder {
        public Queue<Cell> path;
        public Entity entity;
        bool rotateEntity;
        bool animateEntity;
        bool translateEntity;
    }
}