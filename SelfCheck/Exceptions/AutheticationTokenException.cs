using System;
using System.Runtime.Serialization;

namespace SampleSite.Controllers
{
    [Serializable]
    internal class AutheticationTokenException : Exception
    {
        public AutheticationTokenException()
        {
        }

        public AutheticationTokenException(string message) : base(message)
        {
        }

        public AutheticationTokenException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AutheticationTokenException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}