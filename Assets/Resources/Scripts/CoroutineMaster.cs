﻿using System.Collections;
using UnityEngine;

namespace Tools {

    public class CoroutineMaster : MonoBehaviour {

        public static CoroutineMaster singleton;

        void Start() {
            singleton = this;
        }

        public static void startCoroutine(IEnumerator e) {
            singleton.StartCoroutine(e);
        }
    }
}
