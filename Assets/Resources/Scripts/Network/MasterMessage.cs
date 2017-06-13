using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public abstract class ClientMessage : MessageBase {
	public int emitterId; // == 0 is server, > 0 is player
    public ulong gameId;
}

public abstract class ServerMessage : MessageBase {
	public int emitterId;
    public ulong gameId;
}

// client to server
public class ClientSendByteArray : ClientMessage {
	public static short ID = 1000;
	public byte[] array;
}

// server to client
public class ServerSendByteArray : ServerMessage {
	public static short ID = 2000;
	public byte[] array;
}


