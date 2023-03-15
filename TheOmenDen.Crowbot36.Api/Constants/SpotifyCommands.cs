namespace TheOmenDen.CrowBot36.Constants;
internal sealed record SpotifyCommands: EnumerationBase<SpotifyCommands>
{
    private SpotifyCommands(string name, int id) : base(name, id) { }

    public static readonly SpotifyCommands Play = new(nameof(Play), 1);
    public static readonly SpotifyCommands Pause = new(nameof(Pause), 2);
    public static readonly SpotifyCommands Stop = new(nameof(Stop), 3);
    public static readonly SpotifyCommands Add = new(nameof(Add), 4);
    public static readonly SpotifyCommands Remove = new(nameof(Remove), 5);
}
