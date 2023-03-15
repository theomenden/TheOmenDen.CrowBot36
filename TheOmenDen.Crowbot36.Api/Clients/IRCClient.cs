using Microsoft.Extensions.Options;
using TwitchLib.Communication.Clients;

namespace TheOmenDen.CrowBot36.Clients;
internal sealed class IrcClient: IDisposable, IAsyncDisposable
{
    private readonly string _username;
    private readonly string _password;
    private readonly TcpClient _tcpClient;
    private bool disposedValue;
    
    public IrcClient(String username, String password, TcpClient tcpClient)
    {
        _username = username;
        _password = password;

        _tcpClient ??= new TcpClient();

        InitializeClient();
    }

    private void InitializeClient()
    {
        if (_tcpClient.Open())
        {
            _tcpClient.Send("Crowbot connected");
        }
    }


    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _tcpClient.Close();
                _tcpClient.Dispose();
            }

            disposedValue = true;
        }
    }


    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public ValueTask DisposeAsync()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
