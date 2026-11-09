using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;
using Testcontainers.Neo4j;

namespace PureQL.CSharp.Model.Graph.Neo4j.Creation.Tests;

public sealed record Neo4jFixture : IDisposable
{
    private const string Columns =
        "id, name, type, entityType, labelsOrTypes, properties";

    private readonly Neo4jContainer _container;

    public IDriver Driver { get; }

    public INeo4jStoredQueries Store { get; }

    public IReadOnlyList<SchemaObject> Before { get; }

    public Neo4jFixture()
    {
        _container = new Neo4jBuilder("neo4j:5-community").Build();
        _container.StartAsync().GetAwaiter().GetResult();
        Driver = GraphDatabase.Driver(_container.GetConnectionString(), AuthTokens.None);
        Store = new Neo4jStoredQueries(Driver);
        Before = SchemaAsync().GetAwaiter().GetResult();
    }

    public async Task<IReadOnlyList<SchemaObject>> SchemaAsync()
    {
        List<SchemaObject> schema = [];
        schema.AddRange(await ShowAsync("CONSTRAINTS"));
        schema.AddRange(await ShowAsync("INDEXES"));
        return schema;
    }

    public async Task<SchemaObject> SchemaObjectAsync(string kind, string name)
    {
        IReadOnlyList<SchemaObject> schema = await SchemaAsync();
        return schema.Single(item => (item.Kind == kind) && (item.Name == name));
    }

    public async Task RunAsync(string cypher)
    {
        await using IAsyncSession session = Session();
        IResultCursor cursor = await session.RunAsync(cypher);
        _ = await cursor.ConsumeAsync();
    }

    public IAsyncSession Session()
    {
        return Driver.AsyncSession(builder => builder.WithDatabase(Store.Database));
    }

    public void Dispose()
    {
        Driver.Dispose();
        _container.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    private async Task<IReadOnlyList<SchemaObject>> ShowAsync(string kind)
    {
        await using IAsyncSession session = Session();
        IResultCursor cursor = await session.RunAsync(
            $"SHOW {kind} YIELD {Columns} RETURN {Columns} ORDER BY name"
        );
        List<IRecord> records = await cursor.ToListAsync();
        return
        [
            .. records.Select(record => new SchemaObject(
                kind,
                record["id"].As<long>(),
                record["name"].As<string>(),
                record["type"].As<string>(),
                record["entityType"].As<string>(),
                Joined(record["labelsOrTypes"]),
                Joined(record["properties"])
            )),
        ];
    }

    private static string Joined(object? values)
    {
        return values is IEnumerable<object> list ? string.Join(",", list) : "";
    }
}
