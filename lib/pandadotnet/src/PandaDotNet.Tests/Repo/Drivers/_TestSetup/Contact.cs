using PandaDotNet.Repo;

namespace PandaDotNet.Tests.Repo.Drivers._TestSetup
{
    public class Contact : IEntity<string>
    {
        public string Id { get; set; }
        
        public string Name { get; set; }
        public string Surname { get; set; }
    }
}