using CommandLine;
using ConsoleTables;
using DinoClipper.Cli;

Parser.Default.ParseArguments<GetLatestChannelOptions, GetChannelsOptions, DeleteClipOptions>(args)
    .WithParsed<GetLatestChannelOptions>(o =>
    {
        var clipRepository = CommandHandling.GetClipRepository(o);
        var newestClip = clipRepository.GetDateOfNewestClip($"{o.ChannelId}");
        if (newestClip.HasValue)
        {
            Console.Out.WriteLine($"Newest clip: {newestClip.Value}");
        }
        else
        {
            Console.Out.WriteLine($"No clips found for channel {o.ChannelId}");
        }
    })
    .WithParsed<GetChannelsOptions>(o =>
    {
        var clipRepository = CommandHandling.GetClipRepository(o);
        var clips = clipRepository.All
            .Where(c => c.Broadcaster.Id == $"{o.ChannelId}")
            .OrderBy(c => c.CreatedAt)
            .ToArray();
        
        Console.Out.WriteLine($"Found {clips.Length} clips for channel {o.ChannelId}");

        if (clips.Length == 0)
        {
            return;
        }

        var table = new ConsoleTable(
            "Id",
            "Title",
            "Broadcaster",
            "Creator",
            "Game",
            "Duration",
            "Created At")
        {
            Options =
            {
                NumberAlignment = Alignment.Right
            }
        };

        foreach (var clip in clips)
        {
            table.AddRow(
                clip.Id,
                clip.Name,
                clip.Broadcaster?.Name ?? "-",
                clip.Creator?.Name ?? "-",
                clip.Game?.Name ?? "-",
                clip.Duration,
                clip.CreatedAt);
        }

        table.Write(Format.Minimal);
    })
    .WithParsed<DeleteClipOptions>(o =>
    {
        var clipRepository = CommandHandling.GetClipRepository(o);
        var clip = clipRepository.All
            .FirstOrDefault(c => c.Id == o.ClipId && c.Broadcaster.Id == $"{o.ChannelId}");

        if (clip == null)
        {
            Console.Error.WriteLine($"Clip {o.ClipId} not found for channel {o.ChannelId}");
            Environment.Exit(0x20);
            return;
        }
        
        clipRepository.Delete(clip);
        Console.Out.WriteLine($"Deleted clip {o.ClipId} for channel {o.ChannelId}");
    });
