using System.Collections.Generic;
using System.Data;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using NUnit.Framework;

namespace DataDog.Tracing.Sql.Tests.EntityFrameworkCore
{
    [TestFixture]
    public class TraceDbCommandTests
    {
        RootSpan _root;
        SqliteConnection _conn;

        [SetUp]
        public void SetUp()
        {
            _conn = new SqliteConnection("Filename=./test.db");
            _conn.Open();
            _root = new RootSpan();
        }

        [Test]
        [TestCase(CommandBehavior.Default)]
        [TestCase(CommandBehavior.CloseConnection)]
        public void ExecuteReader_is_traced(CommandBehavior commandBehavior)
        {
            var customers = new List<Customer>();
            using (var command = new Sql.EntityFrameworkCore.TraceDbCommand(_conn.CreateCommand(), _root))
            {
                command.CommandText = "SELECT * FROM Customers";
                command.CommandType = CommandType.Text;
                using (var reader = command.ExecuteReader(commandBehavior))
                {
                    while (reader.Read())
                    {
                        customers.Add(new Customer(reader));
                    }
                }
            }
            customers.Count.Should().Be(2);
            _root.Spans[1].Name.Should().Be("sql." + nameof(IDbCommand.ExecuteReader));
            _root.Spans[1].Service.Should().Be("sql");
            _root.Spans[1].Resource.Should().Be("main");
            _root.Spans[1].Type.Should().Be("sql");
            _root.Spans[1].Error.Should().Be(0);
            _root.Spans[1].Meta["sql.CommandBehavior"].Should().Be(commandBehavior.ToString("x"));
            _root.Spans[1].Meta["sql.CommandText"].Should().Be("SELECT * FROM Customers");
            _root.Spans[1].Meta["sql.CommandType"].Should().Be("Text");
        }

        [Test]
        public void ExecuteNonQuery_is_traced()
        {
            int rows;
            using (var command = new Sql.EntityFrameworkCore.TraceDbCommand(_conn.CreateCommand(), _root))
            {
                command.CommandText = "SELECT * FROM Customers";
                command.CommandType = CommandType.Text;
                rows = command.ExecuteNonQuery();
            }
            _root.Spans[1].Name.Should().Be("sql." + nameof(IDbCommand.ExecuteNonQuery));
            _root.Spans[1].Service.Should().Be("sql");
            _root.Spans[1].Resource.Should().Be("main");
            _root.Spans[1].Type.Should().Be("sql");
            _root.Spans[1].Error.Should().Be(0);
            _root.Spans[1].Meta["sql.RowsAffected"].Should().Be(rows.ToString());
            _root.Spans[1].Meta["sql.CommandText"].Should().Be("SELECT * FROM Customers");
            _root.Spans[1].Meta["sql.CommandType"].Should().Be("Text");
        }

        [Test]
        public void ExecuteScalar_is_traced()
        {
            object result;
            using (var command = new Sql.EntityFrameworkCore.TraceDbCommand(_conn.CreateCommand(), _root))
            {
                command.CommandText = "SELECT COUNT(*) FROM Customers";
                command.CommandType = CommandType.Text;
                result = command.ExecuteScalar();
            }
            result.Should().Be(2L);
            _root.Spans[1].Name.Should().Be("sql." + nameof(IDbCommand.ExecuteScalar));
            _root.Spans[1].Service.Should().Be("sql");
            _root.Spans[1].Resource.Should().Be("main");
            _root.Spans[1].Type.Should().Be("sql");
            _root.Spans[1].Error.Should().Be(0);
            _root.Spans[1].Meta["sql.CommandText"].Should().Be("SELECT COUNT(*) FROM Customers");
            _root.Spans[1].Meta["sql.CommandType"].Should().Be("Text");
        }

        [TearDown]
        public void TearDown()
        {
            _conn.Close();
        }
    }
}
