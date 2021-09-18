namespace DinoClipper.Config
{
    public class DinoClipperConfiguration
    {
        public TwitchConfig Twitch { get; set; }
        public string TempStorage { get; set; }
        public int MaxCacheAge { get; set; } = 8 * 60;
        public int SleepInterval { get; set; } = 300;
    }

    public class TwitchConfig
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ChannelId { get; set; }
    }
}