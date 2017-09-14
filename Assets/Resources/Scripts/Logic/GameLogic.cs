using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;
using Tools;

namespace Logic {

    public abstract class GameLogic {

        public static float TURN_DURATION_SECONDS = 5;
        public static float SERVER_DATA_TIMEOUT_SECONDS = 2;
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
        public int serverDataTimeoutSccondRemaining {
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

        public Dictionary<int, Entity> entityList = new Dictionary<int, Entity>();
        public int lastEntityIdGenerated = -1;

        public Grid grid;
        public TurnResolution solver;

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

        // called when client received


        public const short TURN_STATE_NONE = -1;
        public const short TURN_STATE_PREP = 0;
        public const short TURN_STATE_ACT = 1;
        public const short TURN_STATE_SYNC = 2;
        public const short TURN_STATE_RES = 3;
        public short currentTurnState = -1;

        public virtual void prepareNewTurn(long startTurnTimestamp) {
            currentTurn++;
            currentTurnState = TURN_STATE_PREP;
            startTurnDate = DateTime.FromFileTimeUtc(startTurnTimestamp);
            endTurnDate = startTurnDate.AddSeconds(TURN_DURATION_SECONDS);
            serverDataTimeoutDate = endTurnDate.AddSeconds(SERVER_DATA_TIMEOUT_SECONDS);
            CoroutineMaster.startCoroutine(waitForTurnStart());
        }



        protected IEnumerator waitForTurnStart() {
            DateTime now = DateTime.UtcNow;
            TimeSpan nowToStart = startTurnDate.Subtract(now);

            yield return new WaitForSecondsRealtime((float)nowToStart.TotalSeconds);
            startTurn();
        }

        public virtual void startTurn() {
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
            endTurn();
        }

        public virtual void endTurn() {
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
            resolveTurn();
        }

        public virtual void resolveTurn() {
            if (currentTurnState != TURN_STATE_RES) {
                currentTurnState = TURN_STATE_RES;
                // resolve action
                // notify server turn ended on client

                prepareNewTurn(DateTime.UtcNow.AddSeconds(5).ToFileTimeUtc());
            }
        }

        public virtual void registerAction(string actions) { // general action registration

        }

        public virtual void resolveActions() {
            Queue<Data.Action> fast = new Queue<Data.Action>();
            Queue<Data.Action> move = new Queue<Data.Action>();
            Queue<Data.Action> slow = new Queue<Data.Action>();

            for (int i = 0; i < MAX_APT; i++) {
                foreach (Player p in players.Values) {
                    if (p.playerEntity.actions.Count > 0) {
                        Data.Action o = p.playerEntity.actions.Dequeue();
                        switch (o.getPriority()) {
                            case 0:
                                fast.Enqueue(o);
                                break;
                            case 1:
                                move.Enqueue(o);
                                break;
                            default:
                                slow.Enqueue(o);
                                break;
                        }
                    }
                }
                resolvePriority(fast);
                resolvePriority(move);
                resolvePriority(slow);
            }
        }

        public virtual void resolvePriority(Queue<Data.Action> q) {
            Data.Action o;
            while (q.Count > 0) {
                o = q.Dequeue();
                resolveAction(o);
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

        public virtual string generateTurnActionsJSON() {
            string output = "";
            return output;
        }
    }
}