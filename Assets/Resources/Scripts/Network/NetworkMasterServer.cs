using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;
using System;

public class NetworkMasterServer : MonoBehaviour {

    public bool forceServer = false;

    public int MasterServerPort;

    public Dictionary<ulong, GameLogicServer> games = new Dictionary<ulong, GameLogicServer>();

    public Dictionary<NetworkConnection, User> usersByConnection = new Dictionary<NetworkConnection, User>();
    public Dictionary<string, User> usersByName = new Dictionary<string, User>();

    public GameQueue soloQueue = new GameQueue();

    public void Awake() {
        if (forceServer || isHeadless()) {
            InitializeServer();
        }
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

        // application msgs
        NetworkServer.RegisterHandler(ClientIdentificationRequestMessage.ID, OnServerIdentificationRequest);
        NetworkServer.RegisterHandler(ClientJoinGameRequestMessage.ID, OnServerJoinGameRequest);
        NetworkServer.RegisterHandler(ClientLeaveGameRequestMessage.ID, OnServerLeaveGameRequest);

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

    private void OnServerReadyToPlay(NetworkMessage netMsg) {
        ClientReadyToPlayMessage msg = netMsg.ReadMessage<ClientReadyToPlayMessage>();
        GameLogicServer game = games[msg.gameId];
        DateTime startFirstTurn = DateTime.UtcNow.AddSeconds(5);
        long startFirstTurnTimestamp = startFirstTurn.ToFileTimeUtc();
        ServerStartGameMessage msgOut = new ServerStartGameMessage();
        msgOut.startFirstTurnTimestamp = startFirstTurnTimestamp;
        foreach (Player p in game.players.Values) {
            p.user.connection.Send(ServerStartGameMessage.ID, msgOut);
        }
    }

    private void OnServerJoinGameRequest(NetworkMessage netMsg) {
        ClientJoinGameRequestMessage msg = netMsg.ReadMessage<ClientJoinGameRequestMessage>();
        Debug.Log("Server received JoinGame request from " + msg.userId + " " + msg.userName);
        User u = usersByName[msg.userName];
        GameLogic game = games[msg.gameId];
        if (u.isIdentified) {
            ServerJoinGameResponse(u, game, netMsg.conn);
        } else {
            ServerJoinGameResponseMessage responseMsg = new ServerJoinGameResponseMessage();
            responseMsg.hasJoined = false;
            netMsg.conn.Send(ServerJoinGameResponseMessage.ID, responseMsg);
        }
    }

    void ServerJoinGameResponse(User u, GameLogic game, NetworkConnection conn) {
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
            i++;
        }

        msg.gameId = game.gameId;
        msg.currentTurn = game.currentTurn;
        msg.mapId = game.mapId;
        msg.hasJoined = true;


        msg.clientPlayerId = u.player.playerId;
        conn.Send(ServerJoinGameResponseMessage.ID, msg);
    }

    // --------------- Leave game handlers -----------------
    private void OnServerLeaveGameRequest(NetworkMessage netMsg) {
        ClientLeaveGameRequestMessage msg = netMsg.ReadMessage<ClientLeaveGameRequestMessage>();
        Debug.Log("Server received LeaveGame request from " + msg.userId);
        User u = usersByName[msg.userName];
        if (u.currentGameId != 0) {
            ServerLeaveGameResponse(u, u.currentGameId, netMsg.conn); // temporary default gameId
            u.currentGameId = 0;
        } else {
            ServerLeaveGameResponseMessage responseMsg = new ServerLeaveGameResponseMessage();
            responseMsg.hasLeft = false;
            netMsg.conn.Send(ServerJoinGameResponseMessage.ID, responseMsg);
        }
    }

    private void ServerLeaveGameResponse(User u, ulong gameId, NetworkConnection conn) {
        // Notify all other clients that player left game
        ServerPlayerLeftGameMessage msg_removePlayer = new ServerPlayerLeftGameMessage();
        msg_removePlayer.playerId = u.player.playerId;
        msg_removePlayer.playerName = u.player.playerName;
        SendToOtherGamePlayers(msg_removePlayer, ServerPlayerLeftGameMessage.ID, gameId, u);

        // Removing the player from server game
        if (u.player != null) {
            games[gameId].removePlayer(u.player);
            u.player = null;
        }

        ServerLeaveGameResponseMessage msg = new ServerLeaveGameResponseMessage();
        msg.hasLeft = true;
        conn.Send(ServerLeaveGameResponseMessage.ID, msg);
    }


    // --------------- Movement handlers -----------------
    void OnServerRegisterTurnActions(NetworkMessage netMsg) {
        ClientRegisterTurnActionsMessage msg = netMsg.ReadMessage<ClientRegisterTurnActionsMessage>();
        Debug.Log("Server received OnServerRegisterTurnActions");
        GameLogicServer game = games[msg.gameId];
        // save actions forsync
    }

    public void SyncTurnActions(GameLogicServer game) {
        ServerSyncTurnActionsMessage msg = new ServerSyncTurnActionsMessage();
        // msg.actions ="";
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
}
