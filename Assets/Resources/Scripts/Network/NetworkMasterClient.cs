﻿using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.SceneManagement;
using Data;
using Logic;
using Tools;

namespace Network {

    public class NetworkMasterClient : MonoBehaviour {

        public User user;

        public string MasterServerIpAddress;
        public int MasterServerPort;
        public NetworkClient client = null;

        public static NetworkMasterClient singleton;

        private void Awake() {
            if (singleton == null) {
                singleton = this;
                DontDestroyOnLoad(gameObject);
            } else {
                Destroy(gameObject);
            }
        }

        public ulong userId;
        public string userName;

        private void Start() {
            user = new User();
            user.userId = userId;
            user.userName = userName;
            InitializeClient();

            // TEMP
            // TODO : REMOVE AND INJECT CLASSIC IDENTIFICATION ROUTINE
            /*GameLogicClient game = new GameLogicClient("M002");
            game.gameId = 1;
            game.currentTurn = 0;
            user.currentGameId = game.gameId;
            for (int i = 0; i < 2; i++) {
                ulong playerId = (ulong)i;
                int cellId = DataManager.MAP_DATA["M002"].getSpawns(2)[i];
                int entityId = i;
                string displayedName = "Player " + i;
                float r = 255;
                float g = 255 * i;
                float b = 255 - 255 * i;

                Player temp = CreatePlayer(playerId, cellId, entityId, displayedName, r, g, b);
                if (user.userId == playerId) {
                    user.player = temp;
                }
                temp.playerEntity.initSpell(new string[4] { "S001", "S002", "S003", "S004" });
                temp.playerEntity.teamId = i;
            }

            GameLogicClient.game.prepareNewTurn(DateTime.UtcNow.AddSeconds(5).ToFileTimeUtc());*/
            // END TODO : REMOVE
        }

        public void InitializeClient() {
            if (client != null) {
                Debug.LogError("Try to connect, but was already connected.");
                return;
            }

            client = new NetworkClient();
            client.Connect(MasterServerIpAddress, MasterServerPort);

            // System messages
            client.RegisterHandler(MsgType.Connect, OnClientConnect);
            client.RegisterHandler(MsgType.Disconnect, OnClientDisconnect);
            client.RegisterHandler(MsgType.Error, OnClientError);

            // identification
            client.RegisterHandler(ServerIdentificationResponseMessage.ID, OnClientIdentified);

            // queue
            client.RegisterHandler(ServerJoinSoloQueueResponseMessage.ID, OnClientJoinSoloQueue);

            // game
            client.RegisterHandler(ServerStartGameMessage.ID, OnClientStartGame);
            client.RegisterHandler(ServerJoinGameResponseMessage.ID, OnClientJoinGame);
            // turn
            client.RegisterHandler(ServerStartNewTurnMessage.ID, OnClientStartNewTurn);
            client.RegisterHandler(ServerSyncTurnActionsMessage.ID, OnClientSyncTurnActions);
        }

        public void ResetClient() {
            if (client == null) {
                return;
            }
            client.Disconnect();
            client = null;

            if (GameLogicClient.game != null) {
                GameLogicClient.game.clearGame();
            }
        }

        // --------------- System handlers -----------------
        private void OnClientConnect(NetworkMessage netMsg) {
            Debug.Log("Connected to server " + netMsg.conn.address);
            ClientIdentificationRequest();
        }

        private void OnClientDisconnect(NetworkMessage netMsg) {
            Debug.Log("Client Disconnected from Master");
        }

        private void OnClientError(NetworkMessage netMsg) {
            Debug.Log("Client error from Master");
        }

        // --------------- Identification handlers -----------------
        void ClientIdentificationRequest() {
            ClientIdentificationRequestMessage msg = new ClientIdentificationRequestMessage();
            msg.userName = user.userName;
            msg.userId = user.userId;
            client.Send(ClientIdentificationRequestMessage.ID, msg);
            Debug.Log("Sent identification request");
        }

