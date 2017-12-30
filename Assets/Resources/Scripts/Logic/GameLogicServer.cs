using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Data;
using System;
using Tools;
using System.Collections;

namespace Logic {

    public class GameLogicServer : GameLogic {

        public static ulong gameCount = 0;

        public int preparingPlayersNumber;
        public void registerPlayerReady(Player p) {
            if (isDoneResolving || isResolving || isWaitingForGameStart) {
                if (!p.isReady) {
                    p.isReady = true;
                    preparingPlayersNumber--;
                    if (isTurnReady) { // all player ready, start turn !
                        DateTime startTurn = localClock.nowCorrected.AddSeconds(TURN_PREPARATION_DURATION_SECONDS);
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
        private void voidResetTurnReady() {
            foreach (Player p in players.Values) {
                if (p.isReady) {
                    p.isReady = false;
                }
            }
            preparingPlayersNumber = players.Values.Count;

        }


        public int actingPlayerNumber;
        public void registerPlayerAction(Player p, string actions) {
            if (!isSyncReady && (isActing || isWaitingSync)) {
                if (!p.isReady) {
                    receiveActions(actions);
                    p.isReady = true;
                    actingPlayerNumber--;
                    if (isSyncReady) { // all player registered, sync now !
                        synchronizeActions();
                        resolveTurn();
                    }
                    // ELSE - still need to wait for players, just wait
                } else {
                    Debug.LogWarning("Try to registerPlayerAction(), but player actions were already registered!");
                }
            } else { // too late for sync 
                Debug.LogWarning("Try to registerPlayerAction(), but sync is already done!");
            }
        }
        public bool isSyncReady {
            get { return actingPlayerNumber == 0 /*|| endTurnTimeRemaining < 0*/; }
        }
        private void voidResetSyncReady() {
            foreach (Player p in players.Values) {
                if (p.isReady) {
                    p.isReady = false;
                }
            }
            actingPlayerNumber = players.Values.Count;
        }


        public static GameLogicServer createGame() {
            GameLogicServer game = new GameLogicServer();
            game.gameId = gameCount++;
            return game;
        }

        private GameLogicServer() : base() {
            localClock = ClockMaster.serverSingleton;
            foreach (Cell c in grid.cells)
                c.inWorld.gameObject.SetActive(false);
            voidResetTurnReady();
            voidResetSyncReady();
        }

        public override void prepareNewTurn(long startTurnTimestamp) {
            base.prepareNewTurn(startTurnTimestamp);
            voidResetTurnReady();
            voidResetSyncReady();
            serverDataTimeoutDate = endTurnDate.AddSeconds(SERVER_WAIT_PLAYER_ACTIONS_SECONDS);
        }

        public override void startTurn() {
            actingPlayerNumber = players.Count();
            foreach (Player p in players.Values)
                p.isReady = false;
            base.startTurn();
        }

        public override void endTurn() {
            if (isActing) {
                // wait for player ready
                currentTurnState = TURN_STATE_SYNC_WAIT;
                preparingPlayersNumber = players.Count;
                foreach (Player p in players.Values)
                    p.isReady = false;
                CoroutineMaster.startCoroutine(waitForClientsSendData());
            }
        }

        protected IEnumerator waitForClientsSendData() {
            yield return new WaitForSecondsRealtime(SERVER_WAIT_PLAYER_ACTIONS_SECONDS);
            if (isWaitingSync) { // didn't received all client data, but run anywaywith skips
                synchronizeActions();
                resolveTurn();
            }
        }

        public override void resolveTurn() {
            if (isSyncDone) {
                currentTurnState = TURN_STATE_RESOLVING;

                // trigger buff onTurnStartHandler
                foreach (Entity e in entityList.Values) {
                    solver.resolveEntityBuffs(e, "onTurnStartHandler");
                }

                // resolve action
                resolveActions(actions0);
                resolveActions(actions1);
                resolveActions(actions2);

                // trigger buff onTurnEndHandler
                foreach (Entity e in entityList.Values) {
                    solver.resolveEntityBuffs(e, "onTurnEndHandler");
                }

                currentTurnState = TURN_STATE_RESOLVING_DONE;

                if (checkForGameEnd()) {
                    // quit the game
                } else {
                    // wait for player ready
                    preparingPlayersNumber = players.Count;
                    foreach (Player p in players.Values)
                        p.isReady = false;
                }
            }
        }

        public void resolveActions(List<Data.Action> actions) {
            List<Data.Action> fast = new List<Data.Action>();
            List<Data.Action> move = new List<Data.Action>();
            List<Data.Action> slow = new List<Data.Action>();
            foreach (Data.Action action in actions) {
                try {
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
                } catch (Exception e) {
                    Debug.LogError(e.GetType().ToString() + " was catch. Turn resolution is interupted.");
                }
                resolvePriority(fast);
                resolvePriority(move);
                resolvePriority(slow);
            }
            actions.Clear();
        }

        public void resolvePriority(List<Data.Action> priority) {
            foreach (Data.Action a in priority) {
                resolveAction(a);
            }
        }

        public void resolveAction(Data.Action action) {
            Entity e;
            if (!entityList.TryGetValue(action.entityId, out e)) {
                Debug.LogError("resolveAction() - can't find entity with id " + action.entityId);
            }
            if (action is MovementAction) {
                if (!action.isActionSkiped()) {
                    MovementAction movementAction = (MovementAction)action;
                    solver.resolveMovement(e, movementAction.path);
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

        public override void synchronizeActions() {
            currentTurnState = TURN_STATE_SYNC_SEND;
            Network.NetworkMasterServer.singleton.SyncTurnActions(this);
            currentTurnState = TURN_STATE_SYNC_DONE;
        }

        public override string generateTurnActionsJSON() {
            string output = "";
            output += "\"actions0\":" + actionListToArrayJSON(actions0);
            output += ",";
            output += "\"actions1\":" + actionListToArrayJSON(actions1);
            output += ",";
            output += "\"actions2\":" + actionListToArrayJSON(actions2);
            return "{" + output + "}";
        }

        public string actionListToArrayJSON(List<Data.Action> actions) {
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