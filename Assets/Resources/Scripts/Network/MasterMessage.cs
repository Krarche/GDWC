using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Network {

    public abstract class ClientMessage : MessageBase {
        public int emitterId; // == 0 is server, > 0 is player
    }

    public abstract class ServerMessage : MessageBase {
        public int emitterId;
    }
}