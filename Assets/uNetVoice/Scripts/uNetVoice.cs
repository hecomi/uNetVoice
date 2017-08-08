using UnityEngine;
using UnityEngine.Networking;

namespace uNetVoice
{

public class uNetVoice : MonoBehaviour
{
    GameObject recorderObject_;
    VoiceRecorder recorder_;
    GameObject playerObject_;
    VoicePlayer player_;
    bool hasStarted_ = false;

    [SerializeField]
    NetworkManager networkManager;

    [SerializeField]
    int channelId = 0;

    [SerializeField]
    bool playSelfSound = false;

    [SerializeField]
    int maxQueueNumber = 10;

    NetworkClient client
    {
        get { return networkManager.client; }
    }

    bool isHost
    {
        get { return NetworkServer.connections.Count > 0; }
    }

    public void StartVoice()
    {
        if (hasStarted_) return;
        hasStarted_ = true;

        StartRecorder();
        StartPlayer();
        StartNetwork();
    }

    public void StopVoice()
    {
        if (!hasStarted_) return;
        hasStarted_ = false;

        StopNetwork();
        StopPlayer();
        StopRecorder();
    }

    void OnDestroy()
    {
        StopVoice();
    }

    void Update()
    {
        if (player_)
        {
            player_.maxQueueNumber = maxQueueNumber;
        }
    }

    void StartRecorder()
    {
        recorderObject_ = new GameObject("Recorder");
        recorderObject_.transform.SetParent(transform);

        recorder_ = recorderObject_.AddComponent<VoiceRecorder>();
        recorder_.Initialize();
        recorder_.Record();
        recorder_.onAudioFilterRead.AddListener(SendVoice);
    }

    void StopRecorder()
    {
        recorder_.onAudioFilterRead.RemoveListener(SendVoice);
        recorder_.Stop();
        Destroy(recorderObject_);
    }

    void StartPlayer()
    {
        playerObject_ = new GameObject("Player");
        playerObject_.transform.SetParent(transform);

        player_ = playerObject_.AddComponent<VoicePlayer>();
    }

    void StopPlayer()
    {
        Destroy(playerObject_);
    }

    void StartNetwork()
    {
        if (isHost)
        {
            NetworkServer.RegisterHandler(VoiceMessage.Type, OnServerVoiceReceived);
        }
        client.RegisterHandler(VoiceMessage.Type, OnClientVoiceReceived);
    }

    void StopNetwork()
    {
        if (isHost)
        {
            NetworkServer.UnregisterHandler(VoiceMessage.Type);
        }
        client.UnregisterHandler(VoiceMessage.Type);
    }

    void SendVoice(float[] data, int channels)
    {
        if (client.isConnected)
        {
            var voice = new VoiceMessage()
            {
                data = data, 
                channels = channels 
            };
            client.SendByChannel(VoiceMessage.Type, voice, channelId);
        }
    }

    void OnServerVoiceReceived(NetworkMessage msg)
    {
        foreach (var conn in NetworkServer.connections)
        {
            if (!playSelfSound && conn == msg.conn) continue;

            var voice = msg.ReadMessage<VoiceMessage>();
            conn.SendByChannel(VoiceMessage.Type, voice, channelId);
        }
    }

    void OnClientVoiceReceived(NetworkMessage msg)
    {
        var voice = msg.ReadMessage<VoiceMessage>();
        player_.Add(msg.conn, voice);
    }
}

}