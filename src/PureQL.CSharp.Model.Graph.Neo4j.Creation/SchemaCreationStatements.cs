using System.Collections;

namespace PureQL.CSharp.Model.Graph.Neo4j.Creation;

/// <summary>
/// The Cypher statements that create the schema of stored PureQL query graphs: the
/// uniqueness constraint on the query id and the indexes on the nodes of every query.
/// </summary>
/// <remarks>
/// <para>The statements are, in order:</para>
/// <list type="number">
/// <item><description>
/// <c>CREATE CONSTRAINT pureql_query_id IF NOT EXISTS FOR (q:Query) REQUIRE q.id IS UNIQUE</c>
/// </description></item>
/// <item><description>
/// <c>CREATE INDEX pureql_node_query_id IF NOT EXISTS FOR (n:Node) ON (n.queryId)</c>
/// </description></item>
/// <item><description>
/// <c>CREATE INDEX pureql_node_field_source IF NOT EXISTS FOR (n:Node) ON (n.field, n.source)</c>
/// </description></item>
/// <item><description>
/// <c>CREATE INDEX pureql_node_param IF NOT EXISTS FOR (n:Node) ON (n.param)</c>
/// </description></item>
/// </list>
/// <para>
/// Every statement is idempotent (<c>IF NOT EXISTS</c>) and has no parameters. The names
/// are those of <see cref="SchemaNames"/>. Neo4j does not run schema statements in the
/// same transaction as data writes.
/// </para>
/// </remarks>
public sealed record SchemaCreationStatements : IEnumerable<global::Neo4j.Driver.Query>
{
    private const string Field = "field";

    private const string Source = "source";

    private const string Param = "param";

    /// <summary>Returns the statements in the order they are to run.</summary>
    /// <returns>An enumerator over the four statements.</returns>
    public IEnumerator<global::Neo4j.Driver.Query> GetEnumerator()
    {
        yield return new global::Neo4j.Driver.Query(
            $"CREATE CONSTRAINT {SchemaNames.QueryId} IF NOT EXISTS "
                + $"FOR (q:{GraphLabels.Query}) REQUIRE q.{GraphProperties.Id} IS UNIQUE"
        );
        yield return new global::Neo4j.Driver.Query(
            $"CREATE INDEX {SchemaNames.NodeQueryId} IF NOT EXISTS "
                + $"FOR (n:{GraphLabels.Node}) ON (n.{GraphProperties.QueryId})"
        );
        yield return new global::Neo4j.Driver.Query(
            $"CREATE INDEX {SchemaNames.NodeFieldSource} IF NOT EXISTS "
                + $"FOR (n:{GraphLabels.Node}) ON (n.{Field}, n.{Source})"
        );
        yield return new global::Neo4j.Driver.Query(
            $"CREATE INDEX {SchemaNames.NodeParam} IF NOT EXISTS "
                + $"FOR (n:{GraphLabels.Node}) ON (n.{Param})"
        );
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
