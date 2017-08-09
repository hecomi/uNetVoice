using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace uNetVoice
{

[RequireComponent(typeof(AudioSource))]
public class VoicePlayer : MonoBehaviour
{
    Dictionary<NetworkConnection, Queue<VoiceData>> voiceQueues_ 
        = new Dictionary<NetworkConnection, Queue<VoiceData>>();

    public int maxQueueNumber = 10;

    void Awake()
    {
        var source = GetComponent<AudioSource>();
        source.clip = new AudioClip();
        source.loop = true;
        source.Play();
    }

    public void Add(NetworkConnection conn, VoiceData voice)
    {
        Queue<VoiceData> queue = null;
        voiceQueues_.TryGetValue(conn, out queue);

        if (queue == null)
        {
            queue = new Queue<VoiceData>();
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
                Debug.LogWarningFormat(
                    "the queued voice data ({0}:{1}-ch) format is incompatible with audio player ({2}:{3}-ch).",
                    voice.data.Length, voice.channels, data.Length, channels);
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