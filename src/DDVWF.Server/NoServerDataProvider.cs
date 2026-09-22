using DDVWF.Core.Zone;
namespace DDVWF.Server;
public sealed class NoServerDataProvider : IServerDataProvider
{
 public bool CanWrite=>false;
 public Task PopulateAsync(CompleteZone zone,CancellationToken cancellationToken)=>Task.CompletedTask;
}
