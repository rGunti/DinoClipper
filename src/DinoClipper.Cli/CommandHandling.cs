using DinoClipper.Storage;
using LiteDB;

namespace DinoClipper.Cli;

public static class CommandHandling
{
    public static ILiteDatabase GetLiteDatabase(BaseCliOptions o)
    {
        if (o.DatabaseFile == string.Empty)
        {
            Console.Error.WriteLine("Please specify a database file.");
            Environment.Exit(0x10);
            return null;
        }

        var fullPath = Path.GetFullPath(o.DatabaseFile);
        if (!File.Exists(fullPath))
        {
            Console.Error.WriteLine($"Database file \"{fullPath}\" does not exist.");
            Environment.Exit(0x11);
            return null;
        }
        
        return new LiteDatabase(fullPath);
    }

    public static IClipRepository GetClipRepository(BaseCliOptions o)
    {
        return new ClipRepository(GetLiteDatabase(o));
    }
}