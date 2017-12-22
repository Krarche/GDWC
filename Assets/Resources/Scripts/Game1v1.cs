using Data;
using Logic;
using Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game1v1 : MonoBehaviour {

    public static Game1v1 singleton;

    private void Start() {
        LoadGame(SceneMaster.args);
    }

    public void LoadGame(Dictionary<string, object> args) {
        ServerJoinGameResponseMessage msg = (ServerJoinGameResponseMessage)args["msg"];

        if (msg.hasJoined) {
            GameLogicClient game = new GameLogicClient(msg.mapId);
            game.gameId = msg.gameId;
            game.currentTurn = msg.currentTurn;
            NetworkMasterClient.user.currentGameId = game.gameId;
            for (int i = 0; i < msg.cellIds.Length; i++) {
                ulong playerId = msg.playerIds[i];
                int cellId = msg.cellIds[i];
                int entityId = msg.entityIds[i];
                string displayedName = msg.displayedNames[i];
                float r = msg.r[i];
                float g = msg.g[i];
                float b = msg.b[i];

                Player temp = spawnPlayer(playerId, cellId, entityId, displayedName, r, g, b);
                if (msg.clientPlayerId == playerId) {
                    NetworkMasterClient.user.player = temp;
                    //GUIManager.gui.linkWithLocalEntity(NetworkMasterClient.user.player.playerEntity);
                }
                temp.playerEntity.initSpell(msg.spellIds);
            }
            NetworkMasterClient.singleton.ClientReadyToPlay();
        } else {
            Debug.LogError("Failed to join game.");
        }
    }


    // --------------- Players handlers -----------------

    private Player spawnPlayer(ulong playerId, int cellId, int entityId, string displayedName, float r, float g, float b) {
        if (!GameLogicClient.game.players.ContainsKey(playerId)) {
            Player p = new Player();
            p.playerId = playerId;
            p.playerName = displayedName;
            p.playerColor = new Color(r, g, b);
            GameLogicClient.game.spawnPlayer(p, cellId, entityId);
            return p;
        } else {
            return GameLogicClient.game.players[playerId];
        }
    }

    private Entity spawnEntity(int cellId, int entityId, string displayedName, float r, float g, float b) {
        if (!GameLogicClient.game.entityList.ContainsKey(entityId)) {
            Entity e = GameLogicClient.game.spawnEntity(cellId, entityId);
            e.setDisplayedName(displayedName);
            e.setColor(r, g, b);
            return e;
        } else {
            return GameLogicClient.game.entityList[entityId];
        }
    }

}
