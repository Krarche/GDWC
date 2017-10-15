using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.SceneManagement;
using Data;
using Logic;
using Tools;
using Tools.JSON;

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
            try {
                ObjectJSON ID = ParserJSON.getObjectJSONFromAsset("ID");
                userId = (ulong)ID.getLong("userId");
                userName = ID.getString("userName");
                Debug.Log("Using custom ID loaded from file.");
            } catch (Exception) {
                Debug.Log("Using default ID.");
            }
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
            //client.Connect(MasterServerIpAddress, MasterServerPort);
            client.ConnectWithSimulator(MasterServerIpAddress, MasterServerPort, 200, 0.0f); // temp
            // System messages
            client.RegisterHandler(MsgType.Connect, OnClientConnect);
            client.RegisterHandler(MsgType.Disconnect, OnClientDisconnect);
            client.RegisterHandler(MsgType.Error, OnClientError);

            client.RegisterHandler(ServerSyncClockMessage.ID, OnClientSyncClock);

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

        public void ClientSyncClockRequest(long clientCurrentTimestamp) {
            ClientSyncClockMessage msg = new ClientSyncClockMessage();
            msg.timestamp = clientCurrentTimestamp;
            client.Send(ClientSyncClockMessage.ID, msg);
            Debug.Log("Sent clock synchronization request");
        }

        void OnClientSyncClock(NetworkMessage netMsg) {
            Debug.Log("OnClientSyncClock from Master");
            ServerSyncClockMessage msg = netMsg.ReadMessage<ServerSyncClockMessage>();
            ClockMaster.clientSingleton.endSyncClient(msg.timestamp);
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
                ClockMaster.clientSingleton.startSyncClient(); // sync clock
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
                Debug.Log("Received ServerJoinGameResponseMessage " + msg.gameId);
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

                    Player temp = spawnPlayer(playerId, cellId, entityId, displayedName, r, g, b);
                    if (msg.clientPlayerId == playerId) {
                        user.player = temp;
                    }
                    temp.playerEntity.initSpell(msg.spellIds);
                }
                ClientReadyToPlay();
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
            Debug.Log("Sent ClientReadyToPlayMessage.");
        }

        void OnClientStartGame(NetworkMessage netMsg) {
            ServerStartGameMessage msg = netMsg.ReadMessage<ServerStartGameMessage>();
            Debug.Log("Received ServerStartGameMessage");
            GameLogicClient.game.prepareNewTurn(msg.startFirstTurnTimestamp);
            // passer l'écran d'attente des joueurs
        }

        void OnClientStartNewTurn(NetworkMessage netMsg) {
            ServerStartNewTurnMessage msg = netMsg.ReadMessage<ServerStartNewTurnMessage>();
            Debug.Log("Received ServerStartNewTurnMessage");
            GameLogicClient.game.prepareNewTurn(msg.startTurnTimestamp);
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
            Debug.Log("Client received SyncTurnActions");
            GameLogicClient.game.receiveActions(msg.actions);
            GameLogicClient.game.resolveTurn();
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
                        case GameLogicClient.TURN_STATE_SYNC_WAIT:
                            GUI.Label(new Rect(10, 150, 200, 20), "Time remaining : " + GameLogicClient.game.serverDataTimeoutSecondRemaining);
                            GUI.Label(new Rect(10, 160, 200, 20), "Turn step : " + "TURN_STATE_SYNC_WAIT");
                            break;
                        default:
                            GUI.Label(new Rect(10, 160, 200, 20), "Turn step : " + GameLogicClient.game.currentTurnState);
                            break;
                    }
                }
            }
        }
    }
}
