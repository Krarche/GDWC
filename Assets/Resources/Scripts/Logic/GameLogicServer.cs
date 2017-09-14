using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logic {

    public class GameLogicServer : GameLogic {

        public static ulong gameCount = 0;
        public int loadingPlayersNumber;

        public bool isGameReady {
            get { return loadingPlayersNumber == 0; }
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
        }

        public override void resolveTurn() {
            base.resolveTurn();
        }
    }
}