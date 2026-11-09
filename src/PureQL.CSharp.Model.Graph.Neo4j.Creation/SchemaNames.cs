namespace PureQL.CSharp.Model.Graph.Neo4j.Creation;

/// <summary>
/// The names of the constraint and the indexes that the schema of stored PureQL query
/// graphs consists of.
/// </summary>
/// <remarks>
/// The packages <c>PureQL.CSharp.Model.Graph.Neo4j.Existence</c> and
/// <c>PureQL.CSharp.Model.Graph.Neo4j.Destruction</c> do not depend on this package and
/// declare the same names themselves: the values here must match theirs, so renaming one
/// is a breaking change across the three packages.
/// </remarks>
public static class SchemaNames
{
    /// <summary>
    /// The uniqueness constraint on the <c>id</c> of every <c>Query</c> node:
    /// <c>pureql_query_id</c>.
    /// </summary>
    public const string QueryId = "pureql_query_id";

    /// <summary>
    /// The range index on the <c>queryId</c> of every <c>Node</c>:
    /// <c>pureql_node_query_id</c>.
    /// </summary>
    public const string NodeQueryId = "pureql_node_query_id";

    /// <summary>
    /// The composite range index on the <c>field</c> and <c>source</c> of every
    /// <c>Node</c>: <c>pureql_node_field_source</c>.
    /// </summary>
    public const string NodeFieldSource = "pureql_node_field_source";

    /// <summary>
    /// The range index on the <c>param</c> of every <c>Node</c>:
    /// <c>pureql_node_param</c>.
    /// </summary>
    public const string NodeParam = "pureql_node_param";
}
