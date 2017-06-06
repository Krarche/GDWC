using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkMasterClient : MonoBehaviour {
    public bool dedicatedServer;
    public string MasterServerIpAddress;
    public int MasterServerPort;
    public int updateRate;
    public int gamePort;

    public NetworkClient client = null;

    static public NetworkMasterClient singleton;

    // Temporary
    public string playerName = "UnKnown";

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

        // system msgs
        client.RegisterHandler(MsgType.Connect, OnClientConnect);
        client.RegisterHandler(ServerIdentificationMessage.ID, OnClientIdentified);
        client.RegisterHandler(MsgType.Disconnect, OnClientDisconnect);
        client.RegisterHandler(MsgType.Error, OnClientError);

        // application msgs
        client.RegisterHandler(ServerCreatePlayerMessage.ID, OnCreatePlayer);
        client.RegisterHandler(ServerCreateGameMessage.ID, OnCreateGame);
        client.RegisterHandler(ServerMovementOrderMessage.ID, OnMovementOrder);
        
        DontDestroyOnLoad(gameObject);
    }

    public void ResetClient() {
        if (client == null)
            return;

        client.Disconnect();
        client = null;

        GameLogicClient.game.map.clearGrid();

        foreach (Player p in GameLogicClient.game.playerList.Values) {
            GameLogicClient.game.removeEntity(p.entity);
        }

        GameLogicClient.game.playerList.Clear();
    }

    public bool isConnected {
        get {
            if (client == null)
                return false;
            else
                return client.isConnected;
        }
    }

    // --------------- System Handlers -----------------

    void OnClientConnect(NetworkMessage netMsg) {
        Debug.Log("Client Connected to Master");
        ClientIdentification();
    }

    void OnClientIdentified(NetworkMessage netMsg) {
        Debug.Log("Client identified");
        ServerIdentificationMessage msg = netMsg.ReadMessage<ServerIdentificationMessage>();
        GameLogicClient.localPlayerId = msg.playerId;
    }

    void ClientIdentification() {
        ClientIdentificationMessage msg = new ClientIdentificationMessage();
        msg.playerName = playerName;
        client.Send(ClientIdentificationMessage.ID, msg);
        Debug.Log("Client sent ClientIdentification ");
    }

    void OnClientDisconnect(NetworkMessage netMsg) {
        Debug.Log("Client Disconnected from Master");
    }

    void OnClientError(NetworkMessage netMsg) {
        Debug.Log("ClientError from Master");
        OnFailedToConnectToMasterServer();
    }

    // --------------- Application Handlers -----------------

    void OnChangedCellColor(NetworkMessage netMsg) {
        //var msg = netMsg.ReadMessage<MasterMsgTypes.ChangedCellColorMessage> ();
    }

    public void ClearHostList() {
        if (!isConnected) {
            Debug.LogError("ClearHostList not connected");
            return;
        }

    }

    public virtual void OnFailedToConnectToMasterServer() {
        Debug.Log("OnFailedToConnectToMasterServer");
    }

    void OnCreatePlayer(NetworkMessage netMsg) {
        ServerCreatePlayerMessage msg = netMsg.ReadMessage<ServerCreatePlayerMessage>();

        if (!GameLogicClient.game.containsPlayerId(msg.playerId)) {
            Entity e = GameLogicClient.game.createEntity(msg.cellId);
            GameLogicClient.game.playerList[msg.playerId] = new Player(msg.playerId, e);
            Debug.Log("Client received OnCreatePlayer " + msg.playerId);
        } else {
            Debug.Log("Client received OnCreatePlayer " + msg.playerId + " WARNING - DUPLICATED ID");
        }
    }

    void OnCreateGame(NetworkMessage netMsg) {
        ServerCreateGameMessage msg = netMsg.ReadMessage<ServerCreateGameMessage>();
        GameLogicClient game = new GameLogicClient();
        game.id = msg.gameId;
        Debug.Log("Client received OnCreateGame " + msg.gameId);
    }

    void OnMovementOrder(NetworkMessage netMsg) {
        ServerMovementOrderMessage msg = netMsg.ReadMessage<ServerMovementOrderMessage>();
        GameLogicClient.game.playerList[msg.playerId].addOrder(new MovementOrder(msg.cellId));
        Debug.Log("Client received OnMovementOrder ");
    }

    public void MovementOrder(int cellId, ulong playerId) {
        ClientMovementOrderMessage msg = new ClientMovementOrderMessage();
        msg.cellId = cellId;
        msg.playerId = playerId;
        msg.gameId = GameLogicClient.game.id;
        client.Send(ClientMovementOrderMessage.ID, msg);
        Debug.Log("Client sent MovementOrder ");
    }



    void OnGUI() {
        if (client != null && client.isConnected) {
            if (GUI.Button(new Rect(0, 60, 200, 20), "MasterClient Disconnect")) {
                ResetClient();
                if (NetworkManager.singleton != null) {
                    NetworkManager.singleton.StopClient();
                }
            }
        } else {
            playerName = GUI.TextField(new Rect(10, 30, 200, 20), playerName);
            if (GUI.Button(new Rect(10, 60, 200, 20), "MasterClient Connect")) {
                InitializeClient();
            }
            return;
        }
    }
}
