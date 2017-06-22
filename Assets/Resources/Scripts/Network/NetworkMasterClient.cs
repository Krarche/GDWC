using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class NetworkMasterClient : MonoBehaviour {

    public User user = new User("defaultUsername");

    public string MasterServerIpAddress;
    public int MasterServerPort;
    public NetworkClient client = null;

    public static NetworkMasterClient singleton;

    public bool isConnected {
        get {
            return client == null;
        }
    }

    void Awake() {
        if (singleton == null) {
            singleton = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public void InitializeClient() {
        if (client != null) {
            Debug.LogError("Already connected");
            return;
        }

        client = new NetworkClient();
        client.Connect(MasterServerIpAddress, MasterServerPort);

        // System messages
        client.RegisterHandler(MsgType.Connect, OnClientConnect);
        client.RegisterHandler(MsgType.Disconnect, OnClientDisconnect);
        client.RegisterHandler(MsgType.Error, OnClientError);

        // Custom messages
        client.RegisterHandler(ServerIdentificationResponseMessage.ID, OnClientIdentified);

        client.RegisterHandler(ServerJoinGameResponseMessage.ID, OnClientJoinGame);
        client.RegisterHandler(ServerLeaveGameResponseMessage.ID, OnClientLeaveGame);

        client.RegisterHandler(ServerPlayerJoinedMessage.ID, OnClientCreateNewPlayer);
        client.RegisterHandler(ServerPlayerLeftGameMessage.ID, OnClientRemovePlayer);
        
        client.RegisterHandler(ServerMovementOrderMessage.ID, OnMovementOrder);

        DontDestroyOnLoad(gameObject);
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
        client.Send(ClientIdentificationRequestMessage.ID, msg);
        Debug.Log("Sent identification request");
    }

    void OnClientIdentified(NetworkMessage netMsg) {
        ServerIdentificationResponseMessage msg = netMsg.ReadMessage<ServerIdentificationResponseMessage>();
        if (msg.isSuccessful) {
            Debug.Log("Client successfully identified");
            user = new User(msg.userId, msg.userName);
            user.isIdentified = true;

            // If client was in game on identification, join it
            if (msg.currentGameId != 0) {
                ClientJoinGameRequest();
            }
        } else {
            Debug.Log("Client failed identification");
        }
    }


    // --------------- Join game handlers -----------------
    private void ClientJoinGameRequest() {
        ClientJoinGameRequestMessage msg = new ClientJoinGameRequestMessage();
        msg.userId = user.userId;
        msg.userName = user.userName;
        client.Send(ClientJoinGameRequestMessage.ID, msg);
        Debug.Log("Sent join game request");
    }

    private void OnClientJoinGame(NetworkMessage netMsg) {
        ServerJoinGameResponseMessage msg = netMsg.ReadMessage<ServerJoinGameResponseMessage>();

        if(msg.hasJoined) {
            GameLogicClient game = new GameLogicClient();
            game.gameId = msg.gameID;
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
                if(msg.clientPlayerId == playerId) {
                    user.player = temp;
                }
            
            }
            Debug.Log("Received ServerJoinGameResponseMessage " + msg.gameId);
        }
        else {
            Debug.Log("Failed to join game.");
        }
    }


    // --------------- Players handlers -----------------
    private void OnClientCreateNewPlayer(NetworkMessage netMsg) {
        ServerPlayerJoinedMessage msg = netMsg.ReadMessage<ServerPlayerJoinedMessage>();
        CreatePlayer(msg.playerId, msg.cellId, msg.entityId, msg.displayedName, msg.r, msg.g, msg.b);
        Debug.Log(msg.displayedName + " has joined the game.");
    }

    private Player CreatePlayer(ulong playerId, int cellId, int entityId, string displayedName, float r, float g, float b) {
        if (!GameLogicClient.game.players.ContainsKey(playerId)) {
            Cell cell = GameLogicClient.game.grid.GetCell(cellId);
            Entity e = GameLogicClient.game.createEntity(entityId);
            e.setCurrentCell(cell);
            e.setColor(r, g, b);
            e.applyColor();
            e.setDisplayedName(displayedName);
            Player p = new Player(playerId, e);
            GameLogicClient.game.addPlayer(p);
            Debug.Log("Client received OnCreatePlayer " + playerId);
            return p;
        } else {
            return GameLogicClient.game.players[playerId];
        }
    }

    private void OnClientRemovePlayer(NetworkMessage netMsg) {
        ServerPlayerLeftGameMessage msg = netMsg.ReadMessage<ServerPlayerLeftGameMessage>();
        GameLogicClient.game.removePlayer(GameLogicClient.game.players[msg.playerId]);
        Debug.Log(msg.playerName + " has left the game.");
    }


    // --------------- Leave game handlers -----------------
    private void ClientLeaveGameRequest() {
        ClientLeaveGameRequestMessage msg = new ClientLeaveGameRequestMessage();
        msg.userId = user.userId;
        msg.userName = user.userName;
        client.Send(ClientLeaveGameRequestMessage.ID, msg);
        Debug.Log("Sent leave game request");
    }

    private void OnClientLeaveGame(NetworkMessage netMsg) {
        ServerLeaveGameResponseMessage msg = netMsg.ReadMessage<ServerLeaveGameResponseMessage>();
        if(msg.hasLeft) {
            Debug.Log("Successfully left game");
            GameLogicClient.game.clearGame();
            user.currentGameId = 0;
        }
        else {
            Debug.Log("Couldn't leave game. No currentGame ?");
        }
    }


    // --------------- Movement handlers -----------------
    void OnMovementOrder(NetworkMessage netMsg) {
        ServerMovementOrderMessage msg = netMsg.ReadMessage<ServerMovementOrderMessage>();
        GameLogicClient.game.entityList[msg.entityId].addOrder(new MovementOrder(msg.cellId, msg.entityId));
        GameLogicClient.game.resolveAction(new MovementOrder(msg.cellId, msg.entityId));
        Debug.Log("Client received OnMovementOrder " + msg.entityId);
    }

    public void MovementOrder(int cellId, int entityId) {
        ClientMovementOrderMessage msg = new ClientMovementOrderMessage();
        msg.cellId = cellId;
        msg.entityId = entityId;
        msg.gameID = GameLogicClient.game.gameId;
        client.Send(ClientMovementOrderMessage.ID, msg);
        Debug.Log("Client sent MovementOrder ");
    }

    void OnGUI() {
        if (client != null && client.isConnected && user.isIdentified) {
            GUI.Label(new Rect(10, 20, 200, 20), "Identified as : " + user.userName);
            if (GUI.Button(new Rect(10, 60, 200, 20), "MasterClient Disconnect")) {
                ResetClient();
                if (NetworkManager.singleton != null) {
                    NetworkManager.singleton.StopClient();
                }
            }
            if (user.currentGameId == 0) {
                if (GUI.Button(new Rect(10, 100, 200, 20), "Join game")) {
                    ClientJoinGameRequest();
                }
            }
            else {
                if (GUI.Button(new Rect(10, 100, 200, 20), "Leave game")) {
                    ClientLeaveGameRequest();
                }
            }
        } else {
            user.userName = GUI.TextField(new Rect(10, 30, 200, 20), user.userName);
            if (GUI.Button(new Rect(10, 60, 200, 20), "MasterClient Connect")) {
                InitializeClient();
            }
            return;
        }
    }
}
