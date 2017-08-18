using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// client to server
// ID = 1xxx

public class ClientMovementOrderMessage : ClientMessage {
    public static short ID = 1102;
    public int cellId;
    public int entityId;
    public ulong gameId;
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

public class ClientIdentificationRequestMessage : ClientMessage {
    public static short ID = 1113;
    public string userName;
    public ulong userId;
}

public class ClientJoinGameRequestMessage : ClientMessage {
    public static short ID = 1114;
    public ulong userId;
    public string userName;
}

public class ClientLeaveGameRequestMessage : ClientMessage {
    public static short ID = 1115;
    public ulong userId;
    public string userName;
}


// server to client
// ID = 2xxx


public class ServerStartTurnMessage : ServerMessage {
    public static short ID = 2101;
    public int turnNumber;
    public ulong endTurnTimeStamp;
    public ulong gameId;
}

public class ServerMovementOrderMessage : ServerMessage {
    public static short ID = 2102;
    public int cellId;
    public int entityId;
    public ulong gameId;
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

public class ServerIdentificationResponseMessage : ServerMessage {
    public static short ID = 2113;
    public bool isSuccessful;
    public ulong userId;
    public string userName; // temporary : user informations will be fetched from DB
    public ulong currentGameId;
}

public class ServerJoinGameResponseMessage : ServerMessage {
    public static short ID = 2114;
    public ulong gameId;
    public ulong clientPlayerId;
    public int[] cellIds;
    public ulong[] playerIds;
    public int[] entityIds;
    public string[] displayedNames;
    public float[] r, g, b;
    public bool hasJoined;
}

public class ServerPlayerJoinedMessage : ServerMessage {
    public static short ID = 2115;
    public int cellId;
    public ulong playerId;
    public int entityId;
    public string displayedName;
    public float r, g, b;
}

public class ServerLeaveGameResponseMessage : ServerMessage {
    public static short ID = 2116;
    public bool hasLeft;
}

public class ServerPlayerLeftGameMessage : ServerMessage {
    public static short ID = 2117;
    public ulong playerId;
    public string playerName;
}


