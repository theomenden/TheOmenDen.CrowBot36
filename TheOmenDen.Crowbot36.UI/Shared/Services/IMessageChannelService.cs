namespace TheOmenDen.Crowbot36.UI.Shared.Services;
public interface IMessageChannelService<T>
{
    ValueTask PublishMessageAsync(T content, CancellationToken cancellationToken = default);
    ValueTask<T> ReceiveMessageAsync(CancellationToken cancellationToken = default);
    IAsyncEnumerable<T> GetAllMessagesAsync(CancellationToken cancellationToken = default);
}
