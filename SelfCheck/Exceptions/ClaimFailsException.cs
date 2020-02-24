using System;

namespace SampleSite.Controllers
{
    internal class ClaimFailsException : Exception
    {
        public ClaimFailsException(string claimsFailed)
        {
            throw new NotImplementedException();
        }
    }
}