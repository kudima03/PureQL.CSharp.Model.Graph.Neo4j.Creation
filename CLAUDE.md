# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

All `dotnet` commands must be run from the `./src` directory. The projects need the .NET 11 SDK (for the model's C# 15 unions); where the system SDK is older, use `~/.dotnet/dotnet` or put it first on `PATH` with `DOTNET_ROOT=$HOME/.dotnet`.

```bash
dotnet restore
dotnet build --no-restore -warnaserror
dotnet format --verify-no-changes          # check code style (CI enforces this)
dotnet format                              # auto-fix code style
dotnet tool restore && dotnet csharpier check .   # check formatting (CI enforces this)
dotnet csharpier format .                  # auto-fix csharpier formatting
dotnet test --no-build --verbosity normal  # run tests
dotnet pack --configuration Release -p:Version=<version> --output .
```

CI additionally passes `-p:AssemblyVersion` (pinned to the major) and `-p:FileVersion`; see `.github/workflows/publish-nuget.yml`.

## Architecture

This is the **schema creation package** of the Neo4j storage of the `PureQL.CSharp.Model.Graph` family. It depends on `PureQL.CSharp.Model.Graph` `0.1.0-preview.1.0.0`, `PureQL.CSharp.Model.Graph.Neo4j.Abstractions` `0.1.0-preview.1.0.0` and `Neo4j.Driver` `6.5.1`, and on no other operation package. Namespace `PureQL.CSharp.Model.Graph.Neo4j.Creation`, one public type per file:

- `SchemaCreationStatements` — `sealed record : IEnumerable<Query>` yielding the four `CREATE CONSTRAINT … IF NOT EXISTS` / `CREATE INDEX … IF NOT EXISTS` statements in a fixed order, built from `GraphLabels` / `GraphProperties` of the Graph package and `SchemaNames`. `Query` is written `global::Neo4j.Driver.Query`, because inside the namespace `PureQL.CSharp.Model.Graph.Neo4j.Creation` both `Query` (the model's `PureQL.CSharp.Model.Query`) and `Neo4j` (the namespace `PureQL.CSharp.Model.Graph.Neo4j`) resolve to the enclosing namespaces first.
- `CreatedSchema` — `sealed record (IAsyncQueryRunner Runner)`; `ApplyAsync(ct)` runs the statements in order, consuming each result before the next and checking `ct` before each (the driver's `RunAsync` takes no token). It does not own the runner. A transaction runner works as long as it holds schema statements only (Neo4j rejects schema and data writes in one transaction).
- `CreatedSchemaOf` — `sealed record (INeo4jStoredQueries Store)`; `ApplyAsync(ct)` opens a session on `Store.Driver` with `WithDatabase(Store.Database)` and applies `CreatedSchema` on it, so each statement runs in its own auto-commit transaction. It closes the session, never the driver.
- `SchemaNames` — the names of the constraint and the indexes as constants. The Existence and Destruction packages duplicate them rather than depend on this package: renaming one is a breaking change across the three packages.

Every public member has XML documentation (`GenerateDocumentationFile`; a missing comment fails the build under `-warnaserror`).

**Test project:** `PureQL.CSharp.Model.Graph.Neo4j.Creation.Tests` targets `net11.0` and uses xunit with coverlet. `SchemaCreationStatementsTests` and `SchemaNamesTests` check the exact Cypher text, order and names; `CreatedSchemaTests` checks the order, consumption and cancellation over `RecordingQueryRunner` / `RecordingResultCursor` (fakes logging what runs) and creates the schema in a session and in an explicit transaction; `CreatedSchemaOfTests` applies it to a fresh database and reads `SHOW CONSTRAINTS` / `SHOW INDEXES` back (labels, properties, types, no other object added, unchanged ids when applied twice). The integration tests start one `neo4j:5-community` container per test class through `Testcontainers.Neo4j` (`Neo4jFixture`, no fixed host ports), so they need Docker.

**Target framework:** net11.0 only, as the model. `IsAotCompatible` is `true`.

**Package validation:** `EnablePackageValidation` is `true` against `PackageValidationBaselineVersion` `0.1.0-preview.0.1.0`, the first release; the `nuget-publish` skill moves it after each release, and intentional breaks go to `CompatibilitySuppressions.xml` next to the csproj.

**Mutation testing:** the Stryker step of CI is commented out.

**Publishing:** triggered by pushing a semver tag (pattern `*.*.*`). The tag becomes the `PackageVersion`. The workflow publishes to both GitHub Packages and NuGet.org.

## Code Style

Enforced via `.editorconfig`, `dotnet format --verify-no-changes` and `dotnet csharpier check .` in CI:

- No `var` — always use explicit types
- No expression-bodied methods or constructors — use block bodies
- Expression-bodied properties and accessors are required
- File-scoped namespace declarations (`namespace Foo.Bar;`)
- Private fields prefixed with `_` in camelCase (e.g., `_options`)
- Braces always required, even for single-statement blocks
- Max line length: 90 characters
- Allman brace style — opening brace on its own line for all blocks
- `System.*` using directives sorted first, no blank line between using groups
- Parentheses required in arithmetic and relational binary operators for clarity

## Commit Messages

Do not mention Claude or AI assistance in commit messages.
