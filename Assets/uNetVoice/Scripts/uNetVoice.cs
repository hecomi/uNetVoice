﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace uNetVoice
{

public class uNetVoice : MonoBehaviour
{
    const int ChunkSize = 256;

    [SerializeField]
    NetworkManager networkManager;

    [SerializeField]
    VoiceRecorder recorder;

    [SerializeField]
    VoicePlayer player;

    [SerializeField]
    int micIndex = 0;

    [SerializeField]
    int channelId = 0;

    [SerializeField]
    bool playSelfSound = false;

    float[] buffer_ = new float[ChunkSize];
    bool hasStarted_ = false;

    NetworkClient client
    {
        get { return networkManager.client; }
    }

    bool isHost
    {
        get { return NetworkServer.connections.Count > 0; }
    }

    void Awake()
    {
        InitManager();
        InitRecorder();
        InitPlayer();
    }

    void OnDestroy()
    {
        StopVoice();
    }

    void Update()
    {
        if (recorder && recorder.isRecording)
        {
            while (recorder.GetRecordedData(ref buffer_) > 0)
            {
                var voice = new VoiceData() { data = buffer_ };
                client.SendByChannel(VoiceMessage.ClientToHost, voice, channelId);
            }
        }
    }

    public void StartVoice()
    {
        if (hasStarted_) return;
        hasStarted_ = true;

        StartRecorder();
        StartNetwork();
    }

    public void StopVoice()
    {
        if (!hasStarted_) return;
        hasStarted_ = false;

        StopNetwork();
        StopRecorder();
    }

    void InitManager()
    {
        if (networkManager == null)
        {
            networkManager = FindObjectOfType<NetworkManager>();
        }
    }

    void InitRecorder()
    {
        if (recorder == null)
        {
            var go = new GameObject("Recorder");
            go.transform.SetParent(transform);
            recorder = go.AddComponent<VoiceRecorder>();
        }
        recorder.Initialize(micIndex);
    }

    void InitPlayer()
    {
        if (player == null)
        {
            var go = new GameObject("Player");
            go.transform.SetParent(transform);
            player = go.AddComponent<VoicePlayer>();
        }
    }

    void StartRecorder()
    {
        recorder.StartRecord();
    }

    void StopRecorder()
    {
        recorder.StopRecord();
    }

    void StartNetwork()
    {
        if (isHost)
        {
            NetworkServer.RegisterHandler(VoiceMessage.ClientToHost, OnVoiceReceivedFromClient);
        }
        client.RegisterHandler(VoiceMessage.HostToClient, OnVoiceReceivedFromHost);
    }

    void StopNetwork()
    {
        if (isHost)
        {
            NetworkServer.UnregisterHandler(VoiceMessage.ClientToHost);
        }
        client.UnregisterHandler(VoiceMessage.HostToClient);
    }

    void OnVoiceReceivedFromClient(NetworkMessage msg)
    {
        foreach (var conn in NetworkServer.connections)
        {
            bool isSelf = conn == msg.conn;
            if (!playSelfSound && isSelf) continue;

            var voice = msg.ReadMessage<VoiceData>();
            conn.SendByChannel(VoiceMessage.HostToClient, voice, channelId);
        }
    }

    void OnVoiceReceivedFromHost(NetworkMessage msg)
    {
        var conn = msg.conn;
        var voice = msg.ReadMessage<VoiceData>();
        player.Add(conn, voice);
    }
}

}