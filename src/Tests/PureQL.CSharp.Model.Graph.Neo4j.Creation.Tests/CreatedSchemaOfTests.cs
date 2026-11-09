using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;

namespace PureQL.CSharp.Model.Graph.Neo4j.Creation.Tests;

public sealed record CreatedSchemaOfTests : IClassFixture<Neo4jFixture>
{
    private readonly Neo4jFixture _fixture;

    public CreatedSchemaOfTests(Neo4jFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void StartsWithoutSchema()
    {
        Assert.DoesNotContain(
            _fixture.Before,
            item => item.Name.StartsWith("pureql_", StringComparison.Ordinal)
        );
    }

    [Fact]
    public async Task CreatesQueryIdConstraint()
    {
        await new CreatedSchemaOf(_fixture.Store).ApplyAsync();
        SchemaObject constraint = await _fixture.SchemaObjectAsync(
            "CONSTRAINTS",
            SchemaNames.QueryId
        );
        Assert.Equal(
            ("NODE", "Query", "id"),
            (constraint.EntityType, constraint.Labels, constraint.Properties)
        );
    }

    [Fact]
    public async Task CreatesUniquenessConstraint()
    {
        await new CreatedSchemaOf(_fixture.Store).ApplyAsync();
        SchemaObject constraint = await _fixture.SchemaObjectAsync(
            "CONSTRAINTS",
            SchemaNames.QueryId
        );
        Assert.EndsWith("UNIQUENESS", constraint.Type, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreatesNodeQueryIdIndex()
    {
        await new CreatedSchemaOf(_fixture.Store).ApplyAsync();
        await AssertIndexAsync(SchemaNames.NodeQueryId, "queryId");
    }

    [Fact]
    public async Task CreatesNodeFieldSourceIndex()
    {
        await new CreatedSchemaOf(_fixture.Store).ApplyAsync();
        await AssertIndexAsync(SchemaNames.NodeFieldSource, "field,source");
    }

    [Fact]
    public async Task CreatesNodeParamIndex()
    {
        await new CreatedSchemaOf(_fixture.Store).ApplyAsync();
        await AssertIndexAsync(SchemaNames.NodeParam, "param");
    }

    [Fact]
    public async Task CreatesOnlySchemaObjects()
    {
        await new CreatedSchemaOf(_fixture.Store).ApplyAsync();
        IReadOnlyList<SchemaObject> schema = await _fixture.SchemaAsync();
        Assert.Equal(
            [
                $"CONSTRAINTS {SchemaNames.QueryId}",
                $"INDEXES {SchemaNames.NodeFieldSource}",
                $"INDEXES {SchemaNames.NodeParam}",
                $"INDEXES {SchemaNames.NodeQueryId}",
                $"INDEXES {SchemaNames.QueryId}",
            ],
            schema
                .Except(_fixture.Before)
                .Select(item => $"{item.Kind} {item.Name}")
                .Order(StringComparer.Ordinal)
        );
    }

    [Fact]
    public async Task KeepsSchemaWhenAppliedTwice()
    {
        await new CreatedSchemaOf(_fixture.Store).ApplyAsync();
        IReadOnlyList<SchemaObject> once = await _fixture.SchemaAsync();
        await new CreatedSchemaOf(_fixture.Store).ApplyAsync();
        Assert.Equal(once, await _fixture.SchemaAsync());
    }

    [Fact]
    public async Task UsesStoreDatabase()
    {
        _ = await Assert.ThrowsAnyAsync<Neo4jException>(() =>
            new CreatedSchemaOf(
                new Neo4jStoredQueries(_fixture.Driver, "missing")
            ).ApplyAsync()
        );
    }

    [Fact]
    public async Task ThrowsWhenCancelled()
    {
        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new CreatedSchemaOf(_fixture.Store).ApplyAsync(new CancellationToken(true))
        );
    }

    [Fact]
    public void KeepsStore()
    {
        Assert.Same(_fixture.Store, new CreatedSchemaOf(_fixture.Store).Store);
    }

    private async Task AssertIndexAsync(string name, string properties)
    {
        SchemaObject index = await _fixture.SchemaObjectAsync("INDEXES", name);
        Assert.Equal(
            ("RANGE", "NODE", "Node", properties),
            (index.Type, index.EntityType, index.Labels, index.Properties)
        );
    }
}
