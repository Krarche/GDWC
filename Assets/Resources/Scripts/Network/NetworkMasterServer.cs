using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;
using System;

public class NetworkMasterServer : MonoBehaviour {
    public int MasterServerPort;
    public ulong connectionCount = 0;
    public Dictionary<ulong, GameLogicServer> games = new Dictionary<ulong, GameLogicServer>();
    public GameLogicServer defaultGame = null;
    public ulong gameCount = 0;

    public Dictionary<int, Player> playersByConnection = new Dictionary<int, Player>();
    public Dictionary<string, Player> playersByName = new Dictionary<string, Player>();

    // map of gameTypeNames to rooms of that type
    // Dictionary<string, Player> gameTypeRooms = new Dictionary<string, Rooms> ();

    public void Awake() {
        if (isHeadless()) {
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
        NetworkServer.RegisterHandler(ClientIdentificationMessage.ID, OnServerIdentification);
        NetworkServer.RegisterHandler(MsgType.Disconnect, OnServerDisconnect);
        NetworkServer.RegisterHandler(MsgType.Error, OnServerError);

        // application msgs
        NetworkServer.RegisterHandler(ClientMovementOrderMessage.ID, OnClientMovementOrder);

        DontDestroyOnLoad(gameObject);
    }

    public void ResetServer() {
        NetworkServer.Shutdown();
    }

    public GameLogicServer createGame() {
        GameLogicServer game = new GameLogicServer();
        game.id = gameCount++;
        return game;
    }

    // --------------- System Handlers -----------------

    void OnServerConnect(NetworkMessage netMsg) {
        Debug.Log("Master received client");
        Debug.Log(netMsg.conn.address);

        if (defaultGame == null) {
            defaultGame = createGame();
            games.Add(defaultGame.id, defaultGame);
        }
    }

    void OnServerDisconnect(NetworkMessage netMsg) {
        Debug.Log("Master lost client");
        Player p = playersByConnection[netMsg.conn.connectionId];
        if(p.isIdentified) {
            playersByConnection.Remove(netMsg.conn.connectionId);
        }
    }

    void OnServerError(NetworkMessage netMsg) {
        Debug.Log("ServerError from Master");
    }

    // --------------- Player Handlers -----------------

    void OnServerIdentification(NetworkMessage netMsg) {
        ClientIdentificationMessage msg = netMsg.ReadMessage<ClientIdentificationMessage>();
        Debug.Log("Server received Identification from " + msg.playerName);

        Player p = null;
        if(playersByName.ContainsKey(msg.playerName))
            p = playersByName[msg.playerName];
        else
            p = new Player();

        p.identifie(msg.playerName);
        playersByName[msg.playerName] = p;
        playersByConnection[netMsg.conn.connectionId] = p;
        ServerIdentification(p, true);
        PlayerJoinGame(p, defaultGame.id, netMsg.conn);
    }

    void ServerIdentification(Player p, bool state) {
        ServerIdentificationMessage msg = new ServerIdentificationMessage();
        msg.state = state;
        msg.playerId = p.playerId;
        NetworkServer.SendToAll(ServerIdentificationMessage.ID, msg);
        Debug.Log("Server sent ServerIdentification " + msg.state); 
    }

    void PlayerJoinGame(Player player, ulong gameId, NetworkConnection conn) {
        CreateGameForNewClient(defaultGame, conn);

        foreach (Player p in defaultGame.playerList.Values)
            CreatePlayerForNewClient(defaultGame, conn, p.getCurrentCell());

        if (!defaultGame.playerList.ContainsKey(player.playerId)) {
            CreatePlayer(defaultGame, 0, player);
            defaultGame.addPlayer(player);
        }
    }

    // --------------- Application Handlers -----------------


    void OnClientMovementOrder(NetworkMessage netMsg) {
        ClientMovementOrderMessage msg = netMsg.ReadMessage<ClientMovementOrderMessage>();
        Debug.Log("Server received OnClientMovementOrder ");
        GameLogicServer game = games[msg.gameId];
        MovementOrder(game, msg.cellId, msg.playerId);
    }

    public void MovementOrder(GameLogicServer game, int cellId, ulong playerId) {
        game.playerList[playerId].addOrder(new MovementOrder(cellId));
        ServerMovementOrderMessage msg = new ServerMovementOrderMessage();
        msg.cellId = cellId;
        msg.playerId = playerId;
        msg.gameId = game.id;
        NetworkServer.SendToAll(ServerMovementOrderMessage.ID, msg);
        Debug.Log("Server sent MovementOrder ");
    }

    public void CreatePlayer(GameLogicServer game, int cellId, Player p) {
        p.entity = game.createEntity(cellId);
        ServerCreatePlayerMessage msg = new ServerCreatePlayerMessage();
        msg.cellId = cellId;
        msg.gameId = game.id;
        msg.playerId = p.playerId;
        NetworkServer.SendToAll(ServerCreatePlayerMessage.ID, msg);
        Debug.Log("Server sent CreatePlayer ");
    }

    public void CreatePlayerForNewClient(GameLogicServer game, NetworkConnection client, int cellId) {
        ServerCreatePlayerMessage msg = new ServerCreatePlayerMessage();
        msg.cellId = cellId;
        msg.gameId = game.id;
        client.Send(ServerCreatePlayerMessage.ID, msg);
        Debug.Log("Server sent CreatePlayerForNewClient ");
    }

    public void CreateGameForNewClient(GameLogicServer game, NetworkConnection client) {
        ServerCreateGameMessage msg = new ServerCreateGameMessage();
        msg.gameId = game.id;
        client.Send(ServerCreateGameMessage.ID, msg);
        Debug.Log("Server sent CreateGameForNewClient ");
    }
}
