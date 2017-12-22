using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;
using Tools;
using Network;

public class EntryPoint : MonoBehaviour {

    public bool isServer = false;
    public bool forceSoloGame = false;
    public bool isClient = true;

    void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    // Use this for initialization
    void Start() {
        loadData();
        StartCoroutine(DoStart());
    }

    IEnumerator DoStart() {
        yield return new WaitForSeconds(1.0f);
        startServer();
        startClient();
    }

    void loadData() {
        DataParser.loadDataJSON();
    }

    void startServer() {
        if (isServer) {
            GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/ServerHolder"), new Vector3(), Quaternion.identity).transform.parent = transform;
            NetworkMasterServer.singleton.forceSoloGame = forceSoloGame;
        }
    }

    void startClient() {
        if (isClient) {
            GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/ClientHolder"), new Vector3(), Quaternion.identity).transform.parent = transform;
            SceneMaster.singleton.SwitchToLobby();
        }
    }
}
