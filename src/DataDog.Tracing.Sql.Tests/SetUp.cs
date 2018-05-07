using NUnit.Framework;
using SQLitePCL;

namespace DataDog.Tracing.Sql.Tests
{
    [SetUpFixture]
    class SetUp
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            raw.SetProvider(new SQLite3Provider_e_sqlite3());
        }
    }
}
