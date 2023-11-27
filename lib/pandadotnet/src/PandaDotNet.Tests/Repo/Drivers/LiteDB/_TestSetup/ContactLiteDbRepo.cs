using LiteDB;
using PandaDotNet.Repo.Drivers.LiteDB;
using PandaDotNet.Tests.Repo.Drivers._TestSetup;

namespace PandaDotNet.Tests.Repo.Drivers.LiteDB._TestSetup
{
    public class ContactLiteDbRepo : LiteDbRepository<Contact, string>, IContactRepository
    {
        public ContactLiteDbRepo(ILiteDatabase database) : base(database)
        {
        }
    }
}