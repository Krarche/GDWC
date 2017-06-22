using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;
using System;

public class NetworkMasterServer : MonoBehaviour {
    public int MasterServerPort;

    public Dictionary<ulong, GameLogicServer> games = new Dictionary<ulong, GameLogicServer>();
    public GameLogicServer defaultGame = null;
    public ulong gameCount;

    public Dictionary<NetworkConnection, User> usersByConnection = new Dictionary<NetworkConnection, User>();
    public Dictionary<string, User> usersByName = new Dictionary<string, User>();

    public void Awake() {
        if(isHeadless()) {
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

        // Temporary defaultGame
        if (defaultGame == null) {
            gameCount = 0;
            defaultGame = createGame();
            games.Add(defaultGame.gameId, defaultGame);
        }

        // system msgs
        NetworkServer.RegisterHandler(MsgType.Connect, OnServerConnect);
        NetworkServer.RegisterHandler(MsgType.Disconnect, OnServerDisconnect);
        NetworkServer.RegisterHandler(MsgType.Error, OnServerError);

        // application msgs
        NetworkServer.RegisterHandler(ClientIdentificationRequestMessage.ID, OnServerIdentificationRequest);
        NetworkServer.RegisterHandler(ClientJoinGameRequestMessage.ID, OnServerJoinGameRequest);
        NetworkServer.RegisterHandler(ClientLeaveGameRequestMessage.ID, OnServerLeaveGameRequest);
        NetworkServer.RegisterHandler(ClientMovementOrderMessage.ID, OnClientMovementOrder);

        DontDestroyOnLoad(gameObject);

        Debug.Log("Server initialized");
    }

    public void ResetServer() {
        NetworkServer.Shutdown();
    }

    public GameLogicServer createGame() {
        GameLogicServer game = new GameLogicServer();
        game.gameId = ++gameCount;
        return game;
    }

    void OnServerConnect(NetworkMessage netMsg) {
        Debug.Log("Received connection request from client " + netMsg.conn.address);
    }

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
            
        u.identify(msg.userName);
        usersByConnection[netMsg.conn] = u;
        ServerIdentificationResponse(u, netMsg.conn);
    }
   
    void ServerIdentificationResponse(User u, NetworkConnection client) {
        ServerIdentificationResponseMessage msg = new ServerIdentificationResponseMessage();
        msg.isSuccessful = u.isIdentified;
        msg.userId = u.userId;
        msg.userName = u.userName; // temporary

        if(u.currentGameId != 0) {
            msg.currentGameId = u.currentGameId;
        }
        else {
            msg.currentGameId = 0;
        }

        client.Send(ServerIdentificationResponseMessage.ID, msg);
        Debug.Log("Sent ServerIdentificationResponse " + msg.isSuccessful);
    }

    private void OnServerJoinGameRequest(NetworkMessage netMsg) {
        ClientJoinGameRequestMessage msg = netMsg.ReadMessage<ClientJoinGameRequestMessage>();
        Debug.Log("Server received JoinGame request from "  + msg.userId + " " + msg.userName);
        User u = usersByName[msg.userName];
        if (u.isIdentified) {
            ServerJoinGameResponse(u, defaultGame.gameId, netMsg.conn);
            u.currentGameId = defaultGame.gameId;
        }
        else {
            ServerJoinGameResponseMessage responseMsg = new ServerJoinGameResponseMessage();
            responseMsg.hasJoined = false;
            netMsg.conn.Send(ServerJoinGameResponseMessage.ID, responseMsg);
        }
    }

