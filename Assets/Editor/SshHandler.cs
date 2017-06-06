using Renci.SshNet;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEditor;
using System;

[InitializeOnLoad]
public class SshHandler {

    private static ulong fileSize;

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
        // reset the progression log filter
        progression = 0;
        step = 1;

        float i = 0f;
        while (i < 1.1f) {
            
            Thread.Sleep(1000);
            i = i + 0.1f;
        }
        EditorUtility.ClearProgressBar();

        if (sftpclient.IsConnected) {
            var fileStream = new FileStream(path, FileMode.Open);
            if (fileStream != null) { 
                string[] fileSplit = fileStream.Name.Split('/');
                string fileName = fileSplit[fileSplit.Length - 1];
                fileSize = (ulong) fileStream.Length;
                
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    Debug.Log("Starting upload");
                    sftpclient.UploadFile(fileStream, "/root/gdwc/" + fileName, UpdateProgressBar);
                    Debug.Log("Finished upload");
                    SendCommand(stream, "unzip gdwc.zip");
                    SendCommand(stream, "chmod +x gdwc.x86_64");
                }).Start();
            }
        }
    }

    private static ulong progression = 0;
    private static ulong step = 1;

    private static void UpdateProgressBar(ulong uploaded) {
        progression += uploaded;
        ulong percentage = (ulong) ((float)progression / (float) fileSize * 100);
        if (percentage >= 20 * step) {
            Debug.Log("Uploading build to server... " + percentage + " %");
            step++;
        }
    }
}
