using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class CellChangeColorMessage : ClientMessage {
	public static short ID = 1500;
	public float r, g, b, a;
	public int[] cellId;

	public void SetColor(int r, int b, int g, int a = 1) {
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
	}

	public void SetColor(Vector3 color) {
		this.r = color.x;
		this.g = color.y;
		this.b = color.z;
		this.a = 1;
	}

	public void SetColor(Vector4 color) {
		this.r = color.x;
		this.g = color.y;
		this.b = color.z;
		this.a = color.w;
	}

	public void SetColor(Color color) {
		this.r = color.r;
		this.g = color.g;
		this.b = color.b;
		this.a = color.a;
	}

	public Color GetColor() {
		return new Color (r, g, b, a);
	}
}

// client to server

public class ClientSendPathMessage : ClientMessage {
	public static short ID = 1100;
	public int[] path; // path cells ids, from current position cell to new position cell
}

public class ClientSendSpellMessage : ClientMessage {
	public static short ID = 1101;
	public int spellId; // id of spell used
	public int cellId; // id of the cell targeted
}


// server to client

public class ServerSendAllyPathPrevisualisationMessage : ServerMessage {
	public static short ID = 2100;
	public int playerId;
	public int[] path; // path cells ids, from current position cell to new position cell
}

public class ServerSendAllySpellPrevisualisationMessage : ServerMessage {
	public static short ID = 2101;
	public int playerId;
	public int spellId; // id of spell used
	public int cellId; // id of the cell targeted
}

public class ServerSendTurnActionsMessage : ServerMessage {
	public static short ID = 2102;

	public int[][] actions; // {action type(move/spell), player id, priority (first, normal, last), datas}

}


