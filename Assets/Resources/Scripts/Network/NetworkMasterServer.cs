using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;
using Data;
using Logic;

namespace Network {

    public class NetworkMasterServer : MonoBehaviour {

        public static NetworkMasterServer singleton;

        public int MasterServerPort;

        public Dictionary<ulong, GameLogicServer> games = new Dictionary<ulong, GameLogicServer>();

        public Dictionary<NetworkConnection, User> usersByConnection = new Dictionary<NetworkConnection, User>();
        public Dictionary<string, User> usersByName = new Dictionary<string, User>();

        public GameQueue soloQueue = new GameQueue();

        public void Awake() {
            singleton = this;
            InitializeServer();
        }

        public static bool isHeadless() {
            return SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null;
        }

        public void InitializeServer() {
            if (NetworkServer.active) {
                Debug.LogError("Already Initialized");
                return;
            }

            NetworkServer.Listen(MasterServerPort);

            // system msgs
            NetworkServer.RegisterHandler(MsgType.Connect, OnServerConnect);
            NetworkServer.RegisterHandler(MsgType.Disconnect, OnServerDisconnect);
            NetworkServer.RegisterHandler(MsgType.Error, OnServerError);

            // identification
            NetworkServer.RegisterHandler(ClientIdentificationRequestMessage.ID, OnServerIdentificationRequest);

            // queue
            NetworkServer.RegisterHandler(ClientJoinSoloQueueRequestMessage.ID, OnServerJoinSoloQueueRequest);
            NetworkServer.RegisterHandler(ClientLeaveSoloQueueRequestMessage.ID, OnServerLeaveSoloQueueRequest);

            // game
            NetworkServer.RegisterHandler(ClientReadyToPlayMessage.ID, OnServerReadyToPlay);

            // turn
            NetworkServer.RegisterHandler(ClientRegisterTurnActionsMessage.ID, OnServerRegisterTurnActions);

            DontDestroyOnLoad(gameObject);

            Debug.Log("Server initialized");
        }


        // --------------- System handlers -----------------
        private void OnServerConnect(NetworkMessage netMsg) {
            Debug.Log("Received connection request from client " + netMsg.conn.address);
        }

        private void OnServerDisconnect(NetworkMessage netMsg) {
            Debug.Log("Master lost client");
            User u = usersByConnection[netMsg.conn];
            if (u.isIdentified) {
                usersByConnection.Remove(netMsg.conn);
            }
        }

        private void OnServerError(NetworkMessage netMsg) {
            Debug.Log("ServerError from Master");
        }

        // --------------- Identification handlers -----------------
        void OnServerIdentificationRequest(NetworkMessage netMsg) {
            ClientIdentificationRequestMessage msg = netMsg.ReadMessage<ClientIdentificationRequestMessage>();
            Debug.Log("Server received Identification request from " + msg.userName);

            User u = null;
            // Get or create the user identifying
            if (usersByName.ContainsKey(msg.userName)) { // if user already identified
                u = usersByName[msg.userName];
            } else {
                u = new User();
                usersByName[msg.userName] = u;
            }

            u.identify(msg.userName, msg.userId);
            usersByConnection[netMsg.conn] = u;
            u.connection = netMsg.conn;
            ServerIdentificationResponse(u, netMsg.conn);
        }

        void ServerIdentificationResponse(User u, NetworkConnection client) {
            ServerIdentificationResponseMessage msg = new ServerIdentificationResponseMessage();
            msg.isSuccessful = u.isIdentified;
            msg.userId = u.userId;
            msg.userName = u.userName; // temporary
            msg.currentGameId = u.currentGameId;

            client.Send(ServerIdentificationResponseMessage.ID, msg);
            Debug.Log("Sent ServerIdentificationResponse " + msg.isSuccessful);
        }


        // --------------- Join game handlers -----------------
        // queue
        private void OnServerJoinSoloQueueRequest(NetworkMessage netMsg) {
            ClientJoinSoloQueueRequestMessage msg = netMsg.ReadMessage<ClientJoinSoloQueueRequestMessage>();
            Debug.Log("Server received JoinSoloQueue request from " + msg.userId + " " + msg.userName);
            User u = usersByName[msg.userName];
            if (u.isIdentified) {
                if (soloQueue.queueUser(u)) {
                    GameLogicServer newgame = soloQueue.checkQueue();
                    if (newgame != null) {
                        games[newgame.gameId] = newgame;
                        ServerSendNewGameDataToPlayers(newgame);
                    } else {
                        ServerJoinSoloQueueResponseMessage responseMsg = new ServerJoinSoloQueueResponseMessage();
                        responseMsg.joinedQueue = true;
                        netMsg.conn.Send(ServerJoinSoloQueueResponseMessage.ID, responseMsg);
                    }
                }
            } else {
                ServerJoinSoloQueueResponseMessage responseMsg = new ServerJoinSoloQueueResponseMessage();
                responseMsg.joinedQueue = false;
                netMsg.conn.Send(ServerJoinSoloQueueResponseMessage.ID, responseMsg);
            }
        }

