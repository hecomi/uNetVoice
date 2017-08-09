using UnityEngine;

namespace uNetVoice
{

public class VoiceBufferTest : MonoBehaviour
{
    VoiceBuffer buffer_ = new VoiceBuffer(1024);

    void Start()
    {
        var buf = new float[128];
        for (int i = 0; i < 10; ++i)
        {
            for (int j = 0; j < buf.Length; ++j)
            {
                buf[j] = i * buf.Length + j;
            }
            buffer_.Add(buf);
        }

        int n = 0;
        while (buffer_.Get(ref buf, buf.Length) > 0)
        {
            Debug.Log(buf[10 * n++]);
        }
    }
}

}