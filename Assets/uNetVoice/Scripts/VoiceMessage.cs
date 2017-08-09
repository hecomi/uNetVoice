using UnityEngine;
using UnityEngine.Networking;

namespace uNetVoice
{

public static class VoiceMessage
{
    public const short ClientToHost = MsgType.Highest + 1;
    public const short HostToClient = MsgType.Highest + 2;
}

}
