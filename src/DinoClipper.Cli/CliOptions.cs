using CommandLine;

namespace DinoClipper.Cli;

public abstract class BaseCliOptions
{
    [Option('f', "db-file", Required = true, HelpText = "Path to the database file.")]
    public string DatabaseFile { get; set; } = string.Empty;
}

public abstract class ChannelScopedCommandOptions : BaseCliOptions
{
    [Option('c', "channel-id", Required = true, HelpText = "The channel ID to get the clips for.")]
    public long ChannelId { get; set; }
}

[Verb("get-latest-clip", HelpText = "Get the latest clip for a channel.")]
public class GetLatestChannelOptions : ChannelScopedCommandOptions;

[Verb("get-clips", HelpText = "Get all clips for a channel.")]
public class GetChannelsOptions : ChannelScopedCommandOptions;
