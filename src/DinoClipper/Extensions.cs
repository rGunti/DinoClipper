using System;
using System.IO;
using System.Linq;
using DinoClipper.Config;
using Microsoft.Extensions.DependencyInjection;
using PandaDotNet.Utils;

namespace DinoClipper
{
    internal static class Extensions
    {
        public static DinoClipperConfiguration GetAppConfig(this IServiceProvider provider)
            => provider.GetRequiredService<DinoClipperConfiguration>();

        private static char[] unsafeCharacters = Path.GetInvalidPathChars()
            .Concat(Path.GetInvalidFileNameChars())
            .Distinct()
            .ToArray();

        public static string MakeSafe(this string str, string replaceWith = "")
        {
            str.OrThrowNullArg(nameof(str));
            if (replaceWith.Any(c => unsafeCharacters.Contains(c)))
            {
                throw new ArgumentException("Replacement string cannot contain unsafe characters", nameof(replaceWith));
            }

            return unsafeCharacters
                .Aggregate(str, (current, c) => current.Replace(c.ToString(), replaceWith));
        }
    }
}