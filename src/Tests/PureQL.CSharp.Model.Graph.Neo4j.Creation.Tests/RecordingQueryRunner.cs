using Neo4j.Driver;

namespace PureQL.CSharp.Model.Graph.Neo4j.Creation.Tests;

public sealed record RecordingQueryRunner(List<string> Log, Action AfterRun)
    : IAsyncQueryRunner
{
    public RecordingQueryRunner(List<string> log)
        : this(log, () => { }) { }

    public Task<IResultCursor> RunAsync(global::Neo4j.Driver.Query query)
    {
        Log.Add($"run {query.Text}");
        AfterRun();
        return Task.FromResult<IResultCursor>(new RecordingResultCursor(Log, query.Text));
    }

    public Task<IResultCursor> RunAsync(string query)
    {
        throw new NotSupportedException();
    }

    public Task<IResultCursor> RunAsync(string query, object parameters)
    {
        throw new NotSupportedException();
    }

    public Task<IResultCursor> RunAsync(
        string query,
        IDictionary<string, object> parameters
    )
    {
        throw new NotSupportedException();
    }

    public void Dispose() { }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
