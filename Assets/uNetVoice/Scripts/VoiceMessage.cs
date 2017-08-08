using UnityEngine;
using UnityEngine.Networking;

namespace uNetVoice
{

public class VoiceMessage : MessageBase
{
    public static readonly short Type = MsgType.Highest + 1;

    public float[] data;
    public int channels;
}

}
