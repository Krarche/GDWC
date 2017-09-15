using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;
using Tools;
using Tools.JSON;

namespace Logic {

    public abstract class GameLogic {

        public static float TURN_DURATION_SECONDS = 15;
        public static float SERVER_WAIT_PLAYER_ACTIONS_SECONDS = 2;
        public static float CLIENT_WAIT_SERVER_SYNC_SECONDS = 4;
        public static int MAX_TURN_NUMBER = 20;
        public static int MAX_APT = 3; // max actions per turn

        public ulong gameId;
        public int currentTurn;

        public double startTurnTimeRemaining {
            get {
                DateTime now = DateTime.UtcNow;
                TimeSpan nowToStart = startTurnDate.Subtract(now);
                return nowToStart.TotalSeconds;
            }
        }
        public double endTurnTimeRemaining {
            get {
                DateTime now = DateTime.UtcNow;
                TimeSpan nowToEnd = endTurnDate.Subtract(now);
                return nowToEnd.TotalSeconds;
            }
        }
        public double serverDataTimeoutTimeRemaining {
            get {
                DateTime now = DateTime.UtcNow;
                TimeSpan serverDataTimeout = serverDataTimeoutDate.Subtract(now);
                return serverDataTimeout.TotalSeconds;
            }
        }

        public int startTurnSecondRemaining {
            get { return (int)startTurnTimeRemaining; }
        }
        public int endTurnSecondRemaining {
            get { return (int)endTurnTimeRemaining; }
        }
        public int serverDataTimeoutSecondRemaining {
            get { return (int)serverDataTimeoutTimeRemaining; }
        }

        public bool isActionRegistrationAllowed {
            get { return ActionRegistrationAllowed(); }
        }
        protected virtual bool ActionRegistrationAllowed() {
            return currentTurnState == TURN_STATE_ACT;
        }

        public DateTime startTurnDate;
        public DateTime endTurnDate;
        public DateTime serverDataTimeoutDate;
        public string mapId = "NULL";

        public Dictionary<ulong, Player> players = new Dictionary<ulong, Player>();
        public GameObject playerPrefab; // temporary
        public Player getPlayerByUserId(ulong userId) {
            return players.Where(x => x.Value.user.userId == userId).First().Value;
        }

        public Dictionary<int, Entity> entityList = new Dictionary<int, Entity>();
        public int lastEntityIdGenerated = -1;

        public Grid grid;
        public TurnResolution solver;

        public List<Data.Action> actions0 = new List<Data.Action>();
        public List<Data.Action> actions1 = new List<Data.Action>();
        public List<Data.Action> actions2 = new List<Data.Action>();

        public GameLogic() {
            playerPrefab = Resources.Load<GameObject>("Prefabs/PlayerPrefab");
            solver = new TurnResolution(this);
            mapId = "M002";
            grid = new Grid(this, DataManager.MAP_DATA[mapId]);
        }
        public GameLogic(string mapId) {
            playerPrefab = Resources.Load<GameObject>("Prefabs/PlayerPrefab");
            solver = new TurnResolution(this);
            this.mapId = mapId;
            grid = new Grid(this, DataManager.MAP_DATA[mapId]);
        }

        public Entity createEntity() {
            Entity entity = GameObject.Instantiate(playerPrefab, new Vector3(), Quaternion.identity).GetComponent<Entity>();
            entity.entityId = lastEntityIdGenerated++;
            entity.game = this;
            entity.grid = grid;
            entity.setCurrentCell(grid.GetCell(0));
            entityList[lastEntityIdGenerated] = entity;
            return entity;
        }

        public void removeEntity(Entity e) {
            GameObject.Destroy(e.gameObject);
        }

        public void spawnPlayer(Player p) {
            if (!players.ContainsKey(p.playerId)) {
                p.playerEntity = createEntity();
                p.playerEntity.setColor(p.playerColor);
                p.playerEntity.setDisplayedName(p.playerName); // Temporary userName = playerName
                int cellId = grid.mapData.getSpawns(2)[players.Count];
                Cell cell = grid.GetCell(cellId);
                p.playerEntity.setCurrentCell(cell);
                players[p.playerId] = p;
            }
        }

        public void removePlayer(Player p) {
            removeEntity(p.playerEntity);
            entityList.Remove(p.playerEntity.entityId);
            players.Remove(p.playerId);
        }

        public const short TURN_STATE_NONE = -1;
        public const short TURN_STATE_PREP = 0;
        public const short TURN_STATE_ACT = 1;
        public const short TURN_STATE_SYNC = 2;
        public const short TURN_STATE_RES = 3;
        public short currentTurnState = -1;

        public bool isPreparing {
            get { return currentTurnState == TURN_STATE_PREP; }
        }
        public bool isActing {
            get { return currentTurnState == TURN_STATE_ACT; }
        }
        public bool isSynchronizing {
            get { return currentTurnState == TURN_STATE_SYNC; }
        }
        public bool isResolving {
            get { return currentTurnState == TURN_STATE_RES; }
        }