        void OnClientIdentified(NetworkMessage netMsg) {
            ServerIdentificationResponseMessage msg = netMsg.ReadMessage<ServerIdentificationResponseMessage>();
            if (msg.isSuccessful) {
                Debug.Log("Client successfully identified");
                user = new User(msg.userId, msg.userName);
                user.isIdentified = true;
                user.currentGameId = msg.currentGameId;
                // If client was in game on identification, join it
                if (msg.currentGameId != 0) {
                    // TODO - rejoin logic
                }
            } else {
                Debug.Log("Client failed identification");
            }
        }

        // --------------- Join game handlers -----------------

        // queue
        private void ClientJoinSoloQueueRequest() {
            ClientJoinSoloQueueRequestMessage msg = new ClientJoinSoloQueueRequestMessage();
            msg.userId = user.userId;
            msg.userName = user.userName;
            client.Send(ClientJoinSoloQueueRequestMessage.ID, msg);
            user.isQueued = true;
            Debug.Log("Sent join solo queue request");
        }

        private void ClientLeaveSoloQueueRequest() {
            ClientLeaveSoloQueueRequestMessage msg = new ClientLeaveSoloQueueRequestMessage();
            msg.userId = user.userId;
            msg.userName = user.userName;
            client.Send(ClientLeaveSoloQueueRequestMessage.ID, msg);
            user.isQueued = false;
            Debug.Log("Sent leave solo queue request");
        }

        private void OnClientJoinSoloQueue(NetworkMessage netMsg) {
            ServerJoinSoloQueueResponseMessage msg = netMsg.ReadMessage<ServerJoinSoloQueueResponseMessage>();
            if (msg.joinedQueue) {
                Debug.Log("Successfully joined solo queue.");
            } else {
                Debug.LogError("Failed to join solo queue.");
            }
        }

        private void OnClientJoinGame(NetworkMessage netMsg) {
            ServerJoinGameResponseMessage msg = netMsg.ReadMessage<ServerJoinGameResponseMessage>();

            if (msg.hasJoined) {
                GameLogicClient game = new GameLogicClient(msg.mapId);
                game.gameId = msg.gameId;
                game.currentTurn = msg.currentTurn;
                user.currentGameId = game.gameId;
                for (int i = 0; i < msg.cellIds.Length; i++) {
                    ulong playerId = msg.playerIds[i];
                    int cellId = msg.cellIds[i];
                    int entityId = msg.entityIds[i];
                    string displayedName = msg.displayedNames[i];
                    float r = msg.r[i];
                    float g = msg.g[i];
                    float b = msg.b[i];

                    Player temp = CreatePlayer(playerId, cellId, entityId, displayedName, r, g, b);
                    if (msg.clientPlayerId == playerId) {
                        user.player = temp;
                    }
                    temp.playerEntity.initSpell(msg.spellIds);
                }
                ClientReadyToPlay();
                Debug.Log("Received ServerJoinGameResponseMessage " + msg.gameId);
            } else {
                Debug.LogError("Failed to join game.");
            }
        }

        public void ClientReadyToPlay() {
            ClientReadyToPlayMessage msg = new ClientReadyToPlayMessage();
            msg.userId = user.userId;
            msg.userName = user.userName;
            msg.gameId = user.currentGameId;
            client.Send(ClientReadyToPlayMessage.ID, msg);
            Debug.Log("Sent ClientReadyToPlay " + msg.userName);
        }

        void OnClientStartGame(NetworkMessage netMsg) {
            ServerStartGameMessage msg = netMsg.ReadMessage<ServerStartGameMessage>();
            GameLogicClient.game.prepareNewTurn(msg.startFirstTurnTimestamp);
            // passer l'écran d'attente des joueurs
        }

        void OnClientStartNewTurn(NetworkMessage netMsg) {
            ServerStartNewTurnMessage msg = netMsg.ReadMessage<ServerStartNewTurnMessage>();
            GameLogicClient.game.prepareNewTurn(msg.startTurnTimestamp);
        }


