using PandaDotNet.Repo;

namespace DinoClipper.Storage
{
    public class Game : IEntity<string>
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}