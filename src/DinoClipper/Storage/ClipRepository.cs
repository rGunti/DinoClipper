using System;
using System.Linq;
using LiteDB;
using PandaDotNet.Repo.Drivers.LiteDB;

namespace DinoClipper.Storage
{
    public class ClipRepository : LiteDbRepository<Clip, string>, IClipRepository
    {
        public ClipRepository(ILiteDatabase database) : base(database)
        {
        }

        public DateTime? GetDateOfNewestClip(string channelId)
        {
            try
            {
                return All
                    .Where(c => c.Broadcaster.Id == channelId)
                    .Max(c => c.CreatedAt);
            }
            catch (InvalidOperationException)
            {
                // Sequence contains no elements -> return null
                return null;
            }
        }
    }
}