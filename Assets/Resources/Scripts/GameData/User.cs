using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class User {

    public Player player;
    public NetworkConnection connection;
    public static ulong userCount = 0;

    public ulong userId;
    public string userName;
    public string userToken;

    public bool isIdentified;

    public bool isQueued;
    public DateTime joinedQueueTime;
    public double timeSpentInQueue {
        get {
            DateTime now = DateTime.UtcNow;
            TimeSpan queueToNow = now.Subtract(joinedQueueTime);
            return queueToNow.TotalSeconds;
        }
    }

    public ulong currentGameId = 0; // if 0, then no current game

    public int MMR = 1000;

    public User() {
        userId = userCount;
        userCount++;
    }

    // only for client
    public User(string userName) {
        this.userName = userName;
    }

     // Temporary
    public User(ulong userId, string userName) {
        this.userId = userId;
        this.userName = userName;
    }

    public void identify(string userName, ulong userId) {
        this.userName = userName;
        this.userId = userId;
        this.isIdentified = true;
    }
}