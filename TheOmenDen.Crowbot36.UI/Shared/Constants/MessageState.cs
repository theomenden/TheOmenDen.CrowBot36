using TheOmenDen.Shared.Enumerations;

namespace TheOmenDen.Crowbot36.UI.Shared.Constants;

public sealed record MessageState: EnumerationBaseFlag<MessageState, byte>
{
    private MessageState(string name, byte id): base(name, id) { }

    public static readonly MessageState Normal = new(nameof(Normal),0);
    public static readonly MessageState Queued = new(nameof(Queued),1);
    public static readonly MessageState Failed = new(nameof(Failed),2);
}
