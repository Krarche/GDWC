using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data {

    public class Player {

        public User user;

        public static ulong playerCount = 0;
        public ulong playerId;

        public string playerName = "";
        public Color playerColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        public Entity playerEntity = null;

        public int teamId;
        public Team team;

        public bool isReady = false; // to begin game and end turn

        public bool quickActionUsed = false;
        public bool movementActionUsed = false;
        public bool slowActionUsed = false;
        public bool ultimateActionUsed = false;

        public Player() {
            playerId = playerCount;
            playerCount++;
        }
        public Player(ulong playerId, Entity e) {
            this.playerId = playerId;
            this.playerEntity = e;
        }

        public void addOrder(Action o) {
            playerEntity.addOrder(o);
        }
        public int getCurrentCellId() {
            return playerEntity.currentCellId;
        }

        public void restartTurn() {
            quickActionUsed = false;
            movementActionUsed = false;
            slowActionUsed = false;
            ultimateActionUsed = false;
        }

    }
}