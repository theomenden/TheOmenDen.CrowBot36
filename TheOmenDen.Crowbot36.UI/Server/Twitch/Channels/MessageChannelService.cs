using System.Threading.Channels;
using TheOmenDen.Crowbot36.UI.Shared.Services;

namespace TheOmenDen.Crowbot36.UI.Server.Twitch.Channels;

internal sealed class MessageChannelService<T>: IMessageChannelService<T>
{
    private readonly Channel<T> _channel;

	public MessageChannelService()
	{
		_channel = Channel.CreateUnbounded<T>();
	}

	public ValueTask PublishMessageAsync(T content, CancellationToken cancellationToken = default)
	 => _channel.Writer.WriteAsync(content, cancellationToken);

	public ValueTask<T> ReceiveMessageAsync(CancellationToken cancellationToken = default)
		=> _channel.Reader.ReadAsync(cancellationToken);

	public IAsyncEnumerable<T> GetAllMessagesAsync(CancellationToken cancellationToken = default)
	=> _channel.Reader.ReadAllAsync(cancellationToken);
}
