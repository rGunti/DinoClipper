using LiteDB;
using PandaDotNet.Repo.Drivers.LiteDB;

namespace DinoClipper.Storage
{
    public class ClipRepository : LiteDbRepository<Clip, string>, IClipRepository
    {
        public ClipRepository(ILiteDatabase database) : base(database)
        {
        }
    }
}