using System;

namespace DinoClipper.Exceptions
{
    public class DinoClipperException : Exception
    {
        public DinoClipperException(
            string message,
            int? exitCode,
            Exception innerException = null)
            : base(message, innerException)
        {
            ExitCode = exitCode ?? 0xdead;
        }
        
        public int ExitCode { get; }
    }

    public class InitializationException : DinoClipperException
    {
        private const int EXIT_CODE = 0x010;
        
        public InitializationException(string message, Exception innerException = null)
            : base(message, EXIT_CODE, innerException)
        {
        }
    }
}