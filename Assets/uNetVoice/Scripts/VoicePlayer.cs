using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace uNetVoice
{

[RequireComponent(typeof(AudioSource))]
public class VoicePlayer : MonoBehaviour
{
    Dictionary<NetworkConnection, VoiceBuffer> buffers_ 
        = new Dictionary<NetworkConnection, VoiceBuffer>();

    float[] tmpBuffer_ = null;

    void Awake()
    {
        var source = GetComponent<AudioSource>();
        source.clip = new AudioClip();
        source.loop = true;
        source.Play();
    }

    public void Add(NetworkConnection conn, VoiceData voice)
    {
        VoiceBuffer buffer = null;
        buffers_.TryGetValue(conn, out buffer);

        if (buffer == null)
        {
            buffer = new VoiceBuffer();
            buffers_.Add(conn, buffer);
        }

        buffer.Add(voice.data);
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        foreach (var kv in buffers_)
        {
            var conn = kv.Key;
            var buffer = kv.Value;

            if (tmpBuffer_ == null)
            {
                tmpBuffer_ = new float[data.Length / channels];
            }

            if (buffer.Get(ref tmpBuffer_, tmpBuffer_.Length) == 0) continue;

            var n = tmpBuffer_.Length;
            for (int i = 0; i < n; ++i)
            {
                for (int c = 0; c < channels; ++c)
                {
                    var j = channels * i + c;
                    data[j] += tmpBuffer_[i];
                }
            }
        }
    }
}

}