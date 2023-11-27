using System.IO;
using LiteDB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PandaDotNet.Tests.Repo.Drivers._TestSetup;
using PandaDotNet.Tests.Repo.Drivers.LiteDB._TestSetup;

namespace PandaDotNet.Tests.Repo.Drivers.LiteDB
{
    [TestClass]
    public class LiteDbRepositoryTests : BaseRepositoryTest
    {
        private readonly ILiteDatabase _database;

        public LiteDbRepositoryTests()
        {
            _database = new LiteDatabase(new MemoryStream());
            _repo = new ContactLiteDbRepo(_database);
        }

        protected override void SetupDatabaseWithRecords(params Contact[] contacts)
        {
            _database.GetCollection<Contact>()
                .InsertBulk(contacts);
        }
    }
}