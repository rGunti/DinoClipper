using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DinoClipper.Config;
using Microsoft.Extensions.DependencyInjection;
using PandaDotNet.Utils;
using Xabe.FFmpeg;

namespace DinoClipper
{
    internal static class Extensions
    {
        public static DinoClipperConfiguration GetAppConfig(this IServiceProvider provider)
            => provider.GetRequiredService<DinoClipperConfiguration>();

        private static char[] unsafeCharacters = Path.GetInvalidPathChars()
            .Concat(Path.GetInvalidFileNameChars())
            .Concat(new [] { '|', ',', '*', '!', '<', '>', '[', ']', '(', ')', '?' })
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

        public static T RunSync<T>(this Task<T> task)
        {
            return task.GetAwaiter().GetResult();
        }

        public static T RunSafeSync<T>(this Task<T> task, int idleInterval = 500)
        {
            while (!task.IsCompleted)
            {
                Thread.Sleep(idleInterval);
            }

            if (task.Exception != null)
            {
                throw task.Exception;
            }

            return task.Result;
        }

        public static string JoinToString(this IEnumerable<string> strings, string joinSeq = "")
            => string.Join(joinSeq, strings);
    }
}