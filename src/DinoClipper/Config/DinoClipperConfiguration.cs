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
        public TitleInjectionConfig TitleInjection { get; set; } = new();
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

    public class TitleInjectionConfig
    {
        public bool Enabled { get; set; } = true;
        public AvatarSettings Avatar { get; set; } = new();

        public class AvatarSettings
        {
            public bool Add { get; set; } = false;
            public string Source { get; set; }
        }
    }

    public class DownloaderFlags
    {
        public int MaxWorkerThreads { get; set; } = 1;
        public bool UploadOriginal { get; set; }
        public bool SkipUpload { get; set; }
        public bool SkipClearingTempDirectory { get; set; }
    }
}