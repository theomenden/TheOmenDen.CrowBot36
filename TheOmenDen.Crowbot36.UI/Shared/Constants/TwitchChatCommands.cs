namespace TheOmenDen.CrowBot36.Constants;
internal sealed record TwitchChatCommands: EnumerationBase<TwitchChatCommands>
{
    private TwitchChatCommands(string name, int id) : base(name, id) { }

    public static readonly TwitchChatCommands Announcement = new(nameof(Announcement), 1);
    public static readonly TwitchChatCommands Shoutout = new(nameof(Shoutout), 2);
    public static readonly TwitchChatCommands Timeout = new(nameof(Timeout), 3);
    public static readonly TwitchChatCommands Ban = new(nameof(Ban), 4);
    public static readonly TwitchChatCommands Commercial = new(nameof(Commercial), 5);
}
