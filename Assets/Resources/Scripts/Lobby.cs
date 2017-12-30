using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lobby : MonoBehaviour {

    public LobbyViewController lobbyViewController;

    // Use this for initialization
    void Start() {
        LoadLobby(SceneMaster.args);
    }

    public void LoadLobby(Dictionary<string, object> args) {
        if (args.ContainsKey("scoreScreen"))
            LoadLobbyScoreScreen(args);
    }

    public void LoadLobbyScoreScreen(Dictionary<string, object> args) {
        GameMode gameMode = (GameMode)args["gameMode"];
        if (gameMode == GameMode.Mode1v1)
            LoadLobbyScoreScreenMode1v1(args);
    }

    public void LoadLobbyScoreScreenMode1v1(Dictionary<string, object> args) {
        GameMode gameMode = (GameMode)args["gameMode"];
        Team winningTeam = (Team)args["winningTeam"];
        Team losingTeam = (Team)args["losingTeam"];
        List<Entity> team1Entities = (List<Entity>)args["team1Entities"];
        List<Entity> team2Entities = (List<Entity>)args["team2Entities"];
        List<Entity> killedEntities = (List<Entity>)args["killedEntities"];
        int totalTurnCount = (int)args["totalTurnCount"];
        lobbyViewController.SwitchToScoreScreenPanel();
    }

    // Update is called once per frame
    void Update() {

    }
}
