
// client to server
// ID = 1xxx

// identification 1000

namespace Network {

    public class ClientIdentificationRequestMessage : ClientMessage {
        public static short ID = 1000;
        public string userName;
        public ulong userId;
    }

    // queue 1100

    public class ClientJoinSoloQueueRequestMessage : ClientMessage {
        public static short ID = 1100;
        public ulong userId;
        public string userName;
    }

    public class ClientLeaveSoloQueueRequestMessage : ClientMessage {
        public static short ID = 1101;
        public ulong userId;
        public string userName;
    }

    // game 1200

    public class ClientReadyToPlayMessage : ClientMessage {
        public static short ID = 1200;
        public ulong userId;
        public string userName;
        public ulong gameId;
    }

    // turn 1300

    public class ClientRegisterTurnActionsMessage : ClientMessage {
        public static short ID = 1201;
        public ulong userId;
        public string userName;
        public ulong gameId;
        public ulong playerId;
        public ulong entityId;
        public string actions;
        // actions
    }


    // server to client
    // ID = 2xxx

    // identification 2000

    public class ServerStartTurnMessage : ServerMessage {
        public static short ID = 2101;
        public int turnNumber;
        public long endTurnTimestamp;
        public ulong gameId;
    }

    public class ServerIdentificationResponseMessage : ServerMessage {
        public static short ID = 2113;
        public bool isSuccessful;
        public ulong userId;
        public string userName; // temporary : user informations will be fetched from DB
        public ulong currentGameId;
    }

    // game 2200

    public class ServerJoinGameResponseMessage : ServerMessage {
        public static short ID = 2114;
        public ulong gameId;
        public string mapId;
        public int currentTurn;
        public ulong clientPlayerId;
        public int[] currentHP, maxHP;
        public int[] currentMP, maxMP;
        public int[] currentAP, maxAP;
        public int[] cellIds;
        public ulong[] playerIds;
        public int[] entityIds;
        public string[] displayedNames;
        public float[] r, g, b;
        public string[] spellIds;
        public bool hasJoined;

        public void initArrays(int size) {
            currentHP = new int[size];
            maxHP = new int[size];
            currentMP = new int[size];
            maxMP = new int[size];
            currentAP = new int[size];
            maxAP = new int[size];
            cellIds = new int[size];
            playerIds = new ulong[size];
            entityIds = new int[size];
            displayedNames = new string[size];
            r = new float[size];
            g = new float[size];
            b = new float[size];
            spellIds = new string[size];
        }
    }

    // queue

    public class ServerJoinSoloQueueResponseMessage : ServerMessage {
        public static short ID = 2200;
        public bool joinedQueue;
    }

    // game

    public class ServerStartGameMessage : ServerMessage {
        public static short ID = 2300;
        public long startFirstTurnTimestamp;
        // timestamp du début du premier tour
    }

    public class ServerEndGameMessage : ServerMessage {
        public static short ID = 2303;
        public bool joinedQueue;
        // résultats
    }

    // turn

    public class ServerStartNewTurnMessage : ServerMessage {
        public static short ID = 2301;
        public long startTurnTimestamp;
        // timestamp du début du nouveau tour
    }

    public class ServerSyncTurnActionsMessage : ServerMessage {
        public static short ID = 2302;
        public string actions;
        // actions de tous les joueurs
    }
}