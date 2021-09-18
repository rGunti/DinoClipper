using System;
using DinoClipper.Config;
using Microsoft.Extensions.DependencyInjection;

namespace DinoClipper
{
    internal static class Extensions
    {
        public static DinoClipperConfiguration GetAppConfig(this IServiceProvider provider)
            => provider.GetRequiredService<DinoClipperConfiguration>();
    }
}