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
        Player.localPlayer = msg.playerId;
        Debug.Log("Client received OnMovementOrder ");
    }

    void OnCreatePlayer(NetworkMessage netMsg)
    {
        ServerCreatePlayerMessage msg = netMsg.ReadMessage<ServerCreatePlayerMessage>();
        GameLogic.main.createPlayer(msg.cellId);
        Debug.Log("Client received OnMovementOrder ");
    }

    void OnCellChangeColor (NetworkMessage netMsg) {
		CellChangeColorMessage msg = netMsg.ReadMessage<CellChangeColorMessage> ();
		GameLogic.main.map.ClearSelection ();
		GameLogic.main.map.SetCellColor (msg.cellId, msg.GetColor ());
		Debug.Log ("Client received OnCellChangeColor " + msg.GetColor ().ToString ());
	}
	public void ChangeCellColor (int[] cellId, Color color) {
		CellChangeColorMessage msg = new CellChangeColorMessage ();
		msg.cellId = cellId;
		msg.SetColor (color);
		client.Send (CellChangeColorMessage.ID, msg);
		Debug.Log ("Client sent ChangeCellColor " + color.ToString ());
	}

    void OnMovementOrder(NetworkMessage netMsg)
    {
        ServerMovementOrderMessage msg = netMsg.ReadMessage<ServerMovementOrderMessage>();
        Player.playerList[msg.playerId].orderMoveToCell(msg.cellId);
        Debug.Log("Client received OnMovementOrder ");
    }
    public void MovementOrder(int cellId, int playerId)
    {
        ClientMovementOrderMessage msg = new ClientMovementOrderMessage();
        msg.cellId = cellId;
        msg.playerId = playerId;
        client.Send(ClientMovementOrderMessage.ID, msg);
        Debug.Log("Client sent MovementOrder ");
    }

    void OnGUI () {
		if (client != null && client.isConnected) {
			if (GUI.Button (new Rect (0, 60, 200, 20), "MasterClient Disconnect")) {
				ResetClient ();
				if (NetworkManager.singleton != null) {
					NetworkManager.singleton.StopServer ();
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
