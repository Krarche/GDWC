using Data;
using Logic;
using Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game1v1 : MonoBehaviour {

    public static Game1v1 singleton;

    public GameViewController gameViewController;
    public GameLogicClient gameLogic;

    public Player localPlayer;


    private void Start() {
        LoadGame(SceneMaster.args);
    }

    public void LoadGame(Dictionary<string, object> args) {
        ServerJoinGameResponseMessage msg = (ServerJoinGameResponseMessage)args["msg"];

        if (msg.hasJoined) {
            gameLogic = new GameLogicClient(msg.mapId);
            gameLogic.game1v1 = this;
            gameLogic.gameId = msg.gameId;
            gameLogic.currentTurn = msg.currentTurn;
            NetworkMasterClient.user.currentGameId = gameLogic.gameId;
            for (int i = 0; i < msg.cellIds.Length; i++) {
                ulong playerId = msg.playerIds[i];
                int cellId = msg.cellIds[i];
                int entityId = msg.entityIds[i];
                string displayedName = msg.displayedNames[i];
                float r = msg.r[i];
                float g = msg.g[i];
                float b = msg.b[i];

                Player newPlayer = spawnPlayer(playerId, cellId, entityId, displayedName, r, g, b);
                if (msg.clientPlayerId == playerId) {
                    NetworkMasterClient.user.player = newPlayer;
                    gameLogic.localPlayer = newPlayer;
                    localPlayer = newPlayer;
                    //GUIManager.gui.linkWithLocalEntity(NetworkMasterClient.user.player.playerEntity);
                }
                newPlayer.playerEntity.initSpell(msg.spellIds);
            }
            NetworkMasterClient.singleton.ClientReadyToPlay();
        } else {
            Debug.LogError("Failed to join game.");
        }
    }

    public void QuitGame() {
        Dictionary<string, object> args = new Dictionary<string, object>();
        args["scoreScreen"] = true;
        args["gameMode"] = GameMode.Mode1v1;
        args["winningTeam"] = gameLogic.winningTeam;
        args["losingTeam"] = gameLogic.losingTeam;
        args["team1Entities"] = gameLogic.team1Entities;
        args["team2Entities"] = gameLogic.team2Entities;
        args["killedEntities"] = gameLogic.killedEntities;
        args["totalTurnCount"] = gameLogic.currentTurn;
        SceneMaster.singleton.SwitchToLobby(args);
    }


    // --------------- Players handlers -----------------

    private Player spawnPlayer(ulong playerId, int cellId, int entityId, string displayedName, float r, float g, float b) {
        if (!gameLogic.players.ContainsKey(playerId)) {
            Player p = new Player();
            p.playerId = playerId;
            p.playerName = displayedName;
            p.playerColor = new Color(r, g, b);
            p.teamId = entityId;
            p.team = p.teamId == 0 ? Team.Team1 : Team.Team2;
            gameLogic.spawnPlayer(p, cellId, entityId);
            return p;
        } else {
            return gameLogic.players[playerId];
        }
    }

    private Entity spawnEntity(int cellId, int entityId, string displayedName, float r, float g, float b) {
        if (!gameLogic.entityList.ContainsKey(entityId)) {
            Entity e = gameLogic.spawnEntity(cellId, entityId);
            e.setDisplayedName(displayedName);
            e.setColor(r, g, b);
            return e;
        } else {
            return gameLogic.entityList[entityId];
        }
    }

}
