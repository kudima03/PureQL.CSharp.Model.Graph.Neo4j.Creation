using Neo4j.Driver;

namespace PureQL.CSharp.Model.Graph.Neo4j.Creation.Tests;

public sealed record RecordingResultCursor(List<string> Log, string Text) : IResultCursor
{
    public IRecord Current => throw new NotSupportedException();

    public bool IsOpen => true;

    public Task<IResultSummary> ConsumeAsync()
    {
        Log.Add($"consume {Text}");
        return Task.FromResult<IResultSummary>(null!);
    }

    public Task<string[]> KeysAsync()
    {
        throw new NotSupportedException();
    }

    public Task<IRecord> PeekAsync()
    {
        throw new NotSupportedException();
    }

    public Task<bool> FetchAsync()
    {
        throw new NotSupportedException();
    }

    public IAsyncEnumerator<IRecord> GetAsyncEnumerator(
        CancellationToken cancellationToken = default
    )
    {
        throw new NotSupportedException();
    }
}
