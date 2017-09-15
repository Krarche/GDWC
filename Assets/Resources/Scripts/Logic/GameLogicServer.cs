using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Data;
using System;

namespace Logic {

    public class GameLogicServer : GameLogic {

        public static ulong gameCount = 0;

        public int preparingPlayersNumber;
        public bool registerPlayerReady(Player p) {
            if (!isPreparing)
                return false; // too late for ready 
            if (!p.isReady) {
                p.isReady = true;
                preparingPlayersNumber++;
                if (isTurnReady) {
                    DateTime startTurn = DateTime.UtcNow.AddSeconds(5);
                    long startTurnTimestamp = startTurn.ToFileTimeUtc();
                    prepareNewTurn(startTurnTimestamp);
                    Network.NetworkMasterServer.singleton.ServerStartTurnMessage(this, startTurnTimestamp);
                    return true; // all player ready, start turn !
                }
                return false; // still need to wait for players
            }
            Debug.LogWarning("Try to registerPlayerAction(), but player actions were already registered!");
            return true;
        }
        public bool isTurnReady {
            get { return preparingPlayersNumber == 0; }
        }

        public int waitingPlayerNumber;
        public bool registerPlayerAction(Player p, string actions) {
            if (!isSynchronizing)
                return false; // too late for sync 
            if (!p.isReady) {
                p.isReady = true;
                waitingPlayerNumber++;
                receiveActions(actions);
                if (isSyncReady) {
                    synchronizeActions();
                    resolveTurn();
                    return true; // all player registered, sync !
                }
                return false; // still need to wait for players
            }
            Debug.LogWarning("Try to registerPlayerAction(), but player actions were already registered!");
            return true;
        }
        public bool isSyncReady {
            get { return waitingPlayerNumber == 0 || endTurnTimeRemaining < 0; }
        }


        public static GameLogicServer createGame() {
            GameLogicServer game = new GameLogicServer();
            game.gameId = gameCount++;
            return game;
        }

        private GameLogicServer() : base() {

        }

        public override void startTurn() {
            base.startTurn();
            waitingPlayerNumber = players.Count();
        }

        public override void resolveTurn() {
            base.resolveTurn();
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