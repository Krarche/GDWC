using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;


public class NetworkMasterServer : MonoBehaviour {
	public int MasterServerPort;
    public int connectionCount = 0;


	// map of gameTypeNames to rooms of that type
	// Dictionary<string, Player> gameTypeRooms = new Dictionary<string, Rooms> ();

	public void InitializeServer () {
		if (NetworkServer.active) {
			Debug.LogError ("Already Initialized");
			return;
		}

		NetworkServer.Listen (MasterServerPort);

		// system msgs
		NetworkServer.RegisterHandler (MsgType.Connect, OnServerConnect);
		NetworkServer.RegisterHandler (MsgType.Disconnect, OnServerDisconnect);
		NetworkServer.RegisterHandler (MsgType.Error, OnServerError);

		// application msgs
		NetworkServer.RegisterHandler (CellChangeColorMessage.ID, OnCellChangeColor);
        NetworkServer.RegisterHandler(ClientMovementOrderMessage.ID, OnClientMovementOrder);

		DontDestroyOnLoad (gameObject);
	}

	public void ResetServer () {
		NetworkServer.Shutdown ();
	}

	// --------------- System Handlers -----------------

	void OnServerConnect (NetworkMessage netMsg) {
		Debug.Log ("Master received client");

        TellClientId(netMsg.conn, connectionCount);
        for (int i = 0; i < Player.playerList.Count; i++)
            CreatePlayerForNewClient(netMsg.conn, Player.playerList[i].getCurrentCell());
        CreatePlayer(connectionCount);
        connectionCount++;
    }

	void OnServerDisconnect (NetworkMessage netMsg) {
		Debug.Log ("Master lost client");
	}

	void OnServerError (NetworkMessage netMsg) {
		Debug.Log ("ServerError from Master");
	}

	// --------------- Application Handlers -----------------

	void OnCellChangeColor (NetworkMessage netMsg)
    {
        CellChangeColorMessage msg = netMsg.ReadMessage<CellChangeColorMessage> ();
        Debug.Log("Server received OnCellChangeColor " + msg.GetColor().ToString());
        GameLogic.main.map.ClearSelection ();
		ChangeCellColor (msg.cellId, msg.GetColor ());
	}
    void OnClientMovementOrder(NetworkMessage netMsg)
    {
        ClientMovementOrderMessage msg = netMsg.ReadMessage<ClientMovementOrderMessage>();
        Debug.Log("Server received OnClientMovementOrder ");

        MovementOrder(msg.cellId, msg.playerId);
    }

    void OnGUI () {
		if (NetworkServer.active) {
			GUI.Label (new Rect (0, 0, 200, 20), "Online port:" + MasterServerPort);
			if (GUI.Button (new Rect (0, 20, 200, 20), "Reset  Master Server")) {
				ResetServer ();
			}
		} else {
			if (GUI.Button (new Rect (0, 20, 200, 20), "Init Master Server")) {
				InitializeServer ();
			}
		}
	}

	public void ChangeCellColor (int[] cellId, Color color) {
		CellChangeColorMessage msg = new CellChangeColorMessage ();
		msg.cellId = cellId;
		msg.SetColor (color);
		NetworkServer.SendToAll (CellChangeColorMessage.ID, msg);
		Debug.Log ("Server sent ChangeCellColor " + color.ToString ());
    }

    public void MovementOrder(int cellId, int playerId)
    {
        Player.playerList[playerId].orderMoveToCell(cellId);
        ServerMovementOrderMessage msg = new ServerMovementOrderMessage();
        msg.cellId = cellId;
        msg.playerId = playerId;
        NetworkServer.SendToAll(ServerMovementOrderMessage.ID, msg);
        Debug.Log("Server sent MovementOrder ");
    }

    public void TellClientId(NetworkConnection client, int playerId)
    {
        ServerTellClientPlayerIdMessage msg = new ServerTellClientPlayerIdMessage();
        msg.playerId = playerId;
        client.Send(ServerTellClientPlayerIdMessage.ID, msg);
        Debug.Log("Server sent TellClientId ");
    }

    public void CreatePlayer(int cellId)
    {
        GameLogic.main.createPlayer(cellId);
        ServerCreatePlayerMessage msg = new ServerCreatePlayerMessage();
        msg.cellId = cellId;
        NetworkServer.SendToAll(ServerCreatePlayerMessage.ID, msg);
        Debug.Log("Server sent CreatePlayer ");
    }

    public void CreatePlayerForNewClient(NetworkConnection client, int cellId)
    {
        ServerCreatePlayerMessage msg = new ServerCreatePlayerMessage();
        msg.cellId = cellId;
        client.Send(ServerCreatePlayerMessage.ID, msg);
        Debug.Log("Server sent CreatePlayerForNewClient ");
    }
}
