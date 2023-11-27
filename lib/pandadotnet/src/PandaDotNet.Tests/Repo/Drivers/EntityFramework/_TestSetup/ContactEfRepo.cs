using PandaDotNet.Repo.Drivers.EntityFramework;
using PandaDotNet.Tests.Repo.Drivers._TestSetup;

namespace PandaDotNet.Tests.Repo.Drivers.EntityFramework._TestSetup
{
    public class ContactEfRepo :
        EntityFrameworkRepository<Contact, string, TestDbContext>,
        IContactRepository
    {
        public ContactEfRepo(TestDbContext dbContext) : base(dbContext)
        {
        }
    }
}