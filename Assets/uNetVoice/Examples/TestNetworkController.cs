using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace uNetVoice
{

[RequireComponent(typeof(NetworkManager))]
public class TestNetworkController : MonoBehaviour
{
    [SerializeField]
    InputField address;

    [SerializeField]
    InputField port;

    [SerializeField]
    int maxConnections = 10;

    uNetVoice uNetVoice_;
    bool isHost_ = false;
    bool hasStarted_ = false;

    void OnDestroy()
    {
        Stop();
    }

    bool StartManager(bool isHost)
    {
        if (hasStarted_) return false;

        var manager = FindObjectOfType<NetworkManager>();
        manager.networkAddress = address.text;
        manager.networkPort = int.Parse(port.text);
        manager.maxConnections = maxConnections;

        if (isHost)
        {
            manager.StartHost();
        }
        else
        {
            manager.StartClient();
        }

        uNetVoice_ = FindObjectOfType<uNetVoice>();
        uNetVoice_.StartVoiceChat();

        isHost_ = isHost;
        hasStarted_ = true;

        return true;
    }

    public void StartHost()
    {
        if (StartManager(true))
        {
            Debug.Log("Start voice chat as host");
        }
    }

    public void StartClient()
    {
        if (StartManager(false))
        {
            Debug.Log("Start voice chat as client");
        }
    }

    public void Stop()
    {
        if (!hasStarted_) return;

        uNetVoice_.StopVoiceChat();

        var manager = GetComponent<NetworkManager>();
        if (isHost_)
        {
            manager.StopHost();
        }
        else
        {
            manager.StopClient();
        }

        hasStarted_ = false;

        Debug.Log("Stop");
    }
}

}