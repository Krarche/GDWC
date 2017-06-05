using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;


public class NetworkMasterServer : MonoBehaviour {
    public int MasterServerPort;
    public int connectionCount = 0;
    public Dictionary<ulong, GameLogicServer> games = new Dictionary<ulong, GameLogicServer>();
    public GameLogicServer defaultGame = null;
    public ulong gameCount = 0;
    public Dictionary<string, Player> players = new Dictionary<string, Player>();

    // map of gameTypeNames to rooms of that type
    // Dictionary<string, Player> gameTypeRooms = new Dictionary<string, Rooms> ();

    public void Awake() {
        if (isHeadless()) {
            InitializeServer();
        }
    }

    private bool isHeadless() {
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
        NetworkServer.RegisterHandler(CellChangeColorMessage.ID, OnCellChangeColor);
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
        CreateGameForNewClient(defaultGame, netMsg.conn);

        if (!players.ContainsKey(netMsg.conn.address)) {
            Player newPlayer = new Player();
            TellClientId(netMsg.conn, connectionCount);
            CreatePlayer(defaultGame, connectionCount, newPlayer);
            defaultGame.addPlayer(newPlayer);
            players.Add(netMsg.conn.address, newPlayer);
            connectionCount++;
        } else {
            TellClientId(netMsg.conn, players[netMsg.conn.address].playerId);
        }
        
        for (int i = 0; i < defaultGame.playerList.Count; i++)
            CreatePlayerForNewClient(defaultGame, netMsg.conn, defaultGame.playerList[i].getCurrentCell());
    }

    void OnServerDisconnect(NetworkMessage netMsg) {
        Debug.Log("Master lost client");
        Debug.Log(netMsg.conn.address);
        players.Remove(netMsg.conn.address);
        defaultGame.removePlayer(players[netMsg.conn.address]);
    }

    void OnServerError(NetworkMessage netMsg) {
        Debug.Log("ServerError from Master");
    }

    // --------------- Player Handlers -----------------

    public void OnPlayerDisconnected(NetworkPlayer player) {
        Debug.Log("Player "+ player.ipAddress +" disconnected");
        players.Remove(player.ipAddress);
        defaultGame.removePlayer(players[player.ipAddress]);
    }

    // --------------- Application Handlers -----------------

    void OnCellChangeColor(NetworkMessage netMsg) {
        CellChangeColorMessage msg = netMsg.ReadMessage<CellChangeColorMessage>();
        Debug.Log("Server received OnCellChangeColor " + msg.GetColor().ToString());

        GameLogicServer game = games[msg.gameId];
        game.map.ClearSelection();
        ChangeCellColor(game, msg.cellId, msg.GetColor());
    }

    void OnClientMovementOrder(NetworkMessage netMsg) {
        ClientMovementOrderMessage msg = netMsg.ReadMessage<ClientMovementOrderMessage>();
        Debug.Log("Server received OnClientMovementOrder ");

        GameLogicServer game = games[msg.gameId];
        MovementOrder(game, msg.cellId, msg.playerId);
    }

    public void ChangeCellColor(GameLogicServer game, int[] cellId, Color color) {
        CellChangeColorMessage msg = new CellChangeColorMessage();
        msg.cellId = cellId;
        msg.SetColor(color);
        msg.gameId = game.id;
        NetworkServer.SendToAll(CellChangeColorMessage.ID, msg);
        Debug.Log("Server sent ChangeCellColor " + color.ToString());
    }

    public void MovementOrder(GameLogicServer game, int cellId, int playerId) {
        game.playerList[playerId].addOrder(new MovementOrder(cellId));
        ServerMovementOrderMessage msg = new ServerMovementOrderMessage();
        msg.cellId = cellId;
        msg.playerId = playerId;
        msg.gameId = game.id;
        NetworkServer.SendToAll(ServerMovementOrderMessage.ID, msg);
        Debug.Log("Server sent MovementOrder ");
    }

    public void TellClientId(NetworkConnection client, int playerId) {
        ServerTellClientPlayerIdMessage msg = new ServerTellClientPlayerIdMessage();
        msg.playerId = playerId;
        client.Send(ServerTellClientPlayerIdMessage.ID, msg);
        Debug.Log("Server sent TellClientId ");
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
