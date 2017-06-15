using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScene : MonoBehaviour {

    // Use this for initialization
    void Start() {
        if (NetworkMasterServer.isHeadless()) {
            GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/ServerHolder"), new Vector3(), Quaternion.identity).transform.parent = transform;
        } else {
            GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/ClientHolder"), new Vector3(), Quaternion.identity).transform.parent = transform;
        }
        DataParser.buildSpellAndBuffData();

        Debug.Log(DataParser.BUFF_DATA["B001"].description);
        Debug.Log(DataParser.SPELL_DATA["S001"].description);
        Debug.Log(DataParser.SPELL_DATA["S002"].description);
    }

    // Update is called once per frame
    void Update() {

    }
}
