using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PandaDotNet.Tests.Repo.Drivers._TestSetup;
using PandaDotNet.Tests.Repo.Drivers.EntityFramework._TestSetup;

namespace PandaDotNet.Tests.Repo.Drivers.EntityFramework
{
    [TestClass]
    public class EntityFrameworkRepositoryTests : BaseRepositoryTest
    {
        private readonly TestDbContext _dbContext;

        public EntityFrameworkRepositoryTests()
        {
            _dbContext = TestDbContext.Create();
            _repo = new ContactEfRepo(_dbContext);
        }

        protected override void SetupDatabaseWithRecords(params Contact[] contacts)
        {
            _dbContext.Contacts.AddRange(contacts);
            _dbContext.SaveChanges();
        }
    }
}