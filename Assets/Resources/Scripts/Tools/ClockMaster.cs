using Network;
using System;
using UnityEngine;
using UnityEngine.Networking;

public class ClockMaster : MonoBehaviour {

    public const int milliToNano = 10000;

    public static ClockMaster serverSingleton;
    public static ClockMaster clientSingleton;
    public static long serverCurrentTimeStamp {
        get {
            return serverSingleton.currentTimestamp;
        }
    }
    public static long clientCurrentTimeStamp {
        get {
            return clientSingleton.currentTimestamp;
        }
    }

    public bool isServerClock = false;
    public bool isClientClock = false;

    private bool isSyncing = false;

    public int getRTT() {
        if (isClientClock) {
            return NetworkMasterClient.singleton.client.GetRTT();
        }
        return 0;
    }
    private long currentTimestamp;
    public DateTime now {
        get {
            return DateTime.FromFileTimeUtc(currentTimestamp);
        }
    }

    private void Start() {
        currentTimestamp = DateTime.UtcNow.ToFileTimeUtc();
        if (isServerClock)
            serverSingleton = this;
        if (isClientClock)
            clientSingleton = this;
    }

    private void FixedUpdate() {
        currentTimestamp += (long)(TimeSpan.FromSeconds(Time.fixedDeltaTime).TotalMilliseconds * milliToNano);
    }

    private long startSyncUtcTimestamp;
    private long startSyncEmissionTime;
    private long endSyncUtcTimestamp;

    public void startSyncClient() {
        if (!isSyncing) {
            isSyncing = true;
            DateTime now = DateTime.UtcNow;
            startSyncUtcTimestamp = now.ToFileTimeUtc();
            NetworkMasterClient.singleton.ClientSyncClockRequest(currentTimestamp);
            startSyncEmissionTime = (long)(DateTime.UtcNow.Subtract(now).TotalMilliseconds * milliToNano);
            Debug.Log("Synchronization starts" + ".\n"
                + "Emission time was " + startSyncEmissionTime + ".\n"
                + "getRTT is " + getRTT() + ".");
        } else {
            Debug.LogWarning("Tried to start clock synchronization, but was already syncing.");
        }
    }

    public void endSyncClient(long serverCurrentTimestamp) {
        if (isSyncing) {
            isSyncing = false;
            DateTime now = DateTime.UtcNow;
            endSyncUtcTimestamp = now.ToFileTimeUtc();
            long syncDuration = endSyncUtcTimestamp - startSyncUtcTimestamp;
            long RTT = getRTT();// syncDuration - startSyncEmissionTime; // convert into RTT
            long correction = (serverCurrentTimestamp + syncDuration / 2) - currentTimestamp;
            currentTimestamp += correction;
            Debug.Log("Synchronization ends" + ".\n"
                + "Sync duration was " + syncDuration + ".\n"
                + "RTT was " + RTT/ milliToNano + ".\n"
                + "getRTT is " + getRTT() + ".\n"
                + "Correction is " + correction + ".");
        } else {
            Debug.LogError("Tried to end clock synchronization, but wasn't syncing.");
        }
    }

    private void OnGUI() {
        if (isServerClock) {
            GUI.Label(new Rect(200, 0, 200, 100), "Server time :" + "\n"
                + currentTimestamp + "\n"
                + now.ToLongTimeString());
            if (clientSingleton != null)
                GUI.Label(new Rect(200, 60, 200, 20), "Client-Server delta :" + clientSingleton.now.Subtract(now).TotalSeconds);
        }
        if (isClientClock) {
            GUI.Label(new Rect(200, 100, 200, 100), "Client time :" + "\n"
                + currentTimestamp + "\n"
                + now.ToLongTimeString() + "\n"
                + "RTT : " + getRTT());
            if (serverSingleton != null)
                GUI.Label(new Rect(200, 160, 200, 20), "Server-Client delta :" + serverSingleton.now.Subtract(now).TotalSeconds);
            if(!isSyncing) {
                if (GUI.Button(new Rect(300, 100, 100, 20), "Start sync")) {
                    startSyncClient();
                }
            } else {
                GUI.Label(new Rect(300, 100, 200, 20), "Syncing...");
            }
        }
    }
}
