using System;
using PandaDotNet.Repo;

namespace DinoClipper.Storage
{
    public class Clip : IEntity<string>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public User Broadcaster { get; set; }
        public User Creator { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Views { get; set; }
        public string Language { get; set; }
        public float Duration { get; set; }
    }
}