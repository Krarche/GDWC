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

	void Awake () {
		if (singleton == null) {
			singleton = this;
			DontDestroyOnLoad (gameObject);
		} else {
			Destroy (gameObject);
		}
	}

	public void InitializeClient () {
		if (client != null) {
			Debug.LogError ("Already connected");
			return;
		}

		client = new NetworkClient ();
		client.Connect (MasterServerIpAddress, MasterServerPort);

		// system msgs
		client.RegisterHandler (MsgType.Connect, OnClientConnect);
		client.RegisterHandler (MsgType.Disconnect, OnClientDisconnect);
		client.RegisterHandler (MsgType.Error, OnClientError);

        // application msgs
        client.RegisterHandler(ServerTellClientPlayerIdMessage.ID, OnPlayerIdTold);
        client.RegisterHandler(ServerCreatePlayerMessage.ID, OnCreatePlayer);
        client.RegisterHandler(ServerCreateGameMessage.ID, OnCreateGame);
        client.RegisterHandler (CellChangeColorMessage.ID, OnCellChangeColor);
        client.RegisterHandler (ServerMovementOrderMessage.ID, OnMovementOrder);
        DontDestroyOnLoad (gameObject);
	}

	public void ResetClient () {
		if (client == null)
			return;

		client.Disconnect ();
		client = null;
		//hosts = null;
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

	void OnClientConnect (NetworkMessage netMsg) {
		Debug.Log ("Client Connected to Master");
	}

	void OnClientDisconnect (NetworkMessage netMsg) {
		Debug.Log ("Client Disconnected from Master");
		ResetClient ();
		OnFailedToConnectToMasterServer ();
	}

	void OnClientError (NetworkMessage netMsg) {
		Debug.Log ("ClientError from Master");
		OnFailedToConnectToMasterServer ();
	}

	// --------------- Application Handlers -----------------

	void OnChangedCellColor(NetworkMessage netMsg) {
		//var msg = netMsg.ReadMessage<MasterMsgTypes.ChangedCellColorMessage> ();
	}

	public void ClearHostList () {
		if (!isConnected) {
			Debug.LogError ("ClearHostList not connected");
			return;
		}

	}

	public virtual void OnFailedToConnectToMasterServer () {
		Debug.Log ("OnFailedToConnectToMasterServer");
    }

    void OnPlayerIdTold(NetworkMessage netMsg)
    {
        ServerTellClientPlayerIdMessage msg = netMsg.ReadMessage<ServerTellClientPlayerIdMessage>();
        GameLogicClient.localPlayer = msg.playerId;
        Debug.Log("Client received OnMovementOrder ");
    }

    void OnCreatePlayer(NetworkMessage netMsg)
    {
        ServerCreatePlayerMessage msg = netMsg.ReadMessage<ServerCreatePlayerMessage>();
        Entity e = GameLogicClient.game.createEntity(msg.cellId);
        GameLogicClient.game.playerList.Add(new Player(msg.playerId, e));
        Debug.Log("Client received OnMovementOrder ");
    }

    void OnCreateGame(NetworkMessage netMsg) {
        ServerCreateGameMessage msg = netMsg.ReadMessage<ServerCreateGameMessage>();
        GameLogicClient game = new GameLogicClient();
        game.id = msg.gameId;
        Debug.Log("Client received OnCreateGame");
    }

    void OnCellChangeColor (NetworkMessage netMsg) {
		CellChangeColorMessage msg = netMsg.ReadMessage<CellChangeColorMessage> ();
        GameLogicClient.game.map.ClearSelection ();
        GameLogicClient.game.map.SetCellColor (msg.cellId, msg.GetColor ());
		Debug.Log ("Client received OnCellChangeColor " + msg.GetColor ().ToString ());
	}
	public void ChangeCellColor (int[] cellId, Color color) {
		CellChangeColorMessage msg = new CellChangeColorMessage ();
		msg.cellId = cellId;
		msg.SetColor (color);
        msg.gameId = GameLogicClient.game.id;
        client.Send (CellChangeColorMessage.ID, msg);
		Debug.Log ("Client sent ChangeCellColor " + color.ToString ());
	}

    void OnMovementOrder(NetworkMessage netMsg)
    {
        ServerMovementOrderMessage msg = netMsg.ReadMessage<ServerMovementOrderMessage>();
        GameLogicClient.game.playerList[msg.playerId].addOrder(new MovementOrder(msg.cellId));
        Debug.Log("Client received OnMovementOrder ");
    }
    public void MovementOrder(int cellId, int playerId)
    {
        ClientMovementOrderMessage msg = new ClientMovementOrderMessage();
        msg.cellId = cellId;
        msg.playerId = playerId;
        msg.gameId = GameLogicClient.game.id;
        client.Send(ClientMovementOrderMessage.ID, msg);
        Debug.Log("Client sent MovementOrder ");
    }

    void OnGUI () {
		if (client != null && client.isConnected) {
			if (GUI.Button (new Rect (0, 60, 200, 20), "MasterClient Disconnect")) {
				ResetClient ();
				if (NetworkManager.singleton != null) {
					//NetworkManager.singleton.StopServer ();
					NetworkManager.singleton.StopClient ();
				}
			}
		} else {
			if (GUI.Button (new Rect (0, 60, 200, 20), "MasterClient Connect")) {
				InitializeClient ();
			}
			return;
		}
	}
}
