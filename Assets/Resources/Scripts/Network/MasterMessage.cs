using UnityEngine.Networking;

namespace Network {

    public abstract class ClientMessage : MessageBase {
        public int emitterId;
    }

    public abstract class ServerMessage : MessageBase {
        public int emitterId;
    }
}