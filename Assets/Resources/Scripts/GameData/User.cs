using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User {

    public Player player;

    public static ulong userCount = 0;
    public ulong userId;

    public string userName;

    public bool isIdentified;
    public ulong currentGameId = 0; // if 0, then no current game

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

    public void identify(string userName) {
        this.userName = userName;
        this.isIdentified = true;
    }
}
