namespace DinoClipper.Config
{
    public class DinoClipperConfiguration
    {
        public TwitchConfig Twitch { get; set; }
        public string TempStorage { get; set; }
        public int MaxCacheAge { get; set; } = 8 * 60;
        public int SleepInterval { get; set; } = 300;
        public bool RestoreDateFilter { get; set; } = true;
        public bool RestoreCacheFromDatabase { get; set; } = true;
        public string FfmpegPath { get; set; }
        public string YouTubeDlPath { get; set; }
        public WebDavConfig UploadTarget { get; set; }
        public DownloaderFlags DownloaderFlags { get; set; } = new();
    }

    public class TwitchConfig
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ChannelId { get; set; }
    }

    public class WebDavConfig
    {
        public string Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class DownloaderFlags
    {
        public bool SkipInjectingTitle { get; set; }
        public bool SkipUpload { get; set; }
        public bool SkipClearingTempDirectory { get; set; }
    }
}