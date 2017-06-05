using Renci.SshNet;
using System.IO;
using System.IO.Compression;
using System.Threading;
using UnityEditor;
using UnityEngine;

public class ScriptBatch {

    [MenuItem("Server/Build and push %&b")]
    public static void BuildAndPushToServer() {
        BuildServer();
        PushToServer();
    }

    [MenuItem("Server/Build %&c")]
    public static void BuildServer() {
        // Get filename.
        string path = "./Build/Server";

        GameObject clientHolder = GameObject.Find("ClientHolder");
        clientHolder.GetComponent<NetworkMasterClient>().MasterServerIpAddress = "127.0.0.1";

        string[] levels = new string[] { "Assets/main.unity" };

        BuildPipeline.BuildPlayer(levels, path + "/gdwc.x86_64", BuildTarget.StandaloneLinux64, BuildOptions.EnableHeadlessMode);

        using (var zip = new Ionic.Zip.ZipFile()) {
            if (File.Exists("./Build/Server/gdwc.zip")) {
                File.Delete("./Build/Server/gdwc.zip");
            }
            zip.AddDirectory("./Build/Server");
            zip.Save("./Build/Server/gdwc.zip");
        }
    }

    [MenuItem("Server/Push %&p")]
    public static void PushToServer() {
        SshHandler.initConnection();

        SshHandler.SendCommand(SshHandler.stream, "pkill gdwc");
        SshHandler.SendCommand(SshHandler.stream, "rm -rf ~/gdwc");
        SshHandler.SendCommand(SshHandler.stream, "mkdir gdwc && cd gdwc");

        if (File.Exists("./Build/Server/gdwc.zip")) {
            SshHandler.sendFile("./Build/Server/gdwc.zip");
            SshHandler.SendCommand(SshHandler.stream, "unzip gdwc.zip");
            Thread.Sleep(500);
            SshHandler.SendCommand(SshHandler.stream, "chmod +x gdwc.x86_64");
        }
        Thread.Sleep(500);
        SshHandler.disconnect();
    }

    [MenuItem("Server/Status/Start %&s")]
    public static void StartServer() {
        SshHandler.initConnection();
        SshHandler.SendCommand(SshHandler.stream, "cd gdwc && nohup ./gdwc.x86_64 &");
        Thread.Sleep(500);
        SshHandler.disconnect();
        Debug.Log("Server started");
    }

    [MenuItem("Server/Status/Stop %&x")]
    public static void StopServer() {
        SshHandler.initConnection();
        SshHandler.SendCommand(SshHandler.stream, "pkill gdwc");
        Thread.Sleep(500);
        SshHandler.disconnect();
        Debug.Log("Server stopped");
    }

    [MenuItem("Server/Status/Restart %&r")]
    public static void RestartServer() {
        SshHandler.initConnection();
        SshHandler.SendCommand(SshHandler.stream, "cd gdwc && pkill gdwc && nohup ./gdwc.x86_64 &");
        Thread.Sleep(500);
        SshHandler.disconnect();
        Debug.Log("Server restarted");
    }
}