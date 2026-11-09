using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;

namespace PureQL.CSharp.Model.Graph.Neo4j.Creation;

/// <summary>
/// The schema of stored PureQL query graphs, created in a database: a session opened on
/// the store's driver against its database runs the
/// <see cref="SchemaCreationStatements"/> in order, each in its own auto-commit
/// transaction.
/// </summary>
/// <param name="Store">The Neo4j database holding the query graphs.</param>
/// <remarks>
/// Applying is idempotent: a constraint or an index that already exists is left as it
/// is. The session is closed once the statements have run; the driver is not.
/// </remarks>
public sealed record CreatedSchemaOf(INeo4jStoredQueries Store)
{
    /// <summary>
    /// Opens a session on <see cref="INeo4jStoredQueries.Driver"/> against
    /// <see cref="INeo4jStoredQueries.Database"/> and creates the schema in it.
    /// </summary>
    /// <param name="ct">
    /// Cancels the remaining statements; it is checked before each statement.
    /// </param>
    /// <returns>A task that completes once every statement has run.</returns>
    public async Task ApplyAsync(CancellationToken ct = default)
    {
        IAsyncSession session = Store.Driver.AsyncSession(builder =>
            builder.WithDatabase(Store.Database)
        );
        await using (session.ConfigureAwait(false))
        {
            await new CreatedSchema(session).ApplyAsync(ct).ConfigureAwait(false);
        }
    }
}
