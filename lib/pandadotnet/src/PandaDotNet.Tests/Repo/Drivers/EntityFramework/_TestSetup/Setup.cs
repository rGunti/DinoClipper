using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace PandaDotNet.Tests.Repo.Drivers.EntityFramework._TestSetup
{
    public static class Setup
    {
        public static DbConnection CreateDbConnection()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            return connection;
        }
    }
}