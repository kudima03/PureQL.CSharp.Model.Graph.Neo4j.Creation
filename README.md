# PureQL.CSharp.Model.Graph.Neo4j.Creation

Creates the Neo4j schema, constraints and indexes, for stored **PureQL** query graphs.

[![.NET build & test](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Creation/actions/workflows/build-and-test.yml/badge.svg?branch=main)](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Creation/actions/workflows/build-and-test.yml)
[![Build and Deploy](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Creation/actions/workflows/publish-nuget.yml/badge.svg?branch=main)](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Creation/actions/workflows/publish-nuget.yml)
[![NuGet](https://img.shields.io/nuget/v/PureQL.CSharp.Model.Graph.Neo4j.Creation)](https://www.nuget.org/packages/PureQL.CSharp.Model.Graph.Neo4j.Creation)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Overview

`PureQL.CSharp.Model.Graph.Neo4j.Creation` is the schema creation package of the
`PureQL.CSharp.Model.Graph` family: it creates, idempotently, the uniqueness constraint
and the indexes that the other Neo4j packages rely on to store and look up PureQL query
graphs.

## Schema

| Name (`SchemaNames`) | Kind | Cypher |
|------|------|--------|
| `pureql_query_id` (`QueryId`) | Uniqueness constraint | `CREATE CONSTRAINT pureql_query_id IF NOT EXISTS FOR (q:Query) REQUIRE q.id IS UNIQUE` |
| `pureql_node_query_id` (`NodeQueryId`) | Range index | `CREATE INDEX pureql_node_query_id IF NOT EXISTS FOR (n:Node) ON (n.queryId)` |
| `pureql_node_field_source` (`NodeFieldSource`) | Composite range index | `CREATE INDEX pureql_node_field_source IF NOT EXISTS FOR (n:Node) ON (n.field, n.source)` |
| `pureql_node_param` (`NodeParam`) | Range index | `CREATE INDEX pureql_node_param IF NOT EXISTS FOR (n:Node) ON (n.param)` |

The statements run in this order. The constraint keeps query ids unique and backs the
lookup of a query by id; `queryId` finds every node of a query; `field, source` and
`param` serve the structural lookups (queries reading a field of a source, queries using
a parameter). Every statement is `IF NOT EXISTS`, so applying the schema again changes
nothing. The `Existence` and `Destruction` packages repeat these names without depending
on this package: they must stay identical.

## Public API

Namespace `PureQL.CSharp.Model.Graph.Neo4j.Creation`:

| Name | Description |
|------|-------------|
| `SchemaCreationStatements` | `sealed record : IEnumerable<Neo4j.Driver.Query>`: the four statements above, in order, without parameters |
| `CreatedSchema` | `sealed record CreatedSchema(IAsyncQueryRunner Runner)`; `ApplyAsync(CancellationToken ct = default)` runs the statements in order on the session or transaction |
| `CreatedSchemaOf` | `sealed record CreatedSchemaOf(INeo4jStoredQueries Store)`; `ApplyAsync(CancellationToken ct = default)` opens a session on `Store.Driver` against `Store.Database` and runs each statement in its own auto-commit transaction |
| `SchemaNames` | The names of the constraint and the indexes, as constants |

The cancellation token is checked before each statement; the driver does not cancel a
statement that is running. Neo4j does not mix schema statements and data writes in one
transaction, so a transaction passed to `CreatedSchema` must hold schema statements only.

## Usage

```csharp
using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;
using PureQL.CSharp.Model.Graph.Neo4j.Creation;

await using IDriver driver = GraphDatabase.Driver(
    "neo4j://localhost:7687",
    AuthTokens.Basic("neo4j", "password")
);
INeo4jStoredQueries store = new Neo4jStoredQueries(driver);

// Once at startup, before storing queries; safe to repeat.
await new CreatedSchemaOf(store).ApplyAsync();

// Or on a session (or a schema-only transaction) you manage:
await using IAsyncSession session = driver.AsyncSession(
    builder => builder.WithDatabase(store.Database)
);
await new CreatedSchema(session).ApplyAsync();
```

## Design Principles

- **Idempotent** — every statement is `IF NOT EXISTS`
- **Composable** — `CreatedSchema` runs on any `IAsyncQueryRunner`; the statements are plain `Query` values for callers that run them themselves
- **AOT-compatible** — `IsAotCompatible=true`, no reflection

## Dependencies

- [`PureQL.CSharp.Model.Graph`](https://github.com/kudima03/PureQL.CSharp.Model.Graph) `0.1.0-preview.1.0.0` — the labels and properties of a query graph
- [`PureQL.CSharp.Model.Graph.Neo4j.Abstractions`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Abstractions) `0.1.0-preview.1.0.0` — `INeo4jStoredQueries`
- [`Neo4j.Driver`](https://www.nuget.org/packages/Neo4j.Driver) `6.5.1`

## Target Frameworks

- .NET 11

## Installation

```
dotnet add package PureQL.CSharp.Model.Graph.Neo4j.Creation
```

## Related packages

- [`PureQL.CSharp.Model.Graph`](https://github.com/kudima03/PureQL.CSharp.Model.Graph) — maps a `PureQLQuery` to a property graph and back, without a driver
- [`PureQL.CSharp.Model.Graph.Neo4j.Abstractions`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Abstractions) — the stored-queries contract the Neo4j packages share
- [`PureQL.CSharp.Model.Graph.Neo4j.Insertion`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Insertion) — writes a query
- [`PureQL.CSharp.Model.Graph.Neo4j.Querying`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Querying) — reads queries back
- [`PureQL.CSharp.Model.Graph.Neo4j.Deletion`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Deletion) — deletes queries
- [`PureQL.CSharp.Model.Graph.Neo4j.Updating`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Updating) — replaces a query in one transaction
- [`PureQL.CSharp.Model.Graph.Neo4j.Existence`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Existence) — checks the schema and queries exist
- [`PureQL.CSharp.Model.Graph.Neo4j.Destruction`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Destruction) — drops the schema and every stored query
- [`PureQL.CSharp.Model.Graph.Neo4j.Migration`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Migration) — migrates stored graphs between model versions
