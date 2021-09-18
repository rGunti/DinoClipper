using System;
using PandaDotNet.Repo;

namespace DinoClipper.Storage
{
    public interface IClipRepository : IRepository<Clip, string>
    {
        DateTime? GetDateOfNewestClip(string channelId);
    }
}