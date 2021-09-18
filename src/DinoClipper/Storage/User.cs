using PandaDotNet.Repo;

namespace DinoClipper.Storage
{
    public class User : IEntity<string>
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}