    void ServerJoinGameResponse(User u, ulong gameId, NetworkConnection conn) {
        ServerJoinGameResponseMessage msg = new ServerJoinGameResponseMessage();


        // Adding new player to server game
        if (u.player == null) {
            u.player = CreatePlayer(u, 0);
            games[gameId].addPlayer(u.player);
        } else if (games[gameId].players.ContainsKey(u.player.playerId)) {
            u.player = games[gameId].players[u.player.playerId];
        }

        int playerCount = defaultGame.players.Count;
        msg.cellIds = new int[playerCount];
        msg.playerIds = new ulong[playerCount];
        msg.entityIds = new int[playerCount];
        msg.displayedNames = new string[playerCount];
        msg.r = new float[playerCount];
        msg.g = new float[playerCount];
        msg.b = new float[playerCount];

        // Adding all existing players to message (including new player)
        int i = 0;
        foreach(Player p in games[gameId].players.Values) {
            msg.cellIds[i] = p.getCurrentCellId();
            msg.playerIds[i] = p.playerId;
            msg.entityIds[i] = p.playerEntity.entityId;
            msg.displayedNames[i] = p.playerName;
            msg.r[i] = p.playerColor.r;
            msg.g[i] = p.playerColor.g;
            msg.b[i] = p.playerColor.b;
            i++;
        }

        msg.gameID = gameId;
        msg.clientPlayerId = u.player.playerId;
        msg.hasJoined = true;

        // Sending new player to all other clients
        ServerPlayerJoinedMessage msg_addPlayer = new ServerPlayerJoinedMessage();
        Player newPlayer = u.player;
        msg_addPlayer.cellId = newPlayer.getCurrentCellId();
        msg_addPlayer.playerId = newPlayer.playerId;
        msg_addPlayer.entityId = newPlayer.playerEntity.entityId;
        msg_addPlayer.displayedName = newPlayer.playerName;
        msg_addPlayer.r = newPlayer.playerColor.r;
        msg_addPlayer.g = newPlayer.playerColor.g;
        msg_addPlayer.b = newPlayer.playerColor.b;

        SendToOtherGamePlayers(msg_addPlayer, ServerPlayerJoinedMessage.ID, gameId, u);
        conn.Send(ServerJoinGameResponseMessage.ID, msg);
    }

    public Player CreatePlayer(User u, int cellId) {
        Player p = new Player();
        defaultGame.lastEntityIdGenerated++;
        Cell cell = defaultGame.grid.GetCell(cellId);
        p.playerEntity = defaultGame.createEntity(defaultGame.lastEntityIdGenerated);
        p.playerEntity.setCurrentCell(cell);
        p.playerEntity.setColor(p.playerColor);
        p.playerEntity.setDisplayedName(u.userName); // Temporary userName = playerName
        p.playerName = u.userName;
        return p;
    }

    private void OnServerLeaveGameRequest(NetworkMessage netMsg) {
        ClientLeaveGameRequestMessage msg = netMsg.ReadMessage<ClientLeaveGameRequestMessage>();
        Debug.Log("Server received LeaveGame request from " + msg.userId);
        User u = usersByName[msg.userName];
        if (u.currentGameId != 0) {
            ServerLeaveGameResponse(u, defaultGame.gameId, netMsg.conn); // temporary default gameId
            u.currentGameId = 0;
        }
        else {
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

    void OnServerDisconnect(NetworkMessage netMsg) {
        Debug.Log("Master lost client");
        User u = usersByConnection[netMsg.conn];
        if (u.isIdentified) {
            usersByConnection.Remove(netMsg.conn);
        }
    }

    void OnServerError(NetworkMessage netMsg) {
        Debug.Log("ServerError from Master");
    }

    

    // --------------- Application Handlers -----------------


    void OnClientMovementOrder(NetworkMessage netMsg) {
        ClientMovementOrderMessage msg = netMsg.ReadMessage<ClientMovementOrderMessage>();
        Debug.Log("Server received OnClientMovementOrder ");
        GameLogicServer game = games[msg.gameID];
        MovementOrder(game, msg.cellId, msg.entityId);
    }

    public void MovementOrder(GameLogicServer game, int cellId, int entityId) {
        game.entityList[entityId].addOrder(new MovementOrder(cellId, entityId));
        ServerMovementOrderMessage msg = new ServerMovementOrderMessage();
        msg.cellId = cellId;
        msg.entityId = entityId;
        msg.gameId = game.gameId;
        SendToAllGamePlayers(msg, ServerMovementOrderMessage.ID, game.gameId);
        game.resolveAction(new MovementOrder(cellId, entityId));
        Debug.Log("Server sent MovementOrder ");
    }

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
