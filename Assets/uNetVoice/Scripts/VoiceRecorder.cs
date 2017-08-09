using UnityEngine;
using UnityEngine.Events;

namespace uNetVoice
{

[RequireComponent(typeof(AudioSource))]
public class VoiceRecorder : MonoBehaviour
{
    AudioSource source_;
    string micName_ = null;

    bool initialized_ = false;
    bool recording_ = false;

    int minFreq_ = 0;
    int maxFreq_ = 0;
    float[] tmpBuffer_ = null;

    VoiceBuffer buffer_ = new VoiceBuffer();

    public bool isRecording
    {
        get { return recording_; }
    }

    void Awake()
    {
        source_ = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!source_.isPlaying && initialized_ && recording_) 
        {
            source_.clip = Microphone.Start(micName_, false, 10, maxFreq_);
            while (Microphone.GetPosition(micName_) <= 0) {}
            source_.Play();
        }
    }

    void OnApplicationPause()
    {
        source_.Stop();
        Destroy(source_.clip);
    }

    public void Initialize(int micIndex = 0)
    {
        if (Microphone.devices.Length <= 0) 
        {
            Debug.LogWarning("Microphone not found.");
            return;
        }

        int maxIndex = Microphone.devices.Length - 1;
        if (micIndex > maxIndex) 
        {
            micIndex = maxIndex;
        }

        micName_ = Microphone.devices[micIndex];
        if (micName_.Length == 0)
        {
            micName_ = "Default Mic";
        }

        Debug.Log("Use mic: " + micName_);

        Microphone.GetDeviceCaps(micName_, out minFreq_, out maxFreq_);
        if ((minFreq_ == 0 && maxFreq_ == 0) || maxFreq_ > 44100) 
        {
            maxFreq_ = 44100;
        } 

        initialized_ = true;
    }

    public void StartRecord()
    {
        if (!initialized_) 
        {
            Debug.LogError("Mic has not been initialized yet!");
            return;
        } 

        recording_ = true;
    }

    public void StopRecord()
    {
        source_.Stop();
        Destroy(source_.clip);

        recording_ = false;
    }

    public int GetRecordedData(ref float[] buf)
    {
        return buffer_.Get(ref buf, buf.Length);
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        // make voice data monoral
        int n = data.Length / channels;
        if (tmpBuffer_ == null)
        {
            tmpBuffer_ = new float[n];
        }
        for (int i = 0; i < n; ++i)
        {
            tmpBuffer_[i] = data[channels * i];
        }

        buffer_.Add(tmpBuffer_);

        // make mic sound silent
        System.Array.Clear(data, 0, data.Length);
    }
}

}