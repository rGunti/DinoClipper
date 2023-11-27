using System.Collections.Concurrent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PandaDotNet.Tests.Repo.Drivers._TestSetup;
using PandaDotNet.Tests.Repo.Drivers.Memory._TestSetup;

namespace PandaDotNet.Tests.Repo.Drivers.Memory
{
    [TestClass]
    public class MemoryRepositoryTests : BaseRepositoryTest
    {
        public MemoryRepositoryTests()
        {
            _repo = new ContactMemoryRepo();
        }

        protected override void SetupDatabaseWithRecords(params Contact[] contacts)
        {
            ConcurrentDictionary<string, Contact> db = ((ContactMemoryRepo) _repo).Database;
            foreach (Contact contact in contacts)
            {
                db.TryAdd(contact.Id, contact);
            }
        }
    }
}