        // --------------- Players handlers -----------------

        private Player CreatePlayer(ulong playerId, int cellId, int entityId, string displayedName, float r, float g, float b) {
            if (!GameLogicClient.game.players.ContainsKey(playerId)) {
                Cell cell = GameLogicClient.game.grid.GetCell(cellId);
                Player p = new Player();
                p.playerId = playerId;
                p.playerName = displayedName;
                GameLogicClient.game.spawnPlayer(p, entityId);
                p.playerEntity.entityId = entityId;
                GameLogicClient.game.entityList[entityId] = p.playerEntity;
                p.playerEntity.setColor(r, g, b);
                p.playerEntity.applyColor();
                p.playerEntity.setDisplayedName(displayedName);
                p.playerEntity.setCurrentCell(cell);
                return p;
            } else {
                return GameLogicClient.game.players[playerId];
            }
        }


        // --------------- Actions handlers -----------------

        public void SyncTurnActions(GameLogicClient game) {
            ClientRegisterTurnActionsMessage msg = new ClientRegisterTurnActionsMessage();
            msg.gameId = game.gameId;
            msg.userId = user.userId;
            msg.userName = user.userName;
            msg.actions = game.generateTurnActionsJSON();
            client.Send(ClientRegisterTurnActionsMessage.ID, msg);
            Debug.Log("Client sent RegisterTurnActions");
        }

        private void OnClientSyncTurnActions(NetworkMessage netMsg) {
            ServerSyncTurnActionsMessage msg = netMsg.ReadMessage<ServerSyncTurnActionsMessage>();
            GameLogicClient.game.receiveActions(msg.actions);
            GameLogicClient.game.resolveTurn();
            Debug.Log("Client received SyncTurnActions");
        }

        private void OnGUI() {
            Rect pos = new Rect(10, Screen.currentResolution.height - 220, 200, 20);
            GUI.Label(pos, "CLIENT");
            if (client != null && client.isConnected && user.isIdentified) {
                GUI.Label(new Rect(10, 20, 200, 20), "Identified as : " + user.userName);
                if (GUI.Button(new Rect(10, 60, 200, 20), "Disconnect")) {
                    ResetClient();
                    if (NetworkManager.singleton != null) {
                        NetworkManager.singleton.StopClient();
                    }
                }
                if (GameLogicClient.game == null) {
                    if (!user.isQueued) {
                        if (GUI.Button(new Rect(10, 100, 200, 20), "Join solo queue")) {
                            ClientJoinSoloQueueRequest();
                        }
                    } else {
                        if (GUI.Button(new Rect(10, 100, 200, 20), "Leave solo queue")) {
                            ClientLeaveSoloQueueRequest();
                        }
                    }
                } else {
                    GUI.Label(new Rect(10, 140, 200, 20), "Turn : " + GameLogicClient.game.currentTurn);
                    switch (GameLogicClient.game.currentTurnState) {
                        case GameLogicClient.TURN_STATE_PREP:
                            GUI.Label(new Rect(10, 150, 200, 20), "Time remaining : " + GameLogicClient.game.startTurnSecondRemaining);
                            GUI.Label(new Rect(10, 160, 200, 20), "Turn step : " + "TURN_STATE_PREP");
                            break;
                        case GameLogicClient.TURN_STATE_ACT:
                            GUI.Label(new Rect(10, 150, 200, 20), "Time remaining : " + GameLogicClient.game.endTurnSecondRemaining);
                            GUI.Label(new Rect(10, 160, 200, 20), "Turn step : " + "TURN_STATE_ACT");
                            break;
                        case GameLogicClient.TURN_STATE_SYNC:
                            GUI.Label(new Rect(10, 150, 200, 20), "Time remaining : " + GameLogicClient.game.serverDataTimeoutSecondRemaining);
                            GUI.Label(new Rect(10, 160, 200, 20), "Turn step : " + "TURN_STATE_SYNC");
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
