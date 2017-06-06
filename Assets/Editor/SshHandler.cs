using Renci.SshNet;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class SshHandler {

    static SshHandler() {
        sshclient = new SshClient("217.182.205.172", "root", "4uzPqvqU");
        sshclient.Connect();
        stream = sshclient.CreateShellStream("customCommand", 80, 24, 800, 600, 1024);
        sftpclient = new SftpClient("217.182.205.172", "root", "4uzPqvqU");
        sftpclient.Connect();
    }

    public static SshClient sshclient;
    public static SftpClient sftpclient;
    public static ShellStream stream;

    public static string SendCommand(ShellStream stream, string customCMD) {
        StringBuilder strAnswer = new StringBuilder();

        var reader = new StreamReader(stream);
        var writer = new StreamWriter(stream);
        writer.AutoFlush = true;
        WriteStream(customCMD, writer, stream);

        strAnswer.AppendLine(ReadStream(reader));

        string answer = strAnswer.ToString();
        return answer.Trim();
    }

    private static void WriteStream(string cmd, StreamWriter writer, ShellStream stream) {
        writer.WriteLine(cmd);
        while (stream.Length == 0)
            Thread.Sleep(500);
    }

    private static string ReadStream(StreamReader reader) {
        StringBuilder result = new StringBuilder();

        string line;
        while ((line = reader.ReadLine()) != null)
            result.AppendLine(line);

        return result.ToString();
    }

    public static void sendFile(string path) {
        if (sftpclient.IsConnected) {
            var fileStream = new FileStream(path, FileMode.Open);
            if (fileStream != null) {
                string[] fileSplit = fileStream.Name.Split('/');
                string fileName = fileSplit[fileSplit.Length - 1];
                sftpclient.UploadFile(fileStream, "/root/gdwc/" + fileName, null);
            }
        }
    }
}
