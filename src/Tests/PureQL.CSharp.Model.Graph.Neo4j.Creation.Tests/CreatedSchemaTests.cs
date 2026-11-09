using Neo4j.Driver;

namespace PureQL.CSharp.Model.Graph.Neo4j.Creation.Tests;

public sealed record CreatedSchemaTests : IClassFixture<Neo4jFixture>
{
    private readonly Neo4jFixture _fixture;

    public CreatedSchemaTests(Neo4jFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task RunsStatementsInOrder()
    {
        List<string> log = [];
        await new CreatedSchema(new RecordingQueryRunner(log)).ApplyAsync();
        Assert.Equal(
            Statements.All,
            log.Where(entry => entry.StartsWith("run ", StringComparison.Ordinal))
                .Select(entry => entry["run ".Length..])
        );
    }

    [Fact]
    public async Task ConsumesEachResultBeforeNextStatement()
    {
        List<string> log = [];
        await new CreatedSchema(new RecordingQueryRunner(log)).ApplyAsync();
        Assert.Equal(
            Statements.All.SelectMany(statement =>
                new[] { $"run {statement}", $"consume {statement}" }
            ),
            log
        );
    }

    [Fact]
    public async Task RunsNothingWhenCancelledBefore()
    {
        List<string> log = [];
        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new CreatedSchema(new RecordingQueryRunner(log)).ApplyAsync(
                new CancellationToken(true)
            )
        );
        Assert.Empty(log);
    }

    [Fact]
    public async Task StopsWhenCancelledBetweenStatements()
    {
        List<string> log = [];
        using CancellationTokenSource cancellation = new CancellationTokenSource();
        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new CreatedSchema(
                new RecordingQueryRunner(log, cancellation.Cancel)
            ).ApplyAsync(cancellation.Token)
        );
        Assert.Equal([$"run {Statements.QueryId}", $"consume {Statements.QueryId}"], log);
    }

    [Fact]
    public void KeepsRunner()
    {
        IAsyncQueryRunner runner = new RecordingQueryRunner([]);
        Assert.Same(runner, new CreatedSchema(runner).Runner);
    }

    [Fact]
    public async Task CreatesSchemaInSession()
    {
        await DropSchemaAsync();
        await using (IAsyncSession session = _fixture.Session())
        {
            await new CreatedSchema(session).ApplyAsync();
        }
        Assert.Equal(ExpectedNames(), await NamesAsync());
    }

    [Fact]
    public async Task CreatesSchemaInTransaction()
    {
        await DropSchemaAsync();
        await using (IAsyncSession session = _fixture.Session())
        {
            await session.ExecuteWriteAsync(transaction =>
                new CreatedSchema(transaction).ApplyAsync()
            );
        }
        Assert.Equal(ExpectedNames(), await NamesAsync());
    }

    [Fact]
    public async Task KeepsSchemaWhenAppliedTwiceInSession()
    {
        await using IAsyncSession session = _fixture.Session();
        await new CreatedSchema(session).ApplyAsync();
        IReadOnlyList<SchemaObject> once = await _fixture.SchemaAsync();
        await new CreatedSchema(session).ApplyAsync();
        Assert.Equal(once, await _fixture.SchemaAsync());
    }

    private static IReadOnlyList<string> ExpectedNames()
    {
        return
        [
            $"CONSTRAINTS {SchemaNames.QueryId}",
            $"INDEXES {SchemaNames.NodeFieldSource}",
            $"INDEXES {SchemaNames.NodeParam}",
            $"INDEXES {SchemaNames.NodeQueryId}",
            $"INDEXES {SchemaNames.QueryId}",
        ];
    }

    private async Task<IReadOnlyList<string>> NamesAsync()
    {
        IReadOnlyList<SchemaObject> schema = await _fixture.SchemaAsync();
        return
        [
            .. schema
                .Where(item => item.Name.StartsWith("pureql_", StringComparison.Ordinal))
                .Select(item => $"{item.Kind} {item.Name}")
                .Order(StringComparer.Ordinal),
        ];
    }

    private async Task DropSchemaAsync()
    {
        await _fixture.RunAsync($"DROP CONSTRAINT {SchemaNames.QueryId} IF EXISTS");
        await _fixture.RunAsync($"DROP INDEX {SchemaNames.NodeQueryId} IF EXISTS");
        await _fixture.RunAsync($"DROP INDEX {SchemaNames.NodeFieldSource} IF EXISTS");
        await _fixture.RunAsync($"DROP INDEX {SchemaNames.NodeParam} IF EXISTS");
        Assert.Empty(await NamesAsync());
    }
}
