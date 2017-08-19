using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using NUnit.Framework;

namespace DataDog.Tracing.Sql.Tests
{
    [TestFixture]
    public class TraceDbConnectionTests
    {
        RootSpan _root;
        SqliteConnection _conn;

        [SetUp]
        public void SetUp()
        {
            _root = new RootSpan();
            _conn = new SqliteConnection("Filename=./test.db");
        }

        [TearDown]
        public void TearDown()
        {
            _conn.Dispose();
        }

        [Test]
        public void Open_should_be_traced()
        {
            var conn = new TraceDbConnection(_conn, _root);
            _root.Spans.Count.Should().Be(1);
            conn.Open();
            _root.Spans.Count.Should().Be(2);
            var s = _root.Spans[1];
            s.Error.Should().Be(0);
            s.Name.Should().Be("sql.connect");
            s.Service.Should().Be("sql");
            s.Resource.Should().Be("main");
            s.Type.Should().Be("sql");
        }
    }
}
