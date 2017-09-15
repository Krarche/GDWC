using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;
using Tools;
using Network;

public class LoadingScene : MonoBehaviour {

    public bool forceServer = false;

    // Use this for initialization
    void Start() {
        DataParser.loadDataJSON();
        if (NetworkMasterServer.isHeadless()) {
            GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/ServerHolder"), new Vector3(), Quaternion.identity).transform.parent = transform;
        } else if(forceServer) {
            GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/ServerHolder"), new Vector3(), Quaternion.identity).transform.parent = transform;
            GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/ClientHolder"), new Vector3(), Quaternion.identity).transform.parent = transform;
        }
        else {
            GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/ClientHolder"), new Vector3(), Quaternion.identity).transform.parent = transform;
        }
    }
}
