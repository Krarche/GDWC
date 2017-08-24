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
    public ulong gameId;
}

public class ClientLeaveGameRequestMessage : ClientMessage {
    public static short ID = 1115;
    public ulong userId;
    public string userName;
}

// queue

public class ClientJoinSoloQueueRequestMessage : ClientMessage {
    public static short ID = 1200;
    public ulong userId;
    public string userName;
}

public class ClientLeaveSoloQueueRequestMessage : ClientMessage {
    public static short ID = 1201;
    public ulong userId;
    public string userName;
}

public class ClientReadyToPlayMessage : ClientMessage {
    public static short ID = 1202;
    public ulong userId;
    public string userName;
    public ulong gameId;
}

public class ClientRegisterTurnActionsMessage : ClientMessage {
    public static short ID = 1203;
    public ulong userId;
    public string userName;
    public ulong gameId;
    // actions
}


// server to client
// ID = 2xxx


public class ServerStartTurnMessage : ServerMessage {
    public static short ID = 2101;
    public int turnNumber;
    public long endTurnTimestamp;
    public ulong gameId;
}

public class ServerMovementOrderMessage : ServerMessage {
    public static short ID = 2103;
    public int cellId;
    public int entityId;
    public ulong gameId;
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
    public string mapId;
    public int currentTurn;
    public ulong clientPlayerId;
    public int[] cellIds;
    public ulong[] playerIds;
    public int[] entityIds;
    public string[] displayedNames;
    public float[] r, g, b;
    public bool hasJoined;

    public void initArrays(int size) {
        cellIds = new int[size];
        playerIds = new ulong[size];
        entityIds = new int[size];
        displayedNames = new string[size];
        r = new float[size];
        g = new float[size];
        b = new float[size];
    }
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

// queue

public class ServerJoinSoloQueueResponseMessage : ServerMessage {
    public static short ID = 2200;
    public bool joinedQueue;
}

public class ServerStartGameMessage : ServerMessage {
    public static short ID = 2300;
    public long startFirstTurnTimestamp;
    // timestamp du début du premier tour
}

public class ServerStartNewTurnMessage : ServerMessage {
    public static short ID = 2301;
    public long startTurnTimestamp;
    // timestamp du début du nouveau tour
}

public class ServerSendTurnActionsMessage : ServerMessage {
    public static short ID = 2302;
    public bool joinedQueue;
    // actions de tous les joueurs
}

public class ServerEndGameMessage : ServerMessage {
    public static short ID = 2303;
    public bool joinedQueue;
    // résultats
}