        private void OnServerLeaveSoloQueueRequest(NetworkMessage netMsg) {
            ClientLeaveSoloQueueRequestMessage msg = netMsg.ReadMessage<ClientLeaveSoloQueueRequestMessage>();
            Debug.Log("Server received LeaveSoloQueue request from " + msg.userId + " " + msg.userName);
            User u = usersByName[msg.userName];
            if (u.isIdentified && u.currentGameId == 0) {
                soloQueue.unqueueUser(u);
            } else {
                // game already found
                // sad player :)
            }
        }

        void ServerSendNewGameDataToPlayers(GameLogic game) {
            ServerJoinGameResponseMessage msg = new ServerJoinGameResponseMessage();

            int playerCount = game.players.Count;
            msg.initArrays(playerCount);

            // Adding all existing players to message (including new player)
            int i = 0;
            foreach (Player p in game.players.Values) {
                msg.cellIds[i] = p.getCurrentCellId();
                msg.playerIds[i] = p.playerId;
                msg.entityIds[i] = p.playerEntity.entityId;
                msg.displayedNames[i] = p.playerName;
                msg.r[i] = p.playerColor.r;
                msg.g[i] = p.playerColor.g;
                msg.b[i] = p.playerColor.b;
                msg.spellIds = new string[4] { "S001", "S002", "S003", "S004" };
                i++;
            }

            msg.gameId = game.gameId;
            msg.mapId = game.mapId;
            msg.hasJoined = true;

            foreach (Player player in game.players.Values) {
                msg.clientPlayerId = player.playerId;
                User u = player.user;
                u.connection.Send(ServerJoinGameResponseMessage.ID, msg);
            }
            Debug.Log("ServerSendNewGameDataToPlayers " + msg.gameId);
        }
        // --------------- Turns handlers -----------------

        private void OnServerReadyToPlay(NetworkMessage netMsg) {
            ClientReadyToPlayMessage msg = netMsg.ReadMessage<ClientReadyToPlayMessage>();
            GameLogicServer game = games[msg.gameId];
            game.registerPlayerReady(game.getPlayerByUserId(msg.userId));
            Debug.Log("Server received OnServerReadyToPlay from user " + msg.userName + " in game " + msg.gameId);
        }

        public void ServerStartTurnMessage(GameLogicServer game, long startTurnTimestamp) {
            if (game.currentTurn == 0)
                StartGame(game, startTurnTimestamp);
            else
                StartNewTurn(game, startTurnTimestamp);
        }

        private void StartGame(GameLogicServer game, long startFirstTurnTimestamp) {
            ServerStartGameMessage msgOut = new ServerStartGameMessage();
            msgOut.startFirstTurnTimestamp = startFirstTurnTimestamp;
            foreach (Player p in game.players.Values) {
                p.user.connection.Send(ServerStartGameMessage.ID, msgOut);
            }
            Debug.Log("StartGame " + game.gameId);
        }

        private void StartNewTurn(GameLogicServer game, long startTurnTimestamp) {
            ServerStartNewTurnMessage msgOut = new ServerStartNewTurnMessage();
            msgOut.startTurnTimestamp = startTurnTimestamp;
            foreach (Player p in game.players.Values) {
                p.user.connection.Send(ServerStartNewTurnMessage.ID, msgOut);
            }
            Debug.Log("StartNewTurn " + game.gameId);
        }

        // --------------- Actions handlers -----------------
        void OnServerRegisterTurnActions(NetworkMessage netMsg) {
            ClientRegisterTurnActionsMessage msg = netMsg.ReadMessage<ClientRegisterTurnActionsMessage>();
            Debug.Log("Server received OnServerRegisterTurnActions");
            GameLogicServer game = games[msg.gameId];
            game.registerPlayerAction(game.getPlayerByUserId(msg.userId), msg.actions);
        }

        public void SyncTurnActions(GameLogicServer game) {
            ServerSyncTurnActionsMessage msg = new ServerSyncTurnActionsMessage();
            msg.actions = game.generateTurnActionsJSON();
            foreach (Player player in game.players.Values) {
                User u = player.user;
                u.connection.Send(ServerSyncTurnActionsMessage.ID, msg);
            }
            Debug.Log("Server sent SyncTurnActions");
        }

        // --------------- Send to in game players handlers -----------------
        private void SendToOtherGamePlayers(ServerMessage msg, short msgId, ulong gameId, User sender) {
            foreach (NetworkConnection client in usersByConnection.Keys) {
                if (usersByConnection[client] != sender && usersByConnection[client].player != null && games[gameId].players.ContainsKey(usersByConnection[client].player.playerId)) {
                    client.Send(msgId, msg);
                }
            }
        }
        private void SendToAllGamePlayers(ServerMessage msg, short msgId, ulong gameId) {
            foreach (NetworkConnection client in usersByConnection.Keys) {
                if (usersByConnection[client].player != null && games[gameId].players.ContainsKey(usersByConnection[client].player.playerId)) {
                    client.Send(msgId, msg);
                }
            }
        }

        private void OnGUI() {
            Rect pos = new Rect(10, Screen.currentResolution.height - 200, 200, 20);
            GUI.Label(pos, "SERVER");
        }

    }
}