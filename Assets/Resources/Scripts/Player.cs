using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public static int playerCount = 0;
    public int playerId = 0;
    public Entity entity = null;
    public ulong currentGameId = 0;

    public Player() {
        playerId = playerCount;
        playerCount++;
    }

    public Player (int playerId, Entity e) {
        this.playerId = playerId;
        this.entity = e;
    }

    public void addOrder(Order o) {
        entity.addOrder(o);
    }

    public int getCurrentCell() {
        return 0;
    }
}
