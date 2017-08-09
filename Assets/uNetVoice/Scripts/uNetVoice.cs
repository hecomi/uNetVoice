using UnityEngine;
using UnityEngine.Networking;

namespace uNetVoice
{

public class uNetVoice : MonoBehaviour
{
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

    [SerializeField]
    int maxQueueNumber = 10;

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
        if (player)
        {
            player.maxQueueNumber = maxQueueNumber;
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
        recorder.onVoiceRead.AddListener(SendVoice);
        recorder.Record();
    }

    void StopRecorder()
    {
        recorder.onVoiceRead.RemoveListener(SendVoice);
        recorder.Stop();
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

    void SendVoice(float[] data, int channels)
    {
        if (!client.isConnected) return;

        var voice = new VoiceData()
        {
            data = data, 
            channels = channels 
        };
        client.SendByChannel(VoiceMessage.ClientToHost, voice, channelId);
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
        var voice = msg.ReadMessage<VoiceData>();
        player.Add(msg.conn, voice);
    }
}

}