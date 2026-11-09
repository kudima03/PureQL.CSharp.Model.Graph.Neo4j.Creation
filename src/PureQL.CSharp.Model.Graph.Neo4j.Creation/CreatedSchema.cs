using Neo4j.Driver;

namespace PureQL.CSharp.Model.Graph.Neo4j.Creation;

/// <summary>
/// The schema of stored PureQL query graphs, created on a query runner: the
/// <see cref="SchemaCreationStatements"/> run in order.
/// </summary>
/// <param name="Runner">
/// The session or transaction to run the statements on. Neo4j does not run schema
/// statements in a transaction that also writes data, so a transaction passed here must
/// hold schema statements only.
/// </param>
/// <remarks>
/// Applying is idempotent: a constraint or an index that already exists is left as it
/// is. The record does not own <paramref name="Runner"/>.
/// </remarks>
public sealed record CreatedSchema(IAsyncQueryRunner Runner)
{
    /// <summary>
    /// Runs the <see cref="SchemaCreationStatements"/> in order on
    /// <see cref="Runner"/>, consuming the result of each before the next.
    /// </summary>
    /// <param name="ct">
    /// Cancels the remaining statements; it is checked before each statement, since the
    /// driver does not cancel a running one.
    /// </param>
    /// <returns>A task that completes once every statement has run.</returns>
    public async Task ApplyAsync(CancellationToken ct = default)
    {
        foreach (global::Neo4j.Driver.Query statement in new SchemaCreationStatements())
        {
            ct.ThrowIfCancellationRequested();
            IResultCursor cursor = await Runner.RunAsync(statement).ConfigureAwait(false);
            _ = await cursor.ConsumeAsync().ConfigureAwait(false);
        }
    }
}
