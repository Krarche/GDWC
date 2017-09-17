using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System;
using Data;
using Logic;

namespace Network {

    public class GameQueue {
        List<User> queue = new List<User>();

        Mutex operationOnQueue = new Mutex();
        NetworkMasterServer server;
        public bool forceSoloGame = false;
        public static double DELAY_BEFORE_MATCH_IA = 15.0;

        public bool queueUser(User u) {
            operationOnQueue.WaitOne();
            if (!queue.Contains(u)) {
                u.isQueued = true;
                u.joinedQueueTime = DateTime.UtcNow;
                queue.Insert(queue.Count, u);
                operationOnQueue.ReleaseMutex();
                return true;
            }
            operationOnQueue.ReleaseMutex();
            return false;
        }

        public bool unqueueUser(User u) {
            operationOnQueue.WaitOne();
            if (queue.Contains(u)) {
                u.isQueued = false;
                queue.Remove(u);
                operationOnQueue.ReleaseMutex();
                return true;
            }
            operationOnQueue.ReleaseMutex();
            return false;
        }

        public GameLogicServer checkQueue() {
            operationOnQueue.WaitOne();
            if (queue.Count >= 1) {
                User u2 = queue[queue.Count - 1];
                foreach (User u1 in queue) {
                    if (canMatch(u1, u2)) {
                        // dequeue players
                        queue.Remove(u1);
                        if (!forceSoloGame) // if non solo game
                            queue.Remove(u2);
                        u1.isQueued = false;
                        if (!forceSoloGame) // if non solo game
                            u2.isQueued = false;

                        // create game
                        GameLogicServer newGame = GameLogicServer.createGame();
                        u1.player = CreatePlayer(u1, newGame);
                        if (!forceSoloGame) // if non solo game
                            u2.player = CreatePlayer(u2, newGame);
                        
                        operationOnQueue.ReleaseMutex();
                        return newGame;
                    } else if (u1.timeSpentInQueue > DELAY_BEFORE_MATCH_IA) { // too long, match with IA

                    }
                }
            }
            operationOnQueue.ReleaseMutex();
            return null;
        }

        private Player CreatePlayer(User u, GameLogicServer game) {
            Player p = new Player();
            u.player = p;
            p.user = u;
            p.playerId = u.userId;
            p.playerName = u.userName;
            game.createPlayer(p);
            game.preparingPlayersNumber++;
            return p;
        }

        private static int MAX_MMR_DISTANCE = 200;

        private bool canMatch(User u1, User u2) {
            if (u1 == u2)
                return forceSoloGame;
            int mmr1 = u1.MMR;
            int mmr2 = u2.MMR;
            return Mathf.Abs(mmr1 - mmr2) < MAX_MMR_DISTANCE;
        }
    }
}
