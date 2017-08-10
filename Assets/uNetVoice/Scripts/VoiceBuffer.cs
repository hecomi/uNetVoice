using UnityEngine;
using UnityEngine.Assertions;
using System.Threading;

namespace uNetVoice
{

public class VoiceBuffer
{
    public const int DefaultBufferSize = 16384;

    float[] buf_;
    int top_ = 0;
    int bottom_ = 0;
    int mask_ = 0;
    object lockObject_ = new object();

    public VoiceBuffer()
    {
        Init(DefaultBufferSize);
    }

    public VoiceBuffer(int size)
    {
        Init(size);
    }

    void Init(int size)
    {
        Assert.IsTrue(Mathf.IsPowerOfTwo(size));
        buf_ = new float[size];
        mask_ = size - 1;
    }

    public void Add(float[] buf)
    {
        lock (lockObject_)
        {
            int n = buf.Length;
            for (int i = 0; i < n; ++i)
            {
                buf_[(top_ + i) & mask_] = buf[i];
            }
            top_ += n;
        }
    }

    public int Get(ref float[] buf, int minSize = 0)
    {
        int n = buf.Length;
        if (bottom_ + n >= top_)
        {
            n = top_ - bottom_;
        }

        if (n < minSize) return 0;
            
        lock (lockObject_)
        {
            for (int i = 0; i < n; ++i)
            {
                buf[i] = buf_[(bottom_ + i) & mask_];
            }
            bottom_ += n;

            if (top_ > buf_.Length && bottom_ > buf_.Length)
            {
                top_ = top_ & mask_;
                bottom_ = bottom_ & mask_;
            }
        }

        return n;
    }
}

}