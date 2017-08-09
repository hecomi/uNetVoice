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
    bool recording_   = false;

    int sampleCount_ = 0;
    int channels_ = 0;
    int minFreq_ = 0;
    int maxFreq_ = 0;

    public class AudioFilterReadEvent : UnityEvent<float[], int> {}
    public AudioFilterReadEvent onVoiceRead = new AudioFilterReadEvent();

    public bool isReady 
    {
        get { return initialized_; }
    }

    public bool isRecording 
    {
        get { return recording_; }
    }

    public int sampleCount
    {
        get { return sampleCount_; }
    }

    public int channels
    {
        get { return channels_; }
    }

    public int frequency
    {
        get { return maxFreq_; }
    }

    public AudioClip clip 
    {
        get { return source_.clip; }
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
        Debug.Log("Use mic: " + micName_);

        Microphone.GetDeviceCaps(micName_, out minFreq_, out maxFreq_);
        if ((minFreq_ == 0 && maxFreq_ == 0) || maxFreq_ > 44100) 
        {
            maxFreq_ = 44100;
        } 

        initialized_ = true;
    }

    public void Record()
    {
        if (!initialized_) 
        {
            Debug.LogError("Mic has not been initialized yet!");
            return;
        } 

        recording_ = true;
    }

    public void Stop()
    {
        source_.Stop();
        Destroy(source_.clip);

        recording_ = false;
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        sampleCount_ = data.Length;
        channels_ = channels;
        onVoiceRead.Invoke(data, channels);
        System.Array.Clear(data, 0, sampleCount_);
    }
}

}