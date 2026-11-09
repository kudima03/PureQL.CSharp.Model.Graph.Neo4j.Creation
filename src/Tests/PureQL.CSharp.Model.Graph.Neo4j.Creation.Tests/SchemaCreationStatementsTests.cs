using System.Collections;

namespace PureQL.CSharp.Model.Graph.Neo4j.Creation.Tests;

public sealed record SchemaCreationStatementsTests
{
    [Fact]
    public void YieldsFourStatements()
    {
        Assert.Equal(4, new SchemaCreationStatements().Count());
    }

    [Fact]
    public void CreatesQueryIdConstraintFirst()
    {
        Assert.Equal(
            Statements.QueryId,
            new SchemaCreationStatements().ElementAt(0).Text
        );
    }

    [Fact]
    public void CreatesNodeQueryIdIndexSecond()
    {
        Assert.Equal(
            Statements.NodeQueryId,
            new SchemaCreationStatements().ElementAt(1).Text
        );
    }

    [Fact]
    public void CreatesNodeFieldSourceIndexThird()
    {
        Assert.Equal(
            Statements.NodeFieldSource,
            new SchemaCreationStatements().ElementAt(2).Text
        );
    }

    [Fact]
    public void CreatesNodeParamIndexFourth()
    {
        Assert.Equal(
            Statements.NodeParam,
            new SchemaCreationStatements().ElementAt(3).Text
        );
    }

    [Fact]
    public void NamesStatementsInSchemaNamesOrder()
    {
        Assert.Equal(
            [
                SchemaNames.QueryId,
                SchemaNames.NodeQueryId,
                SchemaNames.NodeFieldSource,
                SchemaNames.NodeParam,
            ],
            new SchemaCreationStatements().Select(statement =>
                statement.Text.Split(' ')[2]
            )
        );
    }

    [Fact]
    public void YieldsStatementsWithoutParameters()
    {
        Assert.All(
            new SchemaCreationStatements(),
            statement => Assert.Empty(statement.Parameters)
        );
    }

    [Fact]
    public void YieldsOnlyIdempotentStatements()
    {
        Assert.All(
            new SchemaCreationStatements(),
            statement => Assert.Contains(" IF NOT EXISTS ", statement.Text)
        );
    }

    [Fact]
    public void YieldsSameStatementsOnEveryEnumeration()
    {
        SchemaCreationStatements statements = new SchemaCreationStatements();
        Assert.Equal(
            statements.Select(statement => statement.Text),
            statements.Select(statement => statement.Text)
        );
    }

    [Fact]
    public void YieldsStatementsThroughNonGenericEnumerable()
    {
        List<string> texts = [];
        IEnumerator enumerator = (
            (IEnumerable)new SchemaCreationStatements()
        ).GetEnumerator();
        while (enumerator.MoveNext())
        {
            texts.Add(((global::Neo4j.Driver.Query)enumerator.Current).Text);
        }
        Assert.Equal(Statements.All, texts);
    }

    [Fact]
    public void EqualsAnotherInstance()
    {
        Assert.Equal(new SchemaCreationStatements(), new SchemaCreationStatements());
    }
}
