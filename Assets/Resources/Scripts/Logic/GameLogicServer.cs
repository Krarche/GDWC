using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Data;
using System;
using Tools;
using System.Collections;

namespace Logic {

    public class GameLogicServer : GameLogic {

        private static float TURN_PREPARATION_DURATION_SECONDS = 4;
        public static ulong gameCount = 0;

        public int preparingPlayersNumber;
        public void registerPlayerReady(Player p) {
            if (!isPreparing) {
                if (!p.isReady) {
                    p.isReady = true;
                    preparingPlayersNumber--;
                    if (isTurnReady) { // all player ready, start turn !
                        DateTime startTurn = DateTime.UtcNow.AddSeconds(TURN_PREPARATION_DURATION_SECONDS);
                        long startTurnTimestamp = startTurn.ToFileTimeUtc();
                        Network.NetworkMasterServer.singleton.ServerStartTurnMessage(this, startTurnTimestamp);
                        prepareNewTurn(startTurnTimestamp);
                    }
                    // ELSE - still need to wait for players, just wait
                } else {
                    Debug.LogWarning("Try to registerPlayerReady(), but player was ready!");
                }
            } else {// too late for ready 
                Debug.LogWarning("Try to registerPlayerReady(), but turn already prepared!");
            }
        }
        public bool isTurnReady {
            get { return preparingPlayersNumber == 0; }
        }

        public int actingPlayerNumber;
        public void registerPlayerAction(Player p, string actions) {
            if (isActing || isSynchronizing) {
                if (!p.isReady) {
                    p.isReady = true;
                    actingPlayerNumber--;
                    receiveActions(actions);
                    if (isSyncReady) {// all player registered, sync !
                        synchronizeActions();
                        resolveTurn();
                    }
                    // ELSE - still need to wait for players, just wait
                } else {
                    Debug.LogWarning("Try to registerPlayerAction(), but player actions were already registered!");
                }
            } else { // too late for sync 
                Debug.LogWarning("Try to registerPlayerAction(), but turn is already resolving!");
            }
        }
        public bool isSyncReady {
            get { return actingPlayerNumber == 0 || endTurnTimeRemaining < 0; }
        }


        public static GameLogicServer createGame() {
            GameLogicServer game = new GameLogicServer();
            game.gameId = gameCount++;
            return game;
        }

        private GameLogicServer() : base() {
            foreach (Cell c in grid.cells)
                c.inWorld.gameObject.SetActive(false);
        }


        public override Entity createEntity() {
            Entity entity = base.createEntity();
            entity.gameObject.SetActive(false);
            return entity;
        }

        public override void prepareNewTurn(long startTurnTimestamp) {
            base.prepareNewTurn(startTurnTimestamp);
            serverDataTimeoutDate = endTurnDate.AddSeconds(SERVER_WAIT_PLAYER_ACTIONS_SECONDS);
        }

        public override void startTurn() {
            actingPlayerNumber = players.Count();
            foreach (Player p in players.Values)
                p.isReady = false;
            base.startTurn();
        }

        public override void endTurn() {
            base.endTurn();
            // wait for player ready
            preparingPlayersNumber = players.Count;
            foreach (Player p in players.Values)
                p.isReady = false;
            CoroutineMaster.startCoroutine(waitForClientData());
        }

        protected IEnumerator waitForClientData() {
            DateTime now = DateTime.UtcNow;
            TimeSpan nowToTimeout = serverDataTimeoutDate.Subtract(now);
            yield return new WaitForSecondsRealtime((float)nowToTimeout.TotalSeconds);
            if (isSynchronizing) {
                synchronizeActions();
                resolveTurn();
            }
        }

        public override void resolveTurn() {
            if (!isResolving) {
                currentTurnState = TURN_STATE_RES;
                // resolve actions
                resolveActions(actions0);
                resolveActions(actions1);
                resolveActions(actions2);
                // wait for player ready
                preparingPlayersNumber = players.Count;
                foreach (Player p in players.Values)
                    p.isReady = false;
                waitForEnd();
            }
        }

        public override void synchronizeActions() {
            Network.NetworkMasterServer.singleton.SyncTurnActions(this);
        }

        public override string generateTurnActionsJSON() {
            string output = "";
            output += "\"actions0\":" + actionListToArray(actions0);
            output += ",";
            output += "\"actions1\":" + actionListToArray(actions1);
            output += ",";
            output += "\"actions2\":" + actionListToArray(actions2);
            return "{" + output + "}";
        }

        public string actionListToArray(List<Data.Action> actions) {
            if (actions.Count == 0)
                return "[]";
            string output = "";
            Data.Action last = actions.Last();
            foreach (Data.Action action in actions) {
                output += action.toJSON();
                if (action != last)
                    output += ",";
            }
            return "[" + output + "]";
        }
    }
}