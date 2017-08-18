using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScene : MonoBehaviour {

    // Use this for initialization
    void Start() {
        if (NetworkMasterServer.isHeadless()) {
            GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/ServerHolder"), new Vector3(), Quaternion.identity).transform.parent = transform;
        } else {
            DataParser.loadDataJSON();
            GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/ClientHolder"), new Vector3(), Quaternion.identity).transform.parent = transform;
        }
    }

    // Update is called once per frame
    void Update() {

    }
}
