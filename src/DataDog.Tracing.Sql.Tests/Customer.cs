using System.Data;

namespace DataDog.Tracing.Sql.Tests
{
    public class Customer
    {
        public Customer(IDataRecord record)
        {
            Id = record.GetInt32(0);
            FirstName = record.GetString(1);
            LastName = record.GetString(2);
        }

        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
