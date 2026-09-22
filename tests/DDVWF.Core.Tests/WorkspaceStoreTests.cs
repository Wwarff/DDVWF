using DDVWF.Core.Workspace;

namespace DDVWF.Core.Tests;

public sealed class WorkspaceStoreTests
{
    [Fact]
    public async Task Settings_round_trip()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"ddvwf-{Guid.NewGuid():N}.json");
        try
        {
            var store = new JsonWorkspaceStore(path);
            var expected = new WorkspaceSettings { EqRoot = @"S:\EQ", LastZone = "poknowledge", Camera = new(1,2,3,4,5,20) };
            await store.SaveAsync(expected);
            var actual = await store.LoadAsync();
            Assert.Equal(expected.EqRoot, actual.EqRoot);
            Assert.Equal(expected.LastZone, actual.LastZone);
            Assert.Equal(expected.Camera, actual.Camera);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
}
