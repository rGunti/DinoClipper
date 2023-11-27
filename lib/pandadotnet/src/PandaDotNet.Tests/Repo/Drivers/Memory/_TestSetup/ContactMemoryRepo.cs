using System.Collections.Generic;
using PandaDotNet.Repo.Drivers.Memory;
using PandaDotNet.Tests.Repo.Drivers._TestSetup;

namespace PandaDotNet.Tests.Repo.Drivers.Memory._TestSetup
{
    public class ContactMemoryRepo : MemoryRepository<Contact, string>, IContactRepository
    {
        public ContactMemoryRepo(IEnumerable<Contact> entities = null) : base(entities)
        {
        }
    }
}