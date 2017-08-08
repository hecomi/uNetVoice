using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace uNetVoice
{

[RequireComponent(typeof(NetworkManager))]
public class TestNetworkController : MonoBehaviour
{
    [SerializeField]
    uNetVoice uNetVoice;

    [SerializeField]
    InputField address;

    [SerializeField]
    InputField port;

    [SerializeField]
    int maxConnections = 10;

    bool isHost_ = false;
    bool hasStarted_ = false;

    void StartManager(bool isHost)
    {
        if (hasStarted_) return;

        var manager = GetComponent<NetworkManager>();
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

        uNetVoice.StartVoice();

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

        uNetVoice.StopVoice();

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