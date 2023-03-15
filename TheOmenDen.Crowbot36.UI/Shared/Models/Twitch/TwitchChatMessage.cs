using TheOmenDen.Crowbot36.UI.Shared.Constants;
using TheOmenDen.Crowbot36.UI.Shared.Models.Bot;

namespace TheOmenDen.Crowbot36.UI.Shared.Models.Twitch;
public sealed class TwitchChatMessage
{
    public MessageState State { get; set; }
    internal int Nonce { get; set; }
    public string Channel { get; private set; }
    public string User { get; private set; }
    public string Target { get; private set; }
    public ChatMessage ChatMessage { get; private set; }
    public string TwitchMessage { get; private set; }

    public TwitchChatMessage(string message, string user = "mg36crow", string channel = "mg36crow", ChatMessage chatmessage = null, string target = null)
    {
        TwitchMessage = message;
        User = user;
        Channel = channel;
        ChatMessage = chatmessage;
        Target = target;
    }
}
