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

    void StartManager(bool isHost)
    {
        if (hasStarted_) return;

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
        uNetVoice_.StartVoice();

        isHost_ = isHost;
        hasStarted_ = true;
    }

    public void StartHost()
    {
        StartManager(true);
    }

    public void StartClient()
    {
        StartManager(false);
    }

    public void Stop()
    {
        if (!hasStarted_) return;

        uNetVoice_.StopVoice();

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
    }
}

}