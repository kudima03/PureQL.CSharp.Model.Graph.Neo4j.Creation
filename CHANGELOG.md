# Changelog

All notable changes to `PureQL.CSharp.Model.Graph.Neo4j.Creation` are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.1.0-preview.0.2.0] - 2026-11-11

### Changed

- Depends on `PureQL.CSharp.Model.Graph` and
  `PureQL.CSharp.Model.Graph.Neo4j.Abstractions` `0.1.0-preview.1.1.0`, the
  .NET 11 releases.
- Build and publish workflows install the .NET 11 SDK by version instead of
  pinning it in `global.json`.

## [0.1.0-preview.0.1.0] - 2026-11-09

First release: creates the Neo4j schema that the operation packages of the
`PureQL.CSharp.Model.Graph` family store and look up query graphs with.

### Added

- **`SchemaCreationStatements`** — `sealed record : IEnumerable<Query>`
  yielding, in a fixed order, the Cypher statements that create the
  `pureql_query_id` uniqueness constraint on `Query(id)` and the
  `pureql_node_query_id` (`Node(queryId)`), `pureql_node_field_source`
  (`Node(field, source)`) and `pureql_node_param` (`Node(param)`) indexes.
  Every statement is `IF NOT EXISTS`, so applying the schema again changes
  nothing.
- **`CreatedSchema`** — `sealed record CreatedSchema(IAsyncQueryRunner Runner)`;
  `ApplyAsync(ct)` runs the statements in order on a session or a
  schema-only transaction, consuming each result before the next and
  checking the cancellation token between statements. It does not own the
  runner.
- **`CreatedSchemaOf`** — `sealed record CreatedSchemaOf(INeo4jStoredQueries
  Store)`; `ApplyAsync(ct)` opens a session on the store's driver against its
  database and runs each statement in its own auto-commit transaction. It
  closes the session, never the driver.
- **`SchemaNames`** — the names of the constraint and the indexes as
  constants.
- Depends on `PureQL.CSharp.Model.Graph` `0.1.0-preview.1.0.0`,
  `PureQL.CSharp.Model.Graph.Neo4j.Abstractions` `0.1.0-preview.1.0.0` and
  `Neo4j.Driver` `6.5.1`; targets .NET 11 and is AOT-compatible.
