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

        public static float TURN_PREPARATION_DURATION_SECONDS = 2;
        public static float TURN_DURATION_SECONDS = 6;
        public static float SERVER_WAIT_PLAYER_ACTIONS_SECONDS = 2;
        public static float CLIENT_WAIT_SERVER_SYNC_SECONDS = 4;
        public static int MAX_TURN_NUMBER = 20;
        public static int MAX_APT = 3; // max actions per turn

        public ulong gameId;
        public int currentTurn;
        public ClockMaster localClock;

        public List<Entity> killedEntities = new List<Entity>();

        public List<Entity> team1Entities = new List<Entity>();
        public bool team1Defeated {
            get {
                foreach (Entity e in team1Entities)
                    if (e.currentHealth > 0)
                        return false;
                return true;
            }
        }
        public List<Entity> team2Entities = new List<Entity>();
        public bool team2Defeated {
            get {
                foreach (Entity e in team2Entities)
                    if (e.currentHealth > 0)
                        return false;
                return true;
            }
        }

        public bool gameOver {
            get { return team1Defeated || team2Defeated; }
        }

        public Team winningTeam {
            get {
                if (team1Defeated)
                    return Team.Team2;
                if (team2Defeated)
                    return Team.Team1;
                return Team.None;
            }
        }
        public Team losingTeam {
            get {
                if (team1Defeated)
                    return Team.Team1;
                if (team2Defeated)
                    return Team.Team2;
                return Team.None;
            }
        }

        public double startTurnTimeRemaining {
            get {
                DateTime now = localClock.nowCorrected;
                TimeSpan nowToStart = startTurnDate.Subtract(now);
                return nowToStart.TotalSeconds;
            }
        }
        public double endTurnTimeRemaining {
            get {
                DateTime now = localClock.nowCorrected;
                TimeSpan nowToEnd = endTurnDate.Subtract(now);
                return nowToEnd.TotalSeconds;
            }
        }
        public double serverDataTimeoutTimeRemaining {
            get {
                DateTime now = localClock.nowCorrected;
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

        public Data.Grid grid;
        public TurnResolution solver;

        public List<Data.Action> actions0 = new List<Data.Action>();
        public List<Data.Action> actions1 = new List<Data.Action>();
        public List<Data.Action> actions2 = new List<Data.Action>();

        public GameLogic() {
            playerPrefab = Resources.Load<GameObject>("Prefabs/PlayerPrefab");
            solver = new TurnResolution(this);
            mapId = "M002";
            grid = new Data.Grid(this, DataManager.MAP_DATA[mapId]);
        }

        public GameLogic(string mapId) {
            playerPrefab = Resources.Load<GameObject>("Prefabs/PlayerPrefab");
            solver = new TurnResolution(this);
            this.mapId = mapId;
            grid = new Data.Grid(this, DataManager.MAP_DATA[mapId]);
        }

        public void createPlayer(Player p) {
            if (!players.ContainsKey(p.playerId)) {
                int cellId = grid.mapData.getSpawns(2)[players.Count];
                p.playerEntity = createEntity(cellId);
                players[p.playerId] = p;
            }
        }

        public Entity createEntity(int cellId) {
            Entity entity = GameObject.Instantiate(playerPrefab, new Vector3(), Quaternion.identity).GetComponent<Entity>();
            entity.setCurrentCell(grid.GetCell(cellId));
            lastEntityIdGenerated++;
            entity.entityId = lastEntityIdGenerated;
            entity.game = this;
            entity.grid = grid;
            entityList[lastEntityIdGenerated] = entity;
            entity.gameObject.SetActive(false);
            return entity;
        }

        public void spawnPlayer(Player p, int cellId, int entityId) {
            if (!players.ContainsKey(p.playerId)) {
                p.playerEntity = spawnEntity(cellId, entityId);
                p.playerEntity.setColor(p.playerColor);
                p.playerEntity.setDisplayedName(p.playerName); // Temporary userName = playerName
                p.playerEntity.teamId = p.teamId;
                p.playerEntity.team = p.team;
                players[p.playerId] = p;
            }
        }

        public Entity spawnEntity(int cellId, int entityId) {
            Entity entity = GameObject.Instantiate(playerPrefab, new Vector3(), Quaternion.identity).GetComponent<Entity>();
            entity.setCurrentCell(grid.GetCell(cellId));
            entity.entityId = entityId;
            entity.game = this;
            entity.grid = grid;
            entityList[entityId] = entity;
            return entity;
        }

        public void removePlayer(Player p) {
            removeEntity(p.playerEntity);
            players.Remove(p.playerId);
        }

        public void removeEntity(Entity e) {
            GameObject.Destroy(e.gameObject);
            entityList.Remove(e.entityId);
        }

        public const short TURN_STATE_NONE = -1;
        public const short TURN_STATE_PREP = 0;
        public const short TURN_STATE_ACT = 1;
        public const short TURN_STATE_SYNC_SEND = 2;
        public const short TURN_STATE_SYNC_WAIT = 3;
        public const short TURN_STATE_SYNC_DONE = 4;
        public const short TURN_STATE_RESOLVING = 5;
        public const short TURN_STATE_RESOLVING_DONE = 6;
        public short currentTurnState = -1;

        public bool isWaitingForGameStart {
            get { return currentTurnState == TURN_STATE_NONE; }
        }
        public bool isPreparing {
            get { return currentTurnState == TURN_STATE_PREP; }
        }
        public bool isActing {
            get { return currentTurnState == TURN_STATE_ACT; }
        }
        public bool isSendingSync {
            get { return currentTurnState == TURN_STATE_SYNC_SEND; }
        }
        public bool isWaitingSync {
            get { return currentTurnState == TURN_STATE_SYNC_WAIT; }
        }
        public bool isSyncDone {
            get { return currentTurnState == TURN_STATE_SYNC_DONE; }
        }
        public bool isResolving {
            get { return currentTurnState == TURN_STATE_RESOLVING; }
        }
        public bool isDoneResolving {
            get { return currentTurnState == TURN_STATE_RESOLVING_DONE; }
        }

        public virtual void prepareNewTurn(long startTurnTimestamp) {
            currentTurn++;
            currentTurnState = TURN_STATE_PREP;
            startTurnDate = DateTime.FromFileTimeUtc(startTurnTimestamp);
            endTurnDate = startTurnDate.AddSeconds(TURN_DURATION_SECONDS);
            CoroutineMaster.startCoroutine(waitForTurnStart());
        }

        protected IEnumerator waitForTurnStart() {
            DateTime now = localClock.nowCorrected;
            TimeSpan nowToStart = startTurnDate.Subtract(now);

            yield return new WaitForSecondsRealtime((float)nowToStart.TotalSeconds);
            startTurn();
        }

        public virtual void startTurn() {
            currentTurnState = TURN_STATE_ACT;
            CoroutineMaster.startCoroutine(waitForTurnEnd());
        }

        protected IEnumerator waitForTurnEnd() {
            DateTime now = localClock.nowCorrected;
            TimeSpan nowToEnd = endTurnDate.Subtract(now);

            yield return new WaitForSecondsRealtime((float)nowToEnd.TotalSeconds);
            if (isActing)
                endTurn();
        }

        public abstract void endTurn();

        // override for client and server
        public abstract void resolveTurn();


        // will return true if the game is over
        public bool checkForGameEnd() {
            foreach (Entity e in team1Entities) {
                if (e.currentHealth < 1 && !killedEntities.Contains(e))
                    killedEntities.Add(e);
            }
            foreach (Entity e in team2Entities) {
                if (e.currentHealth < 1 && !killedEntities.Contains(e))
                    killedEntities.Add(e);
            }
            return gameOver;
        }

        public virtual void receiveActions(string actions) {
            ObjectJSON actionsJSON = new ObjectJSON(actions);
            ArrayJSON actions0JSON = actionsJSON.getArrayJSON("actions0");
            for (int i = 0; i < actions0JSON.Length; i++) {
                ObjectJSON actionJSON = actions0JSON.getObjectJSONAt(i);
                Data.Action action = Data.Action.fromJSON(actionJSON);
                actions0.Add(action);
            }
            ArrayJSON actions1JSON = actionsJSON.getArrayJSON("actions1");
            for (int i = 0; i < actions1JSON.Length; i++) {
                ObjectJSON actionJSON = actions1JSON.getObjectJSONAt(i);
                Data.Action action = Data.Action.fromJSON(actionJSON);
                actions1.Add(action);
            }
            ArrayJSON actions2JSON = actionsJSON.getArrayJSON("actions2");
            for (int i = 0; i < actions2JSON.Length; i++) {
                ObjectJSON actionJSON = actions2JSON.getObjectJSONAt(i);
                Data.Action action = Data.Action.fromJSON(actionJSON);
                actions2.Add(action);
            }
        }

        public abstract void synchronizeActions();

        public abstract string generateTurnActionsJSON();
    }
}