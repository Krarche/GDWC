﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player {
    public static ulong playerCount = 0;
    public ulong playerId = 0;
    public Entity entity = null;
    public ulong currentGameId = 0;

    public string playerName;
    public Color playerColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    public bool isIdentified;

    public Player() {
        playerId = playerCount;
        playerCount++;
    }

    public Player(ulong playerId, Entity e) {
        this.playerId = playerId;
        this.entity = e;
    }

    public void addOrder(Order o) {
        entity.addOrder(o);
    }

    public int getCurrentCellId() {
        return entity.currentCellId;
    }

    public void identifie(string playerName) {
        this.playerName = playerName;
        this.isIdentified = true;
    }
}
