namespace PureQL.CSharp.Model.Graph.Neo4j.Creation.Tests;

public sealed record SchemaObject(
    string Kind,
    long Id,
    string Name,
    string Type,
    string EntityType,
    string Labels,
    string Properties
);
