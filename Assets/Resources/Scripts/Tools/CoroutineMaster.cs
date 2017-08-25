using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineMaster : MonoBehaviour {

    public static CoroutineMaster singleton;

    void Start() {
        singleton = this;
    }

    public static void startCoroutine(IEnumerator e) {
        singleton.StartCoroutine(e);
    }

}