        public virtual void prepareNewTurn(long startTurnTimestamp) {
            currentTurn++;
            currentTurnState = TURN_STATE_PREP;
            startTurnDate = DateTime.FromFileTimeUtc(startTurnTimestamp);
            endTurnDate = startTurnDate.AddSeconds(TURN_DURATION_SECONDS);
            serverDataTimeoutDate = endTurnDate.AddSeconds(CLIENT_WAIT_SERVER_SYNC_SECONDS);
            CoroutineMaster.startCoroutine(waitForTurnStart());
        }

        protected IEnumerator waitForTurnStart() {
            DateTime now = DateTime.UtcNow;
            TimeSpan nowToStart = startTurnDate.Subtract(now);

            yield return new WaitForSecondsRealtime((float)nowToStart.TotalSeconds);
            startTurn();
        }

        public virtual void startTurn() {
            // todo : displace in turn resolution - start
            foreach (Entity e in entityList.Values) {
                solver.resolveEntityBuffs(e, "onTurnStartHandler");
            }
            currentTurnState = TURN_STATE_ACT;
            CoroutineMaster.startCoroutine(waitForTurnEnd());
        }

        protected IEnumerator waitForTurnEnd() {
            DateTime now = DateTime.UtcNow;
            TimeSpan nowToEnd = endTurnDate.Subtract(now);

            yield return new WaitForSecondsRealtime((float)nowToEnd.TotalSeconds);
            if (isActing)
                endTurn();
        }

        public virtual void endTurn() {
            // todo : displace in turn resolution - end
            foreach (Entity e in entityList.Values) {
                solver.resolveEntityBuffs(e, "onTurnEndHandler");
            }
            currentTurnState = TURN_STATE_SYNC;
            CoroutineMaster.startCoroutine(waitForServerData());
        }

        protected IEnumerator waitForServerData() {
            DateTime now = DateTime.UtcNow;
            TimeSpan nowToTimeout = serverDataTimeoutDate.Subtract(now);
            yield return new WaitForSecondsRealtime((float)nowToTimeout.TotalSeconds);
            if (isSynchronizing)
                resolveTurn();
        }

        // override for client and server
        public virtual void resolveTurn() {
            if (!isResolving) {
                currentTurnState = TURN_STATE_RES;
                // resolve action
                // notify server turn ended on client
                long timestamp = DateTime.UtcNow.AddSeconds(5).ToFileTimeUtc();
                prepareNewTurn(timestamp);
            }
        }

        public void receiveActions(string actions) {
            ObjectJSON actionsJSON = new ObjectJSON(actions);
            ArrayJSON actions0JSON = actionsJSON.getArrayJSON("actions0");
            for (int i = 0; i < actions0JSON.Length; i++) {
                actions0.Add(Data.Action.fromJSON(actions0JSON.getObjectJSONAt(i)));
            }
            ArrayJSON actions1JSON = actionsJSON.getArrayJSON("actions1");
            for (int i = 0; i < actions1JSON.Length; i++) {
                actions1.Add(Data.Action.fromJSON(actions1JSON.getObjectJSONAt(i)));
            }
            ArrayJSON actions2JSON = actionsJSON.getArrayJSON("actions2");
            for (int i = 0; i < actions2JSON.Length; i++) {
                actions2.Add(Data.Action.fromJSON(actions2JSON.getObjectJSONAt(i)));
            }
        }

        public virtual void resolveActions(List<Data.Action> actions) {
            List<Data.Action> fast = new List<Data.Action>();
            List<Data.Action> move = new List<Data.Action>();
            List<Data.Action> slow = new List<Data.Action>();

            foreach (Data.Action action in actions) {
                switch (action.getPriority()) {
                    case 0:
                        fast.Add(action);
                        break;
                    case 1:
                        move.Add(action);
                        break;
                    default:
                        slow.Add(action);
                        break;
                }
                resolvePriority(fast);
                resolvePriority(move);
                resolvePriority(slow);
            }
        }

        public virtual void resolvePriority(List<Data.Action> priority) {
            foreach(Data.Action a in priority) {
                resolveAction(a);
            }
        }

        public virtual void resolveAction(Data.Action action) {
            Entity e = entityList[action.entityId];
            if (action is MovementAction) {
                if (!action.isActionSkiped()) {
                    MovementAction movementACtion = (MovementAction)action;
                    solver.resolveMovement(e, movementACtion.path);
                } // TODO else give MP
            } else if (action is SpellAction) {
                if (!action.isActionSkiped()) {
                    SpellAction spellAction = (SpellAction)action;
                    Cell target = grid.GetCell(spellAction.targetCellId);
                    SpellData spell = DataManager.SPELL_DATA[spellAction.spellId];
                    solver.resolveSpell(spell, e, target, spellAction is QuickSpellAction);
                } // TODO else give AP
            }
        }

        public abstract void synchronizeActions();

        public abstract string generateTurnActionsJSON();
    }
}