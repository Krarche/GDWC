using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// client to server
// ID = 1xxx

public class ClientMovementOrderMessage : ClientMessage
{
    public static short ID = 1102;
    public int cellId;
    public int entityId;
}

public class ClientSendPathMessage : ClientMessage {
	public static short ID = 1111;
	public int[] path; // path cells ids, from current position cell to new position cell
}

public class ClientSendSpellMessage : ClientMessage {
	public static short ID = 1112;
	public int spellId; // id of spell used
	public int cellId; // id of the cell targeted
}

public class ClientIdentificationMessage : ClientMessage {
    public static short ID = 1113;
    public string playerName;
}


// server to client
// ID = 2xxx

public class ServerCreatePlayerMessage : ServerMessage
{
    public static short ID = 2101;
    public int cellId;
    public ulong playerId;
    public int entityId;
    public string displayedName;
    public float r, g, b;
}

public class ServerCreateGameMessage : ServerMessage {
    public static short ID = 2102;
}

public class ServerMovementOrderMessage : ServerMessage
{
    public static short ID = 2103;
    public int cellId;
    public int entityId;
}

public class ServerSendTurnActionsMessage : ServerMessage {
    public static short ID = 2110;
    public int[][] actions; // {action type(move/spell), player id, priority (first, normal, last), datas}
}

public class ServerSendAllyPathPrevisualisationMessage : ServerMessage {
	public static short ID = 2111;
	public ulong playerId;
	public int[] path; // path cells ids, from current position cell to new position cell
}

public class ServerSendAllySpellPrevisualisationMessage : ServerMessage {
	public static short ID = 2112;
	public ulong playerId;
	public int spellId; // id of spell used
	public int cellId; // id of the cell targeted
}

public class ServerIdentificationMessage : ServerMessage {
    public static short ID = 2113;
    public bool state;
    public ulong playerId;
}


