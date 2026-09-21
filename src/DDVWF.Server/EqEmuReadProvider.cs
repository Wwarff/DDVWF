using DDVWF.Core.Zone;

namespace DDVWF.Server;

public sealed record EqEmuConnectionOptions(string Host, int Port, string Database, string User);

public sealed class EqEmuReadProvider : IServerDataProvider
{
    public bool CanWrite => false;

    public Task PopulateAsync(CompleteZone zone, CancellationToken cancellationToken)
    {
        // The provider is deliberately read-only until exact repository/API joins are implemented.
        // No fabricated rows are emitted into Complete Zone IR.
        return Task.CompletedTask;
    }
}
