namespace PureQL.CSharp.Model.Graph.Neo4j.Creation.Tests;

public static class Statements
{
    public const string QueryId =
        "CREATE CONSTRAINT pureql_query_id IF NOT EXISTS "
        + "FOR (q:Query) REQUIRE q.id IS UNIQUE";

    public const string NodeQueryId =
        "CREATE INDEX pureql_node_query_id IF NOT EXISTS FOR (n:Node) ON (n.queryId)";

    public const string NodeFieldSource =
        "CREATE INDEX pureql_node_field_source IF NOT EXISTS "
        + "FOR (n:Node) ON (n.field, n.source)";

    public const string NodeParam =
        "CREATE INDEX pureql_node_param IF NOT EXISTS FOR (n:Node) ON (n.param)";

    public static IReadOnlyList<string> All { get; } =
    [QueryId, NodeQueryId, NodeFieldSource, NodeParam];
}
