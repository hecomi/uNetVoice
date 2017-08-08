using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace uNetVoice
{

[RequireComponent(typeof(AudioSource))]
public class VoicePlayer : MonoBehaviour
{
    Dictionary<NetworkConnection, Queue<VoiceMessage>> voiceQueues_ 
        = new Dictionary<NetworkConnection, Queue<VoiceMessage>>();

    public int maxQueueNumber = 10;

    void Awake()
    {
        var source = GetComponent<AudioSource>();
        source.clip = new AudioClip();
        source.loop = true;
        source.Play();
    }

    public void Add(NetworkConnection conn, VoiceMessage voice)
    {
        Queue<VoiceMessage> queue = null;
        voiceQueues_.TryGetValue(conn, out queue);

        if (queue == null)
        {
            queue = new Queue<VoiceMessage>();
            voiceQueues_.Add(conn, queue);
        }

        queue.Enqueue(voice);

        while (queue.Count > maxQueueNumber)
        {
            queue.Dequeue();
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        foreach (var kv in voiceQueues_)
        {
            var conn = kv.Key;
            var queue = kv.Value;

            if (queue.Count == 0) continue;

            var voice = queue.Dequeue();
            if (voice.data.Length != data.Length || voice.channels != channels)
            {
                Debug.LogWarningFormat("voice data from {0} does not match to the local audio player.", conn.address);
                continue;
            }

            for (int i = 0; i < data.Length; ++i)
            {
                data[i] += voice.data[i];
            }
        }
    }
}

}