using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using UnityEngine.SceneManagement;

public class NetworkMasterClient : MonoBehaviour {

    public User user;

    public string MasterServerIpAddress;
    public int MasterServerPort;
    public NetworkClient client = null;

    public static NetworkMasterClient singleton;

    public bool isConnected {
        get {
            return client != null;
        }
    }

    private void Awake() {
        if (singleton == null) {
            singleton = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        user = Login.localUser;
        InitializeClient();
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


        // queue
        client.RegisterHandler(ServerJoinSoloQueueResponseMessage.ID, OnClientJoinSoloQueue);
        client.RegisterHandler(ServerStartGameMessage.ID, OnClientStartGame);
        client.RegisterHandler(ServerStartNewTurnMessage.ID, OnClientStartNewTurn);
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
                ClientJoinGameRequest();
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
        Debug.Log("Sent join solo queue request");
    }

    private void OnClientJoinSoloQueue(NetworkMessage netMsg) {
        ServerJoinSoloQueueResponseMessage msg = netMsg.ReadMessage<ServerJoinSoloQueueResponseMessage>();
        if (msg.joinedQueue) {
            Debug.Log("Successfully joined solo queue.");
        } else {
            Debug.LogError("Failed to join solo queue.");
        }
    }


    private void ClientJoinGameRequest() {
        ClientJoinGameRequestMessage msg = new ClientJoinGameRequestMessage();
        msg.userId = user.userId;
        msg.userName = user.userName;
        msg.gameId = user.currentGameId;
        client.Send(ClientJoinGameRequestMessage.ID, msg);
        Debug.Log("Sent join game request");
    }

    private void OnClientJoinGame(NetworkMessage netMsg) {
        ServerJoinGameResponseMessage msg = netMsg.ReadMessage<ServerJoinGameResponseMessage>();

        if(msg.hasJoined) {
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
                if(msg.clientPlayerId == playerId) {
                    user.player = temp;
                }
            }
            ClientReadyToPlay();
            Debug.Log("Received ServerJoinGameResponseMessage " + msg.gameId);
        }
        else {
            Debug.LogError("Failed to join game.");
        }
    }

    void ClientReadyToPlay() {
        ClientReadyToPlayMessage msg = new ClientReadyToPlayMessage();
        msg.userId = user.userId;
        msg.userName = user.userName;
        msg.gameId = user.currentGameId;
        client.Send(ClientReadyToPlayMessage.ID, msg);
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
    private void OnClientCreateNewPlayer(NetworkMessage netMsg) {
        ServerPlayerJoinedMessage msg = netMsg.ReadMessage<ServerPlayerJoinedMessage>();
        CreatePlayer(msg.playerId, msg.cellId, msg.entityId, msg.displayedName, msg.r, msg.g, msg.b);
        Debug.Log(msg.displayedName + " has joined the game.");
    }

    private Player CreatePlayer(ulong playerId, int cellId, int entityId, string displayedName, float r, float g, float b) {
        if (!GameLogicClient.game.players.ContainsKey(playerId)) {
            Cell cell = GameLogicClient.game.grid.GetCell(cellId);
            Player p = new Player();
            p.playerId = playerId;
            p.playerName = displayedName;
            GameLogicClient.game.spawnPlayer(p);
            p.playerEntity.setColor(r, g, b);
            p.playerEntity.applyColor();
            p.playerEntity.setDisplayedName(displayedName);
            p.playerEntity.setCurrentCell(cell);
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
    private void OnMovementOrder(NetworkMessage netMsg) {
        ServerMovementOrderMessage msg = netMsg.ReadMessage<ServerMovementOrderMessage>();
        GameLogicClient.game.entityList[msg.entityId].addOrder(new MovementOrder(msg.cellId, msg.entityId));
        GameLogicClient.game.resolveAction(new MovementOrder(msg.cellId, msg.entityId));
        Debug.Log("Client received OnMovementOrder " + msg.entityId);
    }

    public void MovementOrder(int cellId, int entityId) {
        ClientMovementOrderMessage msg = new ClientMovementOrderMessage();
        msg.cellId = cellId;
        msg.entityId = entityId;
        msg.gameId = GameLogicClient.game.gameId;
        client.Send(ClientMovementOrderMessage.ID, msg);
        Debug.Log("Client sent MovementOrder ");
    }

    private void OnGUI() {
        if (client != null && client.isConnected && user.isIdentified) {
            GUI.Label(new Rect(10, 20, 200, 20), "Identified as : " + user.userName);
            if (GUI.Button(new Rect(10, 60, 200, 20), "Disconnect")) {
                ResetClient();
                if (NetworkManager.singleton != null) {
                    NetworkManager.singleton.StopClient();
                }
                SceneManager.LoadScene("login");
            }
            if (user.currentGameId == 0) {
                if (user.isQueued) {
                    if (GUI.Button(new Rect(10, 100, 200, 20), "Join solo queue")) {
                        ClientJoinSoloQueueRequest();
                    }
                } else {
                    if (GUI.Button(new Rect(10, 100, 200, 20), "Leave solo queue")) {
                        ClientLeaveSoloQueueRequest();
                    }
                }
            } else {
                if (GUI.Button(new Rect(10, 100, 200, 20), "Leave game")) {
                    ClientLeaveGameRequest();
                }
                if(GameLogicClient.game != null) {
                    GUI.Label(new Rect(10, 180, 200, 20), "Turn : " + GameLogicClient.game.currentTurn);
                    GUI.Label(new Rect(10, 260, 200, 20), "Time remaininig : " + GameLogicClient.game.secondRemaining);
                }
            }
        }
    }
